from collections.abc import Awaitable, Callable

from fastapi import Request
from starlette import status
from starlette.concurrency import run_in_threadpool
from starlette.middleware.base import BaseHTTPMiddleware
from starlette.responses import JSONResponse, Response

from apps.gateway.validation import AuthMiddlewareValidation, ValidationErrors

except_routes = {
    "/dev/mauverse/auth",
    "/dev/mauverse/auth/",
    "/health",
    "/health/",
    "/ready",
    "/ready/",
    "/docs",
    "/docs/oauth2-redirect",
    "/openapi.json",
    "/redoc",
}


def _secure_response(response: Response) -> Response:
    response.headers["Cache-Control"] = "no-store"
    response.headers["X-Content-Type-Options"] = "nosniff"
    response.headers["Referrer-Policy"] = "no-referrer"
    return response


class AuthMiddleware(BaseHTTPMiddleware):
    async def dispatch(
        self,
        request: Request,
        call_next: Callable[[Request], Awaitable[Response]],
    ) -> Response:
        if request.url.path in except_routes:
            return _secure_response(await call_next(request))
        validation = AuthMiddlewareValidation(request)
        errors = await run_in_threadpool(validation.user_request_validation)
        content = {
            "statuscode": status.HTTP_200_OK,
            "error": {},
        }
        if errors == ValidationErrors.EMPTY_REQUIRED_HEADERS:
            content["statuscode"] = status.HTTP_401_UNAUTHORIZED
            content["error"] = {
                "error": f"Ошибка {status.HTTP_401_UNAUTHORIZED}",
                "detail": (
                    "Ваши данные не прошли проверку авторизации. Необходимо авторизоваться повторно"
                ),
            }
        elif errors in (ValidationErrors.INVALID_TOKEN, ValidationErrors.WRONG_TOKEN):
            content["statuscode"] = status.HTTP_401_UNAUTHORIZED
            content["error"] = {
                "error": f"Ошибка {status.HTTP_401_UNAUTHORIZED}",
                "detail": "Сессия недействительна, необходимо авторизоваться повторно",
            }
        elif errors == ValidationErrors.SERVICE_UNAVAILABLE:
            content["statuscode"] = status.HTTP_503_SERVICE_UNAVAILABLE
            content["error"] = {
                "error": f"Ошибка {status.HTTP_503_SERVICE_UNAVAILABLE}",
                "detail": "Сервис проверки авторизации временно недоступен",
            }
        if content["statuscode"] != status.HTTP_200_OK:
            return _secure_response(
                JSONResponse(status_code=content["statuscode"], content=content["error"])
            )
        request.state.auth_username = request.headers["X-Auth-Username"].strip()
        request.state.auth_token = request.headers["X-Auth-Token"].strip()
        response = await call_next(request)
        return _secure_response(response)
