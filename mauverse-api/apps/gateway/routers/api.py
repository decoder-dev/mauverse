from hmac import compare_digest
from ipaddress import ip_address
from itertools import permutations
from typing import Any
from unicodedata import normalize

from fastapi import Depends, HTTPException, Request, status
from requests import RequestException

from apps.database.queries.debt_queries import (
    get_credit_book_owners_query,
    get_debts_by_credit_book_query,
    get_debts_by_student_and_group_query,
    get_semester_debts_by_credit_book_query,
    get_semester_debts_by_group_query,
    get_total_debts_by_group_query,
)
from apps.database.queries.schedule_queries import (
    get_all_rooms_query,
    get_group_query,
    get_groups_query,
    get_room_query,
    get_schedule_by_group_query,
    get_schedule_by_room_query,
    get_schedule_by_teacher_query,
)
from apps.database.queries.teachers_queries import (
    get_all_teachers_query,
    get_teacher_query,
    get_teachers_query,
)
from apps.database.queries.user_queries import get_subgroups, get_user_info_query
from apps.database.settings import security_config
from apps.gateway.errors import (
    MoodleAuthenticationError,
    MoodleServiceError,
    UpstreamResponseError,
)
from apps.gateway.models.debt import DebtDTO, SemesterDTO
from apps.gateway.models.schedule import Room, ScheduleDTO
from apps.gateway.models.users import (
    GroupName,
    StudentFormRequest,
    TeacherName,
    UserAuth,
    UserInfo,
    UserNotification,
)
from apps.gateway.utils.converter import get_parser_type
from apps.gateway.utils.mail_sender import mail_sender
from apps.gateway.utils.moodle import moodle_requests
from apps.gateway.utils.parser import ParserType, rss_parser
from apps.gateway.utils.rate_limit import SlidingWindowRateLimiter

auth_ip_rate_limiter = SlidingWindowRateLimiter(
    security_config.AUTH_IP_RATE_LIMIT_REQUESTS,
    security_config.AUTH_RATE_LIMIT_WINDOW_SECONDS,
    security_config.RATE_LIMIT_MAX_ENTRIES,
)
auth_username_rate_limiter = SlidingWindowRateLimiter(
    security_config.AUTH_USERNAME_RATE_LIMIT_REQUESTS,
    security_config.AUTH_RATE_LIMIT_WINDOW_SECONDS,
    security_config.RATE_LIMIT_MAX_ENTRIES,
)
form_rate_limiter = SlidingWindowRateLimiter(
    security_config.FORM_RATE_LIMIT_REQUESTS,
    security_config.FORM_RATE_LIMIT_WINDOW_SECONDS,
    security_config.RATE_LIMIT_MAX_ENTRIES,
)


def clear_rate_limits() -> None:
    auth_ip_rate_limiter.clear()
    auth_username_rate_limiter.clear()
    form_rate_limiter.clear()


def _enforce_rate_limit(limiter: SlidingWindowRateLimiter, key: str) -> None:
    retry_after = limiter.acquire(key)
    if retry_after is not None:
        raise HTTPException(
            status_code=status.HTTP_429_TOO_MANY_REQUESTS,
            detail="Слишком много запросов. Повторите попытку позже",
            headers={"Retry-After": str(retry_after)},
        )


def _client_host(request: Request) -> str:
    if not request.client:
        return "unknown"
    host = request.client.host.strip()
    try:
        return ip_address(host).compressed
    except ValueError:
        return host.casefold() or "unknown"


def _normalized_username(username: str) -> str:
    return normalize("NFKC", username).strip().casefold()


def _normalized_session_username(username: str) -> str:
    return normalize("NFC", username).strip().casefold()


def _normalized_identity_value(value: Any, *, allow_empty: bool = False) -> str | None:
    if not isinstance(value, str):
        return None
    normalized = " ".join(normalize("NFC", value).split()).casefold()
    if not normalized and not allow_empty:
        return None
    return normalized


def _enforce_auth_rate_limits(request: Request, username: str) -> None:
    retry_values = (
        auth_ip_rate_limiter.acquire(_client_host(request)),
        auth_username_rate_limiter.acquire(_normalized_username(username)),
    )
    retry_after = max((value for value in retry_values if value is not None), default=None)
    if retry_after is not None:
        raise HTTPException(
            status_code=status.HTTP_429_TOO_MANY_REQUESTS,
            detail="Слишком много запросов. Повторите попытку позже",
            headers={"Retry-After": str(retry_after)},
        )


def _credit_book_owner_matches_session(
    request: Request,
    profile: dict[str, Any],
    owner: dict[str, Any],
) -> bool:
    student_id = owner.get("student_id")
    if not isinstance(student_id, int) or isinstance(student_id, bool) or student_id <= 0:
        return False
    identity_matches = owner.get("identity_matches")
    if (
        not isinstance(identity_matches, int)
        or isinstance(identity_matches, bool)
        or identity_matches != 1
    ):
        return False

    authenticated_username = _normalized_session_username(request.state.auth_username)
    profile_username = profile.get("username")
    if (
        not isinstance(profile_username, str)
        or _normalized_session_username(profile_username) != authenticated_username
    ):
        return False

    profile_group = _normalized_identity_value(profile.get("groupname"))
    owner_group = _normalized_identity_value(owner.get("study_group"))
    if profile_group is None or profile_group != owner_group:
        return False

    surname = _normalized_identity_value(owner.get("surname"))
    first_name = _normalized_identity_value(owner.get("name"))
    middle_name = _normalized_identity_value(owner.get("middle_name"), allow_empty=True)
    if surname is None or first_name is None or middle_name is None:
        return False

    moodle_user = moodle_requests.get_moodle_user(request.state.auth_token)
    if not isinstance(moodle_user, dict):
        return False
    moodle_username = moodle_user.get("username")
    if (
        not isinstance(moodle_username, str)
        or _normalized_session_username(moodle_username) != authenticated_username
    ):
        return False

    moodle_lastname = _normalized_identity_value(moodle_user.get("lastname"))
    moodle_firstname = _normalized_identity_value(moodle_user.get("firstname"))
    moodle_fullname = _normalized_identity_value(moodle_user.get("fullname"))
    expected_firstnames = {first_name}
    full_name_parts = [surname, first_name]
    if middle_name:
        expected_firstnames.add(f"{first_name} {middle_name}")
        full_name_parts.append(middle_name)
    expected_fullnames = {" ".join(parts) for parts in permutations(full_name_parts)}

    return (
        moodle_lastname == surname
        and moodle_firstname in expected_firstnames
        and moodle_fullname in expected_fullnames
    )


def _authorize_debt_access(
    request: Request, *, credit_book: str | None, group_name: str | None
) -> int | None:
    profile = get_user_info_query(request.state.auth_username)
    if not isinstance(profile, dict) or "roleid" not in profile:
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="Профиль пользователя не разрешает просмотр задолженностей",
        )

    try:
        role_id = int(profile["roleid"])
    except (TypeError, ValueError) as exc:
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="Роль пользователя не разрешает просмотр задолженностей",
        ) from exc

    if credit_book:
        if role_id != 0:
            raise HTTPException(
                status_code=status.HTTP_403_FORBIDDEN,
                detail="Задолженности по зачетной книжке доступны только студенту",
            )
        owners = get_credit_book_owners_query(credit_book)
        if (
            isinstance(owners, list)
            and len(owners) == 1
            and isinstance(owners[0], dict)
            and _credit_book_owner_matches_session(request, profile, owners[0])
        ):
            return owners[0]["student_id"]
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="Принадлежность зачетной книжки текущему пользователю не подтверждена",
        )

    if group_name:
        profile_group = profile.get("groupname")
        if (
            role_id != 1
            or not isinstance(profile_group, str)
            or profile_group.strip().casefold() != group_name.strip().casefold()
        ):
            raise HTTPException(
                status_code=status.HTTP_403_FORBIDDEN,
                detail="Доступ разрешен только к курируемой группе",
            )
        return None

    raise HTTPException(
        status_code=status.HTTP_422_UNPROCESSABLE_ENTITY,
        detail="Не указана зачетная книжка или группа",
    )


def get_user_info(request: Request, user: UserInfo | None = None) -> dict[str, Any]:
    del user  # The authenticated identity is authoritative; the body remains API-compatible.
    return get_user_info_query(request.state.auth_username)


def get_session(request: Request) -> dict[str, str | bool]:
    return {"authenticated": True, "username": request.state.auth_username}


def auth(user: UserAuth, request: Request) -> dict[str, Any]:
    _enforce_auth_rate_limits(request, user.username)
    try:
        tokens = moodle_requests.get_token(
            username=user.username, password=user.password.get_secret_value()
        )
    except MoodleAuthenticationError as exc:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Неверный логин или пароль",
        ) from exc
    except (MoodleServiceError, RequestException) as exc:
        raise HTTPException(
            status_code=status.HTTP_503_SERVICE_UNAVAILABLE,
            detail="Сервис авторизации временно недоступен",
        ) from exc
    token = tokens.get("token")
    if not isinstance(token, str) or not token.strip():
        raise UpstreamResponseError("Moodle token response is incomplete")
    try:
        moodle_user = moodle_requests.get_moodle_user(token)
    except MoodleAuthenticationError as exc:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Сессия Moodle недействительна",
        ) from exc
    except (MoodleServiceError, RequestException) as exc:
        raise HTTPException(
            status_code=status.HTTP_503_SERVICE_UNAVAILABLE,
            detail="Сервис авторизации временно недоступен",
        ) from exc
    moodle_username = moodle_user.get("username")
    if not isinstance(moodle_username, str) or not moodle_username:
        raise UpstreamResponseError("Moodle profile is incomplete")
    user_info = get_user_info_query(moodle_username)
    if not isinstance(user_info, dict) or not user_info or "roleid" not in user_info:
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="Локальный профиль пользователя не найден",
        )
    moodle_user.update(user_info)
    moodle_user["token"] = token
    return moodle_user


def get_notifications(
    user: UserNotification, request: Request
) -> list[dict[str, Any]] | dict[str, str]:
    token = user.token.get_secret_value()
    if not compare_digest(token, request.state.auth_token):
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="Токен запроса не совпадает с авторизованной сессией",
        )
    moodle_user = moodle_requests.get_moodle_user(token)
    if moodle_user.get("userid") != user.user_id:
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="Идентификатор пользователя не совпадает с авторизованной сессией",
        )
    return moodle_requests.get_notifications(token=token, user_id=user.user_id)


def send_order(form: StudentFormRequest, request: Request) -> dict[str, bool]:
    _enforce_rate_limit(form_rate_limiter, request.state.auth_username.casefold())
    payload = {
        "from": form.sender,
        "username": form.username,
        "subject": "MAUverce - заказ справки об обучении",
        "text": [field.model_dump() for field in form.text],
    }
    try:
        mail_sender.send_mail(payload)
    except RequestException as exc:
        raise HTTPException(
            status_code=status.HTTP_502_BAD_GATEWAY,
            detail="Сервис отправки почты временно недоступен",
        ) from exc
    return {"success": True}


def get_subgroup_info(group: GroupName | None = None) -> dict[str, Any]:
    if group is None:
        return {"error": "Не указана группа"}
    return get_subgroups(group.group_name)


def get_news(news_type: ParserType = Depends(get_parser_type)) -> list[dict[str, Any]]:
    return rss_parser.get_rss_data(news_type)


def check_group(group: GroupName) -> bool:
    return get_group_query(group.group_name)


def get_groups(group: GroupName) -> dict[str, list[str]]:
    result = get_groups_query(group.group_name)
    return {"groups": result}


def check_teacher(teacher: TeacherName) -> bool:
    return get_teacher_query(teacher.teacher_name)


def get_semester(data: SemesterDTO, request: Request) -> dict[str, Any]:
    student_id = _authorize_debt_access(
        request,
        credit_book=data.credit_book,
        group_name=data.group_name,
    )
    result = {}
    if data.credit_book:
        if student_id is None:
            raise HTTPException(status_code=status.HTTP_403_FORBIDDEN)
        result["semesters"] = get_semester_debts_by_credit_book_query(data.credit_book, student_id)
    elif data.group_name and data.name and data.first_name and data.last_name:
        result["semesters"] = get_semester_debts_by_group_query(
            surname=data.first_name,
            name=data.name,
            middle_name=data.last_name,
            group_name=data.group_name,
        )
    else:
        result["error"] = "Не удалось получить данные по семестрам"
    return result


def get_debts(data: DebtDTO | None, request: Request) -> dict[str, Any]:
    if data is None:
        return {"error": "Не указаны параметры задолженностей"}
    student_id = _authorize_debt_access(
        request,
        credit_book=data.credit_book,
        group_name=data.group_name,
    )
    if data.credit_book:
        if student_id is None:
            raise HTTPException(status_code=status.HTTP_403_FORBIDDEN)
        if data.semester_number is None:
            raise HTTPException(
                status_code=status.HTTP_422_UNPROCESSABLE_CONTENT,
                detail="Для задолженностей по зачетной книжке необходимо указать семестр",
            )
        result = get_debts_by_credit_book_query(data.semester_number, data.credit_book, student_id)
        return {"debts": result}
    if (
        data.first_name
        and data.name
        and data.last_name
        and data.group_name
        and data.semester_number
    ):
        result = get_debts_by_student_and_group_query(
            surname=data.first_name,
            name=data.name,
            middle_name=data.last_name,
            group_name=data.group_name,
            semester_number=data.semester_number,
        )
        return {"debts": result}
    if data.group_name:
        result = get_total_debts_by_group_query(data.group_name)
        return {"students": result}

    return {"error": "Не удалось получить долги"}


def get_schedule(schedule: ScheduleDTO) -> dict[str, Any]:
    result = {}

    if schedule.group_id:
        result["schedules"] = get_schedule_by_group_query(schedule)
    elif schedule.room_id:
        result["schedules"] = get_schedule_by_room_query(schedule)
    elif (
        schedule.teacher_last_name and schedule.teacher_second_name and schedule.teacher_first_name
    ):
        result["schedules"] = get_schedule_by_teacher_query(schedule)
    else:
        result["error"] = "Не удалось получить расписание"

    return result


def get_teachers(teacher: TeacherName) -> dict[str, list[str]]:
    result = get_teachers_query(teacher.teacher_name)
    return {"teachers": result}


def get_all_teachers(teacher: TeacherName) -> dict[str, list[str]]:
    result = get_all_teachers_query(teacher.teacher_name)
    return {"teachers": result}


def get_room(room: Room) -> dict[str, list[dict[str, Any]]]:
    result = get_room_query(room.room_name)
    return {"rooms": result}


def get_all_rooms(room: Room) -> dict[str, list[dict[str, Any]]]:
    result = get_all_rooms_query(room.room_name)
    return {"rooms": result}
