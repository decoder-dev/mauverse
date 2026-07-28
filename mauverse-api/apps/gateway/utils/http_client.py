from threading import Lock, local
from typing import Any

from requests import Response, Session, TooManyRedirects
from requests.adapters import HTTPAdapter
from urllib3.util.retry import Retry

from apps.database.settings import network_config


def _build_session() -> Session:
    retries = Retry(
        total=1,
        connect=1,
        read=0,
        status=0,
        backoff_factor=0.25,
        allowed_methods=frozenset({"GET", "HEAD", "OPTIONS"}),
        raise_on_status=False,
    )
    adapter = HTTPAdapter(
        max_retries=retries,
        pool_connections=network_config.HTTP_POOL_SIZE,
        pool_maxsize=network_config.HTTP_POOL_SIZE,
    )
    session = Session()
    session.headers.update({"User-Agent": "MAUverce-API/1.7"})
    session.mount("https://", adapter)
    return session


class ThreadLocalHttpClient:
    """Keep connection pools isolated per worker thread and never retain user cookies."""

    def __init__(self) -> None:
        self._local = local()
        self._sessions: set[Session] = set()
        self._lock = Lock()
        self._closed = False

    def request(self, method: str, url: str, **kwargs: Any) -> Response:
        kwargs["allow_redirects"] = False
        session = self._get_session()
        try:
            response = session.request(method, url, **kwargs)
        finally:
            session.cookies.clear()

        # Redirects could forward Moodle tokens or student form data to another origin.
        if response.is_redirect or response.is_permanent_redirect:
            raise TooManyRedirects("External service redirects are not allowed", response=response)
        return response

    def get(self, url: str, **kwargs: Any) -> Response:
        return self.request("GET", url, **kwargs)

    def post(self, url: str, **kwargs: Any) -> Response:
        return self.request("POST", url, **kwargs)

    def close(self) -> None:
        with self._lock:
            self._closed = True
            sessions = tuple(self._sessions)
            self._sessions.clear()
        for session in sessions:
            session.close()

    def _get_session(self) -> Session:
        session = getattr(self._local, "session", None)
        if session is not None:
            return session

        with self._lock:
            if self._closed:
                raise RuntimeError("HTTP client is closed")
            session = _build_session()
            self._sessions.add(session)
        self._local.session = session
        return session


def create_http_session() -> ThreadLocalHttpClient:
    return ThreadLocalHttpClient()


def request_timeout() -> tuple[float, float]:
    return (
        network_config.HTTP_CONNECT_TIMEOUT_SECONDS,
        network_config.HTTP_READ_TIMEOUT_SECONDS,
    )
