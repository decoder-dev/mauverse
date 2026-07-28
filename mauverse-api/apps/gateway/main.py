import logging
from collections.abc import AsyncIterator
from contextlib import asynccontextmanager
from typing import Any

import requests
from fastapi import FastAPI, Request, status
from starlette.responses import JSONResponse

import apps.gateway.routers.common as routers
from apps.database.db_connect import debt_db_connect, schedule_db_connect
from apps.database.settings import app_config
from apps.gateway.errors import (
    DatabaseUnavailableError,
    MoodleAuthenticationError,
    MoodleServiceError,
    UpstreamResponseError,
)
from apps.gateway.middlewares import AuthMiddleware
from apps.gateway.utils.mail_sender import mail_sender
from apps.gateway.utils.moodle import moodle_requests
from apps.gateway.utils.parser import contact_parser, rss_parser
from apps.gateway.validation import close_validation_resources

logger = logging.getLogger(__name__)


@asynccontextmanager
async def lifespan(_app: FastAPI) -> AsyncIterator[None]:
    yield
    close_validation_resources()
    moodle_requests.close()
    mail_sender.close()
    contact_parser.close()
    rss_parser.close()


app = FastAPI(
    title="MAUverce API",
    docs_url="/docs" if app_config.ENABLE_DOCS else None,
    redoc_url="/redoc" if app_config.ENABLE_DOCS else None,
    openapi_url="/openapi.json" if app_config.ENABLE_DOCS else None,
    lifespan=lifespan,
)
app.add_middleware(AuthMiddleware)
routers.init(app)


@app.get("/health", include_in_schema=False)
async def health() -> dict[str, str]:
    return {"status": "ok"}


@app.get("/ready", include_in_schema=False)
def readiness() -> Any:
    if not debt_db_connect.is_ready() or not schedule_db_connect.is_ready():
        return JSONResponse(
            status_code=status.HTTP_503_SERVICE_UNAVAILABLE,
            content={"status": "not_ready"},
        )
    return {"status": "ready"}


@app.exception_handler(DatabaseUnavailableError)
async def database_unavailable_handler(
    request: Request, exc: DatabaseUnavailableError
) -> JSONResponse:
    logger.warning(
        "Database dependency unavailable method=%s path=%s failure_type=%s",
        request.method,
        request.url.path,
        type(exc).__name__,
    )
    return JSONResponse(
        status_code=status.HTTP_503_SERVICE_UNAVAILABLE,
        content={"detail": "Сервис данных временно недоступен"},
    )


@app.exception_handler(UpstreamResponseError)
async def upstream_response_handler(request: Request, exc: UpstreamResponseError) -> JSONResponse:
    logger.warning(
        "Invalid upstream response method=%s path=%s failure_type=%s",
        request.method,
        request.url.path,
        type(exc).__name__,
    )
    return JSONResponse(
        status_code=status.HTTP_502_BAD_GATEWAY,
        content={"detail": "Внешний сервис вернул некорректный ответ"},
    )


@app.exception_handler(MoodleAuthenticationError)
async def moodle_authentication_handler(
    request: Request, exc: MoodleAuthenticationError
) -> JSONResponse:
    logger.info(
        "Moodle rejected authentication method=%s path=%s failure_type=%s",
        request.method,
        request.url.path,
        type(exc).__name__,
    )
    return JSONResponse(
        status_code=status.HTTP_401_UNAUTHORIZED,
        content={"detail": "Сессия недействительна, необходимо авторизоваться повторно"},
    )


@app.exception_handler(MoodleServiceError)
async def moodle_service_handler(request: Request, exc: MoodleServiceError) -> JSONResponse:
    logger.warning(
        "Moodle dependency unavailable method=%s path=%s failure_type=%s",
        request.method,
        request.url.path,
        type(exc).__name__,
    )
    return JSONResponse(
        status_code=status.HTTP_503_SERVICE_UNAVAILABLE,
        content={"detail": "Сервис Moodle временно недоступен"},
    )


@app.exception_handler(requests.RequestException)
async def upstream_unavailable_handler(
    request: Request, exc: requests.RequestException
) -> JSONResponse:
    logger.warning(
        "Upstream request failed method=%s path=%s failure_type=%s",
        request.method,
        request.url.path,
        type(exc).__name__,
    )
    return JSONResponse(
        status_code=status.HTTP_502_BAD_GATEWAY,
        content={"detail": "Внешний сервис временно недоступен"},
    )
