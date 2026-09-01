import asyncio
import json
import os
import unittest
from pathlib import Path
from types import SimpleNamespace
from unittest.mock import AsyncMock, MagicMock, Mock, patch

import mysql.connector
import requests
from fastapi import HTTPException
from pydantic import ValidationError
from starlette.requests import Request
from starlette.responses import JSONResponse

os.environ.setdefault("SCHEDULE_DB_USER", "test")
os.environ.setdefault("SCHEDULE_DB_PASSWORD", "test")
os.environ.setdefault("DEBT_DB_USER", "test")
os.environ.setdefault("DEBT_DB_PASSWORD", "test")

from apps.database.queries.debt_queries import (
    get_credit_book_owners_query as database_get_credit_book_owners_query,
)
from apps.database.queries.debt_queries import (
    get_debts_by_credit_book_query as database_get_debts_by_credit_book_query,
)
from apps.database.queries.user_queries import (
    get_subgroups as database_get_subgroups,
)
from apps.database.queries.user_queries import (
    get_user_info_query as database_get_user_info_query,
)
from apps.database.settings import DBSettings, MailSettings, ParsingSettings, mail_config
from apps.database.utils import execute_query
from apps.gateway.errors import (
    DatabaseUnavailableError,
    MoodleInvalidCredentialsError,
    MoodleInvalidTokenError,
    MoodleServiceError,
    UpstreamResponseError,
)
from apps.gateway.main import app, health, readiness, upstream_response_handler
from apps.gateway.middlewares import AuthMiddleware
from apps.gateway.models.debt import DebtDTO, SemesterDTO
from apps.gateway.models.schedule import ScheduleDTO
from apps.gateway.models.users import (
    StudentFormRequest,
    UserAuth,
    UserInfo,
    UserNotification,
)
from apps.gateway.routers.api import (
    auth,
    auth_ip_rate_limiter,
    auth_username_rate_limiter,
    clear_rate_limits,
    get_debts,
    get_notifications,
    get_semester,
    get_session,
    get_user_info,
    send_order,
)
from apps.gateway.utils.http_client import create_http_session
from apps.gateway.utils.mail_sender import mail_sender
from apps.gateway.utils.moodle import moodle_requests
from apps.gateway.utils.pair_number_converter import convert_pair_number
from apps.gateway.utils.rate_limit import SlidingWindowRateLimiter
from apps.gateway.validation import (
    ValidationErrors,
    clear_validation_cache,
    validate_credentials,
)


def make_request(
    path: str,
    *,
    username: str = "student",
    token: str = "token",
    client: str = "127.0.0.1",
) -> Request:
    request = Request(
        {
            "type": "http",
            "http_version": "1.1",
            "method": "POST",
            "scheme": "https",
            "path": path,
            "raw_path": path.encode(),
            "query_string": b"",
            "headers": [],
            "client": (client, 12345),
            "server": ("testserver", 443),
        }
    )
    request.state.auth_username = username
    request.state.auth_token = token
    return request


class PairNumberConverterTests(unittest.TestCase):
    def test_known_pair(self):
        pair = convert_pair_number(3)
        self.assertEqual("12:40", pair.start_date)
        self.assertEqual("14:15", pair.end_date)

    def test_unknown_pair(self):
        with self.assertRaises(ValueError):
            convert_pair_number(0)


class CredentialValidationTests(unittest.TestCase):
    def setUp(self):
        clear_validation_cache()

    def test_missing_and_oversized_headers_are_rejected_locally(self):
        self.assertEqual(
            ValidationErrors.EMPTY_REQUIRED_HEADERS,
            validate_credentials(None, None),
        )
        self.assertEqual(
            ValidationErrors.EMPTY_REQUIRED_HEADERS,
            validate_credentials("x" * 4097, "student"),
        )

    @patch("apps.gateway.validation._http.post")
    def test_valid_credentials_use_post_body_not_url_query(self, request_post):
        response = Mock()
        response.json.return_value = {"username": "student"}
        response.raise_for_status.return_value = None
        request_post.return_value = response

        self.assertEqual(
            ValidationErrors.NO_ERRORS,
            validate_credentials("token", "student"),
        )
        kwargs = request_post.call_args.kwargs
        self.assertEqual("token", kwargs["data"]["wstoken"])
        self.assertNotIn("params", kwargs)
        self.assertNotIn("token", kwargs["url"])

    @patch("apps.gateway.validation._http.post")
    def test_token_for_another_user(self, request_post):
        response = Mock()
        response.json.return_value = {"username": "another"}
        response.raise_for_status.return_value = None
        request_post.return_value = response

        self.assertEqual(
            ValidationErrors.WRONG_TOKEN,
            validate_credentials("token", "student"),
        )

    @patch("apps.gateway.validation._http.post")
    def test_successful_validation_is_cached(self, request_post):
        response = Mock()
        response.json.return_value = {"username": "student"}
        response.raise_for_status.return_value = None
        request_post.return_value = response

        self.assertEqual(ValidationErrors.NO_ERRORS, validate_credentials("token", "student"))
        self.assertEqual(ValidationErrors.NO_ERRORS, validate_credentials("token", "student"))
        self.assertEqual(1, request_post.call_count)

    @patch("apps.gateway.validation._http.post", side_effect=requests.ConnectionError)
    def test_auth_service_outage_is_not_reported_as_invalid_token(self, _request_post):
        self.assertEqual(
            ValidationErrors.SERVICE_UNAVAILABLE,
            validate_credentials("token", "student"),
        )

    @patch("apps.gateway.validation._http.post")
    def test_malformed_auth_payload_is_dependency_outage(self, request_post):
        response = Mock()
        response.json.return_value = []
        response.raise_for_status.return_value = None
        request_post.return_value = response

        self.assertEqual(
            ValidationErrors.SERVICE_UNAVAILABLE,
            validate_credentials("token", "student"),
        )

    @patch("apps.gateway.validation._http.post")
    def test_invalidtoken_is_rejected_but_internal_moodle_error_is_outage(self, request_post):
        response = Mock()
        response.raise_for_status.return_value = None
        request_post.return_value = response

        response.json.return_value = {
            "exception": "moodle_exception",
            "errorcode": "invalidtoken",
            "message": "upstream detail",
        }
        self.assertEqual(
            ValidationErrors.INVALID_TOKEN,
            validate_credentials("token", "student"),
        )

        response.json.return_value = {
            "exception": "dml_read_exception",
            "errorcode": "dmlreadexception",
            "message": "sensitive upstream detail",
        }
        self.assertEqual(
            ValidationErrors.SERVICE_UNAVAILABLE,
            validate_credentials("token", "student"),
        )


class ApiFlowTests(unittest.TestCase):
    def setUp(self):
        clear_rate_limits()

    def test_unscoped_broadcast_websocket_is_not_exposed(self):
        paths = set(app.openapi()["paths"])
        self.assertNotIn("/mauverse/ws", paths)

    def test_mobile_http_contract_is_exposed(self):
        expected_routes = {
            "/dev/mauverse/auth": {"POST"},
            "/dev/mauverse/check_group": {"POST"},
            "/dev/mauverse/check_teacher": {"POST"},
            "/dev/mauverse/get_all_rooms": {"POST"},
            "/dev/mauverse/get_all_teachers": {"POST"},
            "/dev/mauverse/get_contacts_json": {"POST"},
            "/dev/mauverse/get_debts": {"POST"},
            "/dev/mauverse/get_depts_json": {"GET"},
            "/dev/mauverse/get_groups": {"POST"},
            "/dev/mauverse/get_notifications": {"POST"},
            "/dev/mauverse/get_rooms": {"POST"},
            "/dev/mauverse/get_schedule": {"POST"},
            "/dev/mauverse/get_semesters": {"POST"},
            "/dev/mauverse/get_subgroups": {"POST"},
            "/dev/mauverse/get_teacher_info": {"POST"},
            "/dev/mauverse/get_teachers": {"POST"},
            "/dev/mauverse/news": {"GET"},
            "/dev/mauverse/session": {"GET"},
            "/dev/mauverse/send_order": {"POST"},
        }
        actual_routes = {
            path: {method.upper() for method in operations}
            for path, operations in app.openapi()["paths"].items()
        }

        for path, methods in expected_routes.items():
            self.assertIn(path, actual_routes)
            self.assertTrue(methods.issubset(actual_routes[path]))

    def test_openapi_operation_ids_are_unique(self):
        operation_ids = [
            operation["operationId"]
            for path in app.openapi()["paths"].values()
            for operation in path.values()
            if "operationId" in operation
        ]

        self.assertEqual(len(operation_ids), len(set(operation_ids)))

    def test_health_is_public_and_has_security_headers(self):
        middleware = AuthMiddleware(app)
        call_next = AsyncMock(return_value=JSONResponse(asyncio.run(health())))
        response = asyncio.run(middleware.dispatch(make_request("/health"), call_next))

        self.assertEqual(200, response.status_code)
        self.assertEqual({"status": "ok"}, json.loads(response.body))
        self.assertEqual("no-store", response.headers["Cache-Control"])
        self.assertEqual("nosniff", response.headers["X-Content-Type-Options"])
        call_next.assert_awaited_once()

    def test_protected_route_rejects_missing_auth_headers(self):
        middleware = AuthMiddleware(app)
        call_next = AsyncMock(return_value=JSONResponse({"unexpected": True}))
        response = asyncio.run(
            middleware.dispatch(make_request("/dev/mauverse/check_group"), call_next)
        )

        self.assertEqual(401, response.status_code)
        self.assertIn("detail", json.loads(response.body))
        self.assertEqual("no-store", response.headers["Cache-Control"])
        call_next.assert_not_awaited()

    def test_invalid_or_mismatched_session_is_unauthorized(self):
        for validation_error in (ValidationErrors.INVALID_TOKEN, ValidationErrors.WRONG_TOKEN):
            with self.subTest(validation_error=validation_error):
                middleware = AuthMiddleware(app)
                call_next = AsyncMock(return_value=JSONResponse({"unexpected": True}))
                with patch(
                    "apps.gateway.middlewares.AuthMiddlewareValidation.user_request_validation",
                    return_value=validation_error,
                ):
                    response = asyncio.run(
                        middleware.dispatch(
                            make_request("/dev/mauverse/session"),
                            call_next,
                        )
                    )

                self.assertEqual(401, response.status_code)
                self.assertEqual("no-store", response.headers["Cache-Control"])
                call_next.assert_not_awaited()

    def test_session_returns_only_authenticated_identity(self):
        result = get_session(make_request("/dev/mauverse/session", username="authenticated-user"))

        self.assertEqual(
            {"authenticated": True, "username": "authenticated-user"},
            result,
        )

    @patch("apps.gateway.routers.api.get_user_info_query")
    def test_user_info_uses_authenticated_identity(self, get_info):
        get_info.return_value = {"username": "authenticated"}

        result = get_user_info(
            make_request("/dev/mauverse/get_user_info", username="authenticated"),
            UserInfo(username="requested-other-user"),
        )

        self.assertEqual({"username": "authenticated"}, result)
        get_info.assert_called_once_with("authenticated")

    @patch("apps.gateway.routers.api.get_user_info_query")
    @patch("apps.gateway.routers.api.moodle_requests.get_moodle_user")
    @patch("apps.gateway.routers.api.moodle_requests.get_token")
    def test_auth_returns_only_public_token(self, get_token, get_user, get_info):
        get_token.return_value = {"token": "public", "privatetoken": "must-not-leak"}
        get_user.return_value = {"username": "student", "userid": 7}
        get_info.return_value = {"roleid": 0, "groupname": "GROUP"}

        result = auth(
            UserAuth(username="student", password="password"),
            make_request("/dev/mauverse/auth"),
        )

        self.assertEqual("public", result["token"])
        self.assertNotIn("privatetoken", result)
        self.assertNotIn(
            "password='plaintext-secret'",
            repr(UserAuth(username="student", password="plaintext-secret")),
        )

    @patch(
        "apps.gateway.routers.api.moodle_requests.get_token",
        side_effect=MoodleInvalidCredentialsError,
    )
    def test_auth_rejects_invalid_credentials_with_401(self, _get_token):
        with self.assertRaises(HTTPException) as raised:
            auth(
                UserAuth(username="student", password="wrong-password"),
                make_request("/dev/mauverse/auth"),
            )

        self.assertEqual(401, raised.exception.status_code)

    @patch("apps.gateway.routers.api.get_user_info_query", return_value={})
    @patch("apps.gateway.routers.api.moodle_requests.get_moodle_user")
    @patch("apps.gateway.routers.api.moodle_requests.get_token")
    def test_auth_rejects_moodle_user_without_local_profile(self, get_token, get_user, _get_info):
        get_token.return_value = {"token": "token"}
        get_user.return_value = {"username": "moodle-only-user"}

        with self.assertRaises(HTTPException) as raised:
            auth(
                UserAuth(username="moodle-only-user", password="password"),
                make_request("/dev/mauverse/auth"),
            )

        self.assertEqual(403, raised.exception.status_code)

    @patch(
        "apps.gateway.routers.api.moodle_requests.get_token",
        side_effect=MoodleServiceError("sensitive upstream detail"),
    )
    def test_auth_maps_internal_moodle_failure_to_safe_503(self, _get_token):
        with self.assertRaises(HTTPException) as raised:
            auth(
                UserAuth(username="student", password="password"),
                make_request("/dev/mauverse/auth"),
            )

        self.assertEqual(503, raised.exception.status_code)
        self.assertNotIn("sensitive", raised.exception.detail)

    @patch(
        "apps.gateway.routers.api.moodle_requests.get_token",
        side_effect=MoodleInvalidCredentialsError,
    )
    def test_auth_counts_ip_and_normalized_username_independently(self, _get_token):
        with (
            patch.object(auth_ip_rate_limiter, "acquire", return_value=None) as ip_acquire,
            patch.object(
                auth_username_rate_limiter,
                "acquire",
                return_value=None,
            ) as username_acquire,
            self.assertRaises(HTTPException),
        ):
            auth(
                UserAuth(username="ＳtUdEnT", password="password"),
                make_request("/dev/mauverse/auth", client="192.0.2.10"),
            )

        ip_acquire.assert_called_once_with("192.0.2.10")
        username_acquire.assert_called_once_with("student")

    @patch("apps.gateway.routers.api.moodle_requests.get_token")
    def test_auth_records_username_limit_even_when_ip_limit_is_exhausted(self, get_token):
        with (
            patch.object(auth_ip_rate_limiter, "acquire", return_value=9) as ip_acquire,
            patch.object(
                auth_username_rate_limiter,
                "acquire",
                return_value=None,
            ) as username_acquire,
            self.assertRaises(HTTPException) as raised,
        ):
            auth(
                UserAuth(username="Student", password="password"),
                make_request("/dev/mauverse/auth", client="192.0.2.11"),
            )

        self.assertEqual(429, raised.exception.status_code)
        ip_acquire.assert_called_once_with("192.0.2.11")
        username_acquire.assert_called_once_with("student")
        get_token.assert_not_called()

    @patch(
        "apps.gateway.routers.api.get_user_info_query",
        side_effect=DatabaseUnavailableError,
    )
    @patch("apps.gateway.routers.api.moodle_requests.get_moodle_user")
    @patch("apps.gateway.routers.api.moodle_requests.get_token")
    def test_auth_does_not_hide_profile_database_failure(self, get_token, get_user, _get_info):
        get_token.return_value = {"token": "token"}
        get_user.return_value = {"username": "student"}

        with self.assertRaises(DatabaseUnavailableError):
            auth(
                UserAuth(username="student", password="password"),
                make_request("/dev/mauverse/auth"),
            )

    @patch("apps.gateway.routers.api.moodle_requests.get_notifications")
    def test_notifications_reject_body_token_substitution(self, get_notifications_mock):
        with self.assertRaises(HTTPException) as raised:
            get_notifications(
                UserNotification(token="other-token", user_id=7),
                make_request("/dev/mauverse/get_notifications", token="header-token"),
            )

        self.assertEqual(403, raised.exception.status_code)
        get_notifications_mock.assert_not_called()

    @patch("apps.gateway.routers.api.moodle_requests.get_notifications")
    @patch("apps.gateway.routers.api.moodle_requests.get_moodle_user")
    def test_notifications_reject_user_id_substitution(
        self, get_moodle_user_mock, get_notifications_mock
    ):
        get_moodle_user_mock.return_value = {"username": "student", "userid": 8}

        with self.assertRaises(HTTPException) as raised:
            get_notifications(
                UserNotification(token="header-token", user_id=7),
                make_request("/dev/mauverse/get_notifications", token="header-token"),
            )

        self.assertEqual(403, raised.exception.status_code)
        get_notifications_mock.assert_not_called()

    @patch("apps.gateway.routers.api.moodle_requests.get_notifications")
    @patch("apps.gateway.routers.api.moodle_requests.get_moodle_user")
    def test_notifications_accept_authenticated_user_id(
        self, get_moodle_user_mock, get_notifications_mock
    ):
        get_moodle_user_mock.return_value = {"username": "student", "userid": 7}
        get_notifications_mock.return_value = []

        result = get_notifications(
            UserNotification(token="header-token", user_id=7),
            make_request("/dev/mauverse/get_notifications", token="header-token"),
        )

        self.assertEqual([], result)
        get_notifications_mock.assert_called_once_with(token="header-token", user_id=7)

    @patch("apps.gateway.routers.api.get_total_debts_by_group_query")
    @patch("apps.gateway.routers.api.get_user_info_query")
    def test_teacher_can_only_read_curated_group(self, get_info, get_group_debts):
        get_info.return_value = {"roleid": 1, "groupname": "OWN-GROUP"}

        with self.assertRaises(HTTPException) as raised:
            get_debts(
                DebtDTO(group_name="OTHER-GROUP"),
                make_request("/dev/mauverse/get_debts", username="teacher"),
            )

        self.assertEqual(403, raised.exception.status_code)
        get_group_debts.assert_not_called()

    @patch("apps.gateway.routers.api.get_debts_by_credit_book_query")
    @patch("apps.gateway.routers.api.get_user_info_query")
    def test_teacher_cannot_use_student_credit_book_flow(self, get_info, get_debts_mock):
        get_info.return_value = {"roleid": 1, "groupname": "OWN-GROUP"}

        with self.assertRaises(HTTPException) as raised:
            get_debts(
                DebtDTO(credit_book="123", semester_number=1),
                make_request("/dev/mauverse/get_debts", username="teacher"),
            )

        self.assertEqual(403, raised.exception.status_code)
        get_debts_mock.assert_not_called()

    @patch("apps.gateway.routers.api.get_total_debts_by_group_query")
    @patch("apps.gateway.routers.api.get_user_info_query")
    def test_teacher_can_read_own_group(self, get_info, get_group_debts):
        get_info.return_value = {"roleid": 1, "groupname": "OWN-GROUP"}
        get_group_debts.return_value = [{"firstname": "Student"}]

        result = get_debts(
            DebtDTO(group_name="own-group"),
            make_request("/dev/mauverse/get_debts", username="teacher"),
        )

        self.assertEqual([{"firstname": "Student"}], result["students"])
        get_group_debts.assert_called_once_with("own-group")

    @patch("apps.gateway.routers.api.get_debts_by_credit_book_query")
    @patch("apps.gateway.routers.api.moodle_requests.get_moodle_user")
    @patch("apps.gateway.routers.api.get_credit_book_owners_query")
    @patch("apps.gateway.routers.api.get_user_info_query")
    def test_student_credit_book_access_fails_closed_for_ambiguous_owner(
        self, get_info, get_owners, get_moodle_user, get_debts_mock
    ):
        get_info.return_value = {
            "roleid": 0,
            "groupname": "OWN-GROUP",
            "username": "student",
        }
        get_owners.return_value = [{"student_id": 41}, {"student_id": 42}]

        with self.assertRaises(HTTPException) as raised:
            get_debts(
                DebtDTO(credit_book="someone-elses-book", semester_number=1),
                make_request("/dev/mauverse/get_debts", username="student"),
            )

        self.assertEqual(403, raised.exception.status_code)
        self.assertIn("не подтверждена", raised.exception.detail)
        get_moodle_user.assert_not_called()
        get_debts_mock.assert_not_called()

    @patch("apps.gateway.routers.api.get_semester_debts_by_credit_book_query")
    @patch("apps.gateway.routers.api.get_credit_book_owners_query", return_value=[])
    @patch("apps.gateway.routers.api.get_user_info_query")
    def test_student_semester_credit_book_access_fails_closed_without_owner(
        self, get_info, _get_owners, get_semesters_mock
    ):
        get_info.return_value = {
            "roleid": 0,
            "groupname": "OWN-GROUP",
            "username": "student",
        }

        with self.assertRaises(HTTPException) as raised:
            get_semester(
                SemesterDTO(credit_book="someone-elses-book"),
                make_request("/dev/mauverse/get_semesters", username="student"),
            )

        self.assertEqual(403, raised.exception.status_code)
        get_semesters_mock.assert_not_called()

    @patch("apps.gateway.routers.api.get_debts_by_credit_book_query")
    @patch("apps.gateway.routers.api.moodle_requests.get_moodle_user")
    @patch("apps.gateway.routers.api.get_credit_book_owners_query")
    @patch("apps.gateway.routers.api.get_user_info_query")
    def test_student_can_read_uniquely_owned_credit_book_without_trusting_body_identity(
        self, get_info, get_owners, get_moodle_user, get_debts_mock
    ):
        get_info.return_value = {
            "roleid": 0,
            "groupname": " ГРУППА-1 ",
            "username": "student",
        }
        get_owners.return_value = [
            {
                "student_id": 42,
                "surname": "Иванов",
                "name": "Иван",
                "middle_name": "Иванович",
                "study_group": "группа-1",
                "identity_matches": 1,
            }
        ]
        get_moodle_user.return_value = {
            "username": "Student",
            "firstname": "Иван Иванович",
            "lastname": "Иванов",
            "fullname": "Иванов Иван Иванович",
        }
        get_debts_mock.return_value = [{"discipline": "Math"}]

        result = get_debts(
            DebtDTO(
                credit_book="owned-book",
                semester_number=2,
                group_name="ATTACKER-GROUP",
                first_name="Чужая",
                name="Личность",
                last_name="Из-Тела",
            ),
            make_request(
                "/dev/mauverse/get_debts",
                username="Student",
                token="validated-token",
            ),
        )

        self.assertEqual([{"discipline": "Math"}], result["debts"])
        get_moodle_user.assert_called_once_with("validated-token")
        get_debts_mock.assert_called_once_with(2, "owned-book", 42)

    @patch("apps.gateway.routers.api.get_debts_by_credit_book_query")
    @patch("apps.gateway.routers.api.moodle_requests.get_moodle_user")
    @patch("apps.gateway.routers.api.get_credit_book_owners_query")
    @patch("apps.gateway.routers.api.get_user_info_query")
    def test_student_credit_book_access_fails_closed_for_group_mismatch(
        self, get_info, get_owners, get_moodle_user, get_debts_mock
    ):
        get_info.return_value = {
            "roleid": 0,
            "groupname": "LOCAL-GROUP",
            "username": "student",
        }
        get_owners.return_value = [
            {
                "student_id": 42,
                "surname": "Иванов",
                "name": "Иван",
                "middle_name": "Иванович",
                "study_group": "OTHER-GROUP",
                "identity_matches": 1,
            }
        ]

        with self.assertRaises(HTTPException) as raised:
            get_debts(
                DebtDTO(credit_book="owned-book", semester_number=2),
                make_request("/dev/mauverse/get_debts", username="student"),
            )

        self.assertEqual(403, raised.exception.status_code)
        get_moodle_user.assert_not_called()
        get_debts_mock.assert_not_called()

    @patch("apps.gateway.routers.api.get_debts_by_credit_book_query")
    @patch("apps.gateway.routers.api.moodle_requests.get_moodle_user")
    @patch("apps.gateway.routers.api.get_credit_book_owners_query")
    @patch("apps.gateway.routers.api.get_user_info_query")
    def test_student_credit_book_access_fails_closed_for_non_unique_db_identity(
        self, get_info, get_owners, get_moodle_user, get_debts_mock
    ):
        get_info.return_value = {
            "roleid": 0,
            "groupname": "GROUP-1",
            "username": "student",
        }
        get_owners.return_value = [
            {
                "student_id": 42,
                "surname": "Иванов",
                "name": "Иван",
                "middle_name": "Иванович",
                "study_group": "GROUP-1",
                "identity_matches": 2,
            }
        ]

        with self.assertRaises(HTTPException) as raised:
            get_debts(
                DebtDTO(credit_book="owned-book", semester_number=2),
                make_request("/dev/mauverse/get_debts", username="student"),
            )

        self.assertEqual(403, raised.exception.status_code)
        get_moodle_user.assert_not_called()
        get_debts_mock.assert_not_called()

    @patch("apps.gateway.routers.api.get_debts_by_credit_book_query")
    @patch("apps.gateway.routers.api.moodle_requests.get_moodle_user")
    @patch("apps.gateway.routers.api.get_credit_book_owners_query")
    @patch("apps.gateway.routers.api.get_user_info_query")
    def test_student_credit_book_access_fails_closed_for_moodle_name_mismatch(
        self, get_info, get_owners, get_moodle_user, get_debts_mock
    ):
        get_info.return_value = {
            "roleid": 0,
            "groupname": "GROUP-1",
            "username": "student",
        }
        get_owners.return_value = [
            {
                "student_id": 42,
                "surname": "Иванов",
                "name": "Иван",
                "middle_name": "Иванович",
                "study_group": "GROUP-1",
                "identity_matches": 1,
            }
        ]
        get_moodle_user.return_value = {
            "username": "student",
            "firstname": "Петр Петрович",
            "lastname": "Петров",
            "fullname": "Петров Петр Петрович",
        }

        with self.assertRaises(HTTPException) as raised:
            get_debts(
                DebtDTO(credit_book="owned-book", semester_number=2),
                make_request("/dev/mauverse/get_debts", username="student"),
            )

        self.assertEqual(403, raised.exception.status_code)
        get_debts_mock.assert_not_called()

    @patch("apps.gateway.routers.api.get_semester_debts_by_credit_book_query")
    @patch("apps.gateway.routers.api.moodle_requests.get_moodle_user")
    @patch("apps.gateway.routers.api.get_credit_book_owners_query")
    @patch("apps.gateway.routers.api.get_user_info_query")
    def test_student_can_read_semesters_for_uniquely_owned_credit_book(
        self, get_info, get_owners, get_moodle_user, get_semesters_mock
    ):
        get_info.return_value = {
            "roleid": 0,
            "groupname": "GROUP-1",
            "username": "student",
        }
        get_owners.return_value = [
            {
                "student_id": 77,
                "surname": "Petrova",
                "name": "Elena",
                "middle_name": "",
                "study_group": "GROUP-1",
                "identity_matches": 1,
            }
        ]
        get_moodle_user.return_value = {
            "username": "student",
            "firstname": "Elena",
            "lastname": "Petrova",
            "fullname": "Elena Petrova",
        }
        get_semesters_mock.return_value = [{"semester": "Spring"}]

        result = get_semester(
            SemesterDTO(credit_book="owned-book"),
            make_request("/dev/mauverse/get_semesters", username="student"),
        )

        self.assertEqual([{"semester": "Spring"}], result["semesters"])
        get_semesters_mock.assert_called_once_with("owned-book", 77)

    @patch("apps.gateway.routers.api.get_debts_by_student_and_group_query")
    @patch("apps.gateway.routers.api.get_debts_by_credit_book_query")
    @patch("apps.gateway.routers.api.moodle_requests.get_moodle_user")
    @patch("apps.gateway.routers.api.get_credit_book_owners_query")
    @patch("apps.gateway.routers.api.get_user_info_query")
    def test_credit_book_request_never_falls_through_to_body_identity_flow(
        self,
        get_info,
        get_owners,
        get_moodle_user,
        get_credit_book_debts,
        get_group_debts,
    ):
        get_info.return_value = {
            "roleid": 0,
            "groupname": "GROUP-1",
            "username": "student",
        }
        get_owners.return_value = [
            {
                "student_id": 77,
                "surname": "Petrova",
                "name": "Elena",
                "middle_name": "",
                "study_group": "GROUP-1",
                "identity_matches": 1,
            }
        ]
        get_moodle_user.return_value = {
            "username": "student",
            "firstname": "Elena",
            "lastname": "Petrova",
            "fullname": "Elena Petrova",
        }

        with self.assertRaises(HTTPException) as raised:
            get_debts(
                DebtDTO(
                    credit_book="owned-book",
                    group_name="ATTACKER-GROUP",
                    first_name="Чужая",
                    name="Личность",
                    last_name="Из-Тела",
                ),
                make_request("/dev/mauverse/get_debts", username="student"),
            )

        self.assertEqual(422, raised.exception.status_code)
        get_credit_book_debts.assert_not_called()
        get_group_debts.assert_not_called()

    def test_send_order_rejects_empty_and_extra_payload(self):
        with self.assertRaises(ValidationError):
            StudentFormRequest.model_validate({})
        with self.assertRaises(ValidationError):
            StudentFormRequest.model_validate(
                {
                    "from": "student@example.com",
                    "username": "Student",
                    "text": [{"title": "Field", "value": "Value"}],
                    "subject": "untrusted subject",
                }
            )

    @patch("apps.gateway.routers.api.mail_sender.send_mail")
    def test_send_order_uses_server_controlled_subject(self, send_mail_mock):
        form = StudentFormRequest.model_validate(
            {
                "from": "student@example.com",
                "username": "Student Name",
                "text": [{"title": "Field", "value": "Value"}],
            }
        )
        result = send_order(form, make_request("/dev/mauverse/send_order"))

        self.assertEqual({"success": True}, result)
        payload = send_mail_mock.call_args.args[0]
        self.assertEqual("MAUverce - заказ справки об обучении", payload["subject"])
        self.assertNotIn("to", payload)


class ReliabilityAndAbuseTests(unittest.TestCase):
    @patch("apps.database.queries.debt_queries.execute_query")
    def test_credit_book_owner_lookup_reports_db_identity_cardinality(self, execute):
        execute.return_value = [(42, "Иванов", "Иван", "Иванович", "GROUP-1", 1)]

        result = database_get_credit_book_owners_query("owned-book")

        self.assertEqual(1, result[0]["identity_matches"])
        query = execute.call_args.args[1]
        self.assertIn("COUNT(DISTINCT identity_student.student_id)", query)
        self.assertEqual(("owned-book",), execute.call_args.args[2])

    @patch("apps.database.queries.debt_queries.execute_query")
    def test_credit_book_debt_query_is_scoped_to_authorized_student_id(self, execute):
        execute.return_value = [("Spring", 2, "Math", "Exam")]

        database_get_debts_by_credit_book_query(2, "owned-book", 42)

        query = execute.call_args.args[1]
        self.assertIn("students.student_id = %s", query)
        self.assertEqual(("owned-book", 42, 2), execute.call_args.args[2])

    @patch("apps.gateway.utils.http_client.Session.request")
    def test_http_client_rejects_external_redirects(self, request_mock):
        response = Mock(is_redirect=True, is_permanent_redirect=False)
        request_mock.return_value = response
        client = create_http_session()

        try:
            with self.assertRaises(requests.TooManyRedirects):
                client.post("https://example.com/submit", json={"token": "secret"})
        finally:
            client.close()

        self.assertFalse(request_mock.call_args.kwargs["allow_redirects"])

    def test_database_passwords_are_redacted(self):
        settings = DBSettings(
            SCHEDULE_DB_USER="schedule",
            SCHEDULE_DB_PASSWORD="schedule-secret",
            DEBT_DB_USER="debt",
            DEBT_DB_PASSWORD="debt-secret",
        )

        representation = repr(settings)
        self.assertNotIn("schedule-secret", representation)
        self.assertNotIn("debt-secret", representation)

    def test_rate_limiter_returns_retry_after(self):
        limiter = SlidingWindowRateLimiter(requests=2, window_seconds=60, max_entries=64)
        self.assertIsNone(limiter.acquire("student"))
        self.assertIsNone(limiter.acquire("student"))
        self.assertGreaterEqual(limiter.acquire("student"), 1)
        self.assertIsNone(limiter.acquire("another-student"))

    def test_docker_uses_single_worker_for_process_local_rate_limits(self):
        dockerfile = Path(__file__).resolve().parents[1] / "Dockerfile"
        contents = dockerfile.read_text(encoding="utf-8")

        self.assertIn('"--workers", "1"', contents)
        self.assertNotIn('"--workers", "2"', contents)

    @patch("apps.database.queries.user_queries.execute_query")
    def test_get_subgroups_uses_exact_group_match(self, execute):
        execute.return_value = [
            ("uid-main", "ИС-21", "Информатика", ""),
            ("uid-sub", "ИС-21-1", "Информатика", "uid-main"),
        ]

        result = database_get_subgroups("ис-21")

        execute.assert_called_once()
        query, params = execute.call_args[0][1], execute.call_args[0][2]
        self.assertIn("groups.group = %s", query)
        self.assertEqual(("ИС-21", "ИС-21-%"), params)
        self.assertEqual("uid-main", result["groupid"])
        self.assertEqual("Информатика", result["speciality"])
        self.assertEqual(
            [{"groupid": "uid-sub", "name": "ИС-21-1"}],
            result["subgroups"],
        )

    @patch("apps.database.queries.user_queries.execute_query")
    def test_get_subgroups_rejects_empty_group(self, execute):
        result = database_get_subgroups("   ")

        execute.assert_not_called()
        self.assertEqual({"error": "Не указана группа", "subgroups": []}, result)

    @patch("apps.database.queries.user_queries.execute_query")
    def test_ambiguous_local_user_profile_fails_closed(self, execute):
        execute.return_value = [
            (0, "GROUP", "student"),
            (0, "GROUP", "student"),
        ]

        self.assertEqual({}, database_get_user_info_query("student"))

    def test_schedule_period_is_bounded(self):
        with self.assertRaises(ValidationError):
            ScheduleDTO.model_validate(
                {
                    "start_date": "2026-01-01",
                    "end_date": "2026-12-31",
                    "group_id": "group",
                }
            )

    def test_untrusted_http_origins_are_rejected(self):
        with self.assertRaises(ValidationError):
            MailSettings(MAIL_API_BASE_URL="http://127.0.0.1:8080")
        with self.assertRaises(ValidationError):
            ParsingSettings(MAIN_URL="https://user:pass@example.com")

    @patch("apps.gateway.utils.mail_sender.mail_sender._http.post")
    def test_mail_recipient_is_server_controlled_without_mutating_input(self, post):
        response = Mock()
        response.raise_for_status.return_value = None
        post.return_value = response
        source = {"from": "student@example.com", "text": [], "username": "Student"}

        mail_sender.send_mail(source)

        payload = post.call_args.kwargs["json"]
        self.assertEqual(mail_config.MAIL_ADDRESS, payload["to"])
        self.assertNotIn("to", source)

    @patch("apps.gateway.utils.moodle.moodle_requests._http.post")
    def test_moodle_token_is_sent_in_post_body(self, post):
        response = Mock()
        response.raise_for_status.return_value = None
        response.json.return_value = {
            "username": "student",
            "firstname": "Student",
            "lastname": "Test",
            "fullname": "Student Test",
            "userid": 7,
        }
        post.return_value = response

        moodle_requests.get_moodle_user("secret-token")

        kwargs = post.call_args.kwargs
        self.assertEqual("secret-token", kwargs["data"]["wstoken"])
        self.assertNotIn("params", kwargs)
        self.assertNotIn("secret-token", post.call_args.args[0])

    @patch("apps.gateway.utils.moodle.moodle_requests._http.post")
    def test_moodle_invalidlogin_has_dedicated_exception(self, post):
        response = Mock()
        response.raise_for_status.return_value = None
        response.json.return_value = {
            "error": "upstream message",
            "errorcode": "invalidlogin",
        }
        post.return_value = response

        with self.assertRaises(MoodleInvalidCredentialsError):
            moodle_requests.get_token("student", "wrong-password")

    @patch("apps.gateway.utils.moodle.moodle_requests._http.post")
    def test_moodle_invalidtoken_has_dedicated_exception(self, post):
        response = Mock()
        response.raise_for_status.return_value = None
        response.json.return_value = {
            "exception": "moodle_exception",
            "errorcode": "invalidtoken",
            "message": "upstream message",
        }
        post.return_value = response

        with self.assertRaises(MoodleInvalidTokenError):
            moodle_requests.get_moodle_user("expired-token")

    @patch("apps.gateway.utils.moodle.moodle_requests._http.post")
    def test_moodle_internal_exception_is_not_treated_as_bad_credentials(self, post):
        response = Mock()
        response.raise_for_status.return_value = None
        response.json.return_value = {
            "exception": "dml_read_exception",
            "errorcode": "dmlreadexception",
            "message": "sensitive upstream detail",
        }
        post.return_value = response

        with self.assertRaises(MoodleServiceError):
            moodle_requests.get_moodle_user("token")

    def test_malformed_upstream_handler_does_not_leak_exception_detail(self):
        response = asyncio.run(
            upstream_response_handler(
                make_request("/dev/mauverse/auth"),
                UpstreamResponseError("sensitive upstream detail"),
            )
        )

        self.assertEqual(502, response.status_code)
        self.assertNotIn("sensitive", response.body.decode())

    def test_database_failure_is_raised_and_connection_is_closed(self):
        database = MagicMock()
        cursor = database.cursor.return_value.__enter__.return_value
        cursor.execute.side_effect = mysql.connector.Error("sensitive database detail")
        connection = SimpleNamespace(
            connect=Mock(return_value=database),
            database_name="test",
        )

        with self.assertRaises(DatabaseUnavailableError):
            execute_query(connection, "SELECT 1", ())

        database.close.assert_called_once_with()

    @patch("apps.gateway.main.schedule_db_connect.is_ready", return_value=True)
    @patch("apps.gateway.main.debt_db_connect.is_ready", return_value=False)
    def test_readiness_reports_dependency_outage(self, _debt_ready, _schedule_ready):
        response = readiness()

        self.assertEqual(503, response.status_code)
        self.assertEqual({"status": "not_ready"}, json.loads(response.body))


if __name__ == "__main__":
    unittest.main()
