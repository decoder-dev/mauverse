from enum import Enum
from hashlib import sha256
from hmac import compare_digest
from threading import Lock
from time import monotonic

import requests
from fastapi import Request

from apps.database.settings import parser_config, security_config
from apps.gateway.utils.http_client import create_http_session, request_timeout


class ValidationErrors(Enum):
    NO_ERRORS = 0
    INVALID_TOKEN = 1
    WRONG_TOKEN = 2
    EMPTY_REQUIRED_HEADERS = 3
    SERVICE_UNAVAILABLE = 4


_validation_cache: dict[bytes, float] = {}
_validation_cache_lock = Lock()
_http = create_http_session()


def _cache_key(token: str, username: str) -> bytes:
    # Bearer tokens are hashed so raw credentials never become process-level cache keys.
    material = f"{token}\0{username}".encode()
    return sha256(material).digest()


def _is_cached(token: str, username: str) -> bool:
    key = _cache_key(token, username)
    now = monotonic()
    with _validation_cache_lock:
        expires_at = _validation_cache.get(key, 0)
        if expires_at > now:
            return True
        _validation_cache.pop(key, None)
    return False


def _cache_success(token: str, username: str) -> None:
    if security_config.AUTH_CACHE_TTL_SECONDS == 0:
        return

    now = monotonic()
    with _validation_cache_lock:
        if len(_validation_cache) >= security_config.AUTH_CACHE_MAX_ENTRIES:
            expired = [key for key, expires_at in _validation_cache.items() if expires_at <= now]
            for key in expired:
                _validation_cache.pop(key, None)
            if len(_validation_cache) >= security_config.AUTH_CACHE_MAX_ENTRIES:
                oldest_key = min(_validation_cache, key=_validation_cache.get)
                _validation_cache.pop(oldest_key, None)
        _validation_cache[_cache_key(token, username)] = (
            now + security_config.AUTH_CACHE_TTL_SECONDS
        )


def clear_validation_cache() -> None:
    with _validation_cache_lock:
        _validation_cache.clear()


def close_validation_resources() -> None:
    _http.close()


def validate_credentials(token: str | None, username: str | None) -> ValidationErrors:
    if not token or not username:
        return ValidationErrors.EMPTY_REQUIRED_HEADERS
    token = token.strip()
    username = username.strip()
    if not token or not username or len(token) > 4096 or len(username) > 200:
        return ValidationErrors.EMPTY_REQUIRED_HEADERS
    if _is_cached(token, username):
        return ValidationErrors.NO_ERRORS

    auth_params = {
        "wsfunction": "core_webservice_get_site_info",
        "wstoken": token,
        "moodlewsrestformat": "json",
    }
    try:
        response = _http.post(
            url=parser_config.EIOS_AUTH_URL,
            data=auth_params,
            timeout=request_timeout(),
        )
        response.raise_for_status()
        payload = response.json()
    except requests.HTTPError as exc:
        if exc.response is not None and exc.response.status_code in (401, 403):
            return ValidationErrors.INVALID_TOKEN
        return ValidationErrors.SERVICE_UNAVAILABLE
    except (requests.RequestException, ValueError):
        return ValidationErrors.SERVICE_UNAVAILABLE

    if not isinstance(payload, dict):
        return ValidationErrors.SERVICE_UNAVAILABLE
    raw_error_code = payload.get("errorcode")
    error_code = raw_error_code.strip().casefold() if isinstance(raw_error_code, str) else None
    if error_code == "invalidtoken":
        return ValidationErrors.INVALID_TOKEN
    if any(key in payload for key in ("error", "errorcode", "exception")):
        return ValidationErrors.SERVICE_UNAVAILABLE
    upstream_username = payload.get("username")
    if not isinstance(upstream_username, str) or not upstream_username:
        return ValidationErrors.SERVICE_UNAVAILABLE
    if not compare_digest(username, upstream_username):
        return ValidationErrors.WRONG_TOKEN
    _cache_success(token, username)
    return ValidationErrors.NO_ERRORS


class AuthMiddlewareValidation:
    def __init__(self, request: Request) -> None:
        self.request = request

    def user_request_validation(self) -> ValidationErrors:
        headers = self.request.headers
        token = headers.get("X-Auth-Token", None)
        username = headers.get("X-Auth-Username", None)
        return validate_credentials(token, username)
