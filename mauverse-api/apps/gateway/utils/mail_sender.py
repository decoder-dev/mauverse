from typing import Any

from apps.database.settings import mail_config
from apps.gateway.utils.http_client import create_http_session, request_timeout


class MailSender:
    def __init__(self) -> None:
        self.url = f"{mail_config.MAIL_API_BASE_URL}{mail_config.MAIL_API_URL}"
        self._http = create_http_session()

    def close(self) -> None:
        self._http.close()

    def send_mail(self, form: dict[str, Any]) -> None:
        payload = dict(form)
        if mail_config.MAIL_COPY_ADDRESS:
            payload["bcc"] = mail_config.MAIL_COPY_ADDRESS
        payload["to"] = mail_config.MAIL_ADDRESS
        response = self._http.post(self.url, json=payload, timeout=request_timeout())
        response.raise_for_status()


mail_sender = MailSender()
