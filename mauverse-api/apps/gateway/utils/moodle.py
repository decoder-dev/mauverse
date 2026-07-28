from typing import Any

import requests

from apps.gateway.errors import (
    MoodleAuthenticationError,
    MoodleInvalidCredentialsError,
    MoodleInvalidTokenError,
    MoodleServiceError,
    UpstreamResponseError,
)
from apps.gateway.utils.converter import format_unix_time
from apps.gateway.utils.http_client import create_http_session, request_timeout


class MoodleRequests:
    def __init__(self) -> None:
        self.url = "https://eios.mauniver.ru/moodle"
        self.service_url = "/webservice/rest/server.php"
        self.service_name = "moodle_mobile_app"
        self._http = create_http_session()

    def close(self) -> None:
        self._http.close()

    @staticmethod
    def _json_object(response: requests.Response) -> dict[str, Any]:
        try:
            payload = response.json()
        except ValueError as exc:
            raise UpstreamResponseError("Moodle returned invalid JSON") from exc
        if not isinstance(payload, dict):
            raise UpstreamResponseError("Moodle returned an invalid response shape")
        return payload

    @staticmethod
    def _raise_for_moodle_error(payload: dict[str, Any]) -> None:
        if not any(key in payload for key in ("error", "errorcode", "exception")):
            return

        raw_error_code = payload.get("errorcode")
        error_code = raw_error_code.strip().casefold() if isinstance(raw_error_code, str) else None
        if error_code == "invalidlogin":
            raise MoodleInvalidCredentialsError
        if error_code == "invalidtoken":
            raise MoodleInvalidTokenError
        raise MoodleServiceError

    def _post_json(
        self,
        url: str,
        data: dict[str, Any],
        authentication_error: type[MoodleAuthenticationError],
    ) -> dict[str, Any]:
        try:
            response = self._http.post(url, data=data, timeout=request_timeout())
            response.raise_for_status()
        except requests.HTTPError as exc:
            if exc.response is not None and exc.response.status_code in (401, 403):
                raise authentication_error from exc
            raise MoodleServiceError from exc
        except requests.RequestException as exc:
            raise MoodleServiceError from exc

        payload = self._json_object(response)
        self._raise_for_moodle_error(payload)
        return payload

    def get_token(self, username: str, password: str) -> dict[str, Any]:
        payload = self._post_json(
            f"{self.url}/login/token.php",
            {"username": username, "password": password, "service": self.service_name},
            MoodleInvalidCredentialsError,
        )
        token = payload.get("token")
        if not isinstance(token, str) or not token.strip():
            raise UpstreamResponseError("Moodle token response is incomplete")
        return {"token": token}

    def get_moodle_user(self, token: str) -> dict[str, Any]:
        function = "core_webservice_get_site_info"
        payload = self._post_json(
            f"{self.url}{self.service_url}",
            {"wstoken": token, "wsfunction": function, "moodlewsrestformat": "json"},
            MoodleInvalidTokenError,
        )
        required = ("username", "firstname", "lastname", "fullname", "userid")
        if any(key not in payload for key in required):
            raise UpstreamResponseError("Moodle profile is incomplete")
        if any(not isinstance(payload[key], str) for key in required[:-1]) or not isinstance(
            payload["userid"], int
        ):
            raise UpstreamResponseError("Moodle profile has invalid field types")
        return {key: payload[key] for key in required}

    def get_notifications(self, token: str, user_id: int) -> list[dict[str, Any]] | dict[str, str]:
        function = "core_message_get_messages"
        message_type = "notifications"
        user_from = "0"
        payload = self._post_json(
            f"{self.url}{self.service_url}",
            {
                "wstoken": token,
                "wsfunction": function,
                "type": message_type,
                "useridto": user_id,
                "useridfrom": user_from,
                "moodlewsrestformat": "json",
            },
            MoodleInvalidTokenError,
        )
        raw_messages = payload.get("messages")
        if not isinstance(raw_messages, list):
            raise UpstreamResponseError("Moodle messages have an invalid response shape")
        messages = [dict(message) for message in raw_messages if isinstance(message, dict)]
        formatted_messages: list[dict[str, Any]] = []

        for message in messages:
            if message.get("eventtype") == "newlogin":
                continue
            time_created = message.get("timecreated")
            if not isinstance(time_created, int | float):
                continue
            message["timecreatedstring"] = format_unix_time(time_created)
            sender_id = message.get("useridfrom")
            if message.get("eventtype") == "insights":
                message["colorname"] = "#A052BA44"
            elif sender_id == -10:
                message["colorname"] = "#F1C187"
            elif sender_id not in (-10, -20):
                message["colorname"] = "#A052BA44"
            else:
                message["colorname"] = "#0064BE"
            small_message = message.get("smallmessage")
            if isinstance(small_message, str):
                cleaned_message = small_message.replace("<br/>", "").replace("<br>", "")
                cleaned_message = cleaned_message.replace("</br>", "").replace("\n", "")
                message["smallmessage"] = cleaned_message[:40] + (
                    "..." if len(cleaned_message) > 40 else ""
                )
            formatted_messages.append(message)

        return formatted_messages


moodle_requests = MoodleRequests()
