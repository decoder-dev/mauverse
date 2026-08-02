import re
from urllib.parse import urlsplit

from pydantic import Field, SecretStr, ValidationInfo, field_validator
from pydantic_settings import BaseSettings, SettingsConfigDict


def _https_url(value: str, *, origin_only: bool) -> str:
    cleaned = value.strip().rstrip("/")
    parsed = urlsplit(cleaned)
    if (
        parsed.scheme != "https"
        or not parsed.hostname
        or parsed.username
        or parsed.password
        or parsed.query
        or parsed.fragment
        or (origin_only and parsed.path not in ("", "/"))
    ):
        raise ValueError("Ожидается безопасный HTTPS URL без credentials, query и fragment")
    return cleaned


def _email(value: str, *, allow_empty: bool = False) -> str:
    cleaned = value.strip()
    if allow_empty and not cleaned:
        return ""
    if not re.fullmatch(r"[^@\s]+@[^@\s]+\.[^@\s]+", cleaned):
        raise ValueError("Некорректный адрес электронной почты")
    return cleaned


def _relative_path(value: str, *, setting_name: str) -> str:
    cleaned = value.strip()
    parsed = urlsplit(cleaned)
    if not cleaned.startswith("/") or cleaned.startswith("//") or parsed.scheme or parsed.netloc:
        raise ValueError(f"{setting_name} должен быть относительным путем")
    if parsed.query or parsed.fragment:
        raise ValueError(f"{setting_name} не должен содержать query или fragment")
    return cleaned


class MailSettings(BaseSettings):
    MAIL_ADDRESS: str = "mauverse@mauniver.ru"
    MAIL_COPY_ADDRESS: str = ""
    MAIL_API_BASE_URL: str = "https://api.mauniver.ru"
    MAIL_API_URL: str = "/send-email"

    model_config = SettingsConfigDict(env_file=".env", extra="ignore")

    @field_validator("MAIL_ADDRESS")
    @classmethod
    def validate_mail_address(cls, value: str) -> str:
        return _email(value)

    @field_validator("MAIL_COPY_ADDRESS")
    @classmethod
    def validate_copy_address(cls, value: str) -> str:
        return _email(value, allow_empty=True)

    @field_validator("MAIL_API_BASE_URL")
    @classmethod
    def validate_mail_origin(cls, value: str) -> str:
        return _https_url(value, origin_only=True)

    @field_validator("MAIL_API_URL")
    @classmethod
    def validate_mail_path(cls, value: str) -> str:
        return _relative_path(value, setting_name="MAIL_API_URL")


class DBSettings(BaseSettings):
    SCHEDULE_DB_NAME: str = "1c"
    SCHEDULE_DB_HOST: str = "95.54.199.81"
    SCHEDULE_DB_PORT: int = Field(default=3306, ge=1, le=65535)
    SCHEDULE_DB_USER: str = Field(...)
    SCHEDULE_DB_PASSWORD: SecretStr = Field(...)

    DEBT_DB_NAME: str = "lko"
    DEBT_DB_HOST: str = "95.54.199.81"
    DEBT_DB_PORT: int = Field(default=3306, ge=1, le=65535)
    DEBT_DB_USER: str = Field(...)
    DEBT_DB_PASSWORD: SecretStr = Field(...)

    DB_CONNECT_TIMEOUT_SECONDS: int = Field(default=5, ge=1, le=30)
    DB_READ_TIMEOUT_SECONDS: int = Field(default=15, ge=1, le=60)
    DB_WRITE_TIMEOUT_SECONDS: int = Field(default=15, ge=1, le=60)
    DB_CONNECT_ATTEMPTS: int = Field(default=2, ge=1, le=3)

    model_config = SettingsConfigDict(env_file=".env", extra="ignore")


class ParsingSettings(BaseSettings):
    MAIN_URL: str = "https://www.mauniver.ru"
    EIOS_AUTH_URL: str = "https://eios.mauniver.ru/moodle/webservice/rest/server.php"
    CONTACTS_PATH: str = "/structure/phones"
    TEACHER_PATH: str = "/sveden/employees"
    API_DEPTS_URL: str = "/api/get_departments.php"
    API_CONTACTS_URL: str = "/api/get_contacts.php"
    NEWS_URL: str = "/press/news/rss/"
    EVENTS_URL: str = "/press/information/rss/"
    SPORTS_URL: str = "/press/sport/rss/"
    DEPTS_URL: str = "/press/deps/rss/"
    INTERNATIONAL_URL: str = "/press/inter/rss/"
    SCIENCE_URL: str = "/press/science/rss/"
    STUDENTS_URL: str = "/press/community/rss/"
    OTHER_URL: str = "/press/smi/rss/"
    APPLICANT_URL: str = "/abit/news/rss/"
    CALENDAR_URL: str = "/press/calendar/rss/"
    model_config = SettingsConfigDict(env_file=".env", extra="ignore")

    @field_validator("MAIN_URL")
    @classmethod
    def validate_main_origin(cls, value: str) -> str:
        return _https_url(value, origin_only=True)

    @field_validator("EIOS_AUTH_URL")
    @classmethod
    def validate_auth_url(cls, value: str) -> str:
        return _https_url(value, origin_only=False)

    @field_validator(
        "CONTACTS_PATH",
        "TEACHER_PATH",
        "API_DEPTS_URL",
        "API_CONTACTS_URL",
        "NEWS_URL",
        "EVENTS_URL",
        "SPORTS_URL",
        "DEPTS_URL",
        "INTERNATIONAL_URL",
        "SCIENCE_URL",
        "STUDENTS_URL",
        "OTHER_URL",
    )
    @classmethod
    def validate_relative_path(cls, value: str, info: ValidationInfo) -> str:
        return _relative_path(value, setting_name=info.field_name or "URL path")


class NetworkSettings(BaseSettings):
    HTTP_CONNECT_TIMEOUT_SECONDS: float = Field(default=5, ge=0.5, le=30)
    HTTP_READ_TIMEOUT_SECONDS: float = Field(default=15, ge=1, le=60)
    HTTP_POOL_SIZE: int = Field(default=20, ge=4, le=100)

    model_config = SettingsConfigDict(env_file=".env", extra="ignore")


class SecuritySettings(BaseSettings):
    AUTH_CACHE_TTL_SECONDS: int = Field(default=60, ge=0, le=300)
    AUTH_CACHE_MAX_ENTRIES: int = Field(default=512, ge=16, le=4096)
    AUTH_IP_RATE_LIMIT_REQUESTS: int = Field(default=20, ge=2, le=200)
    AUTH_USERNAME_RATE_LIMIT_REQUESTS: int = Field(default=8, ge=2, le=100)
    AUTH_RATE_LIMIT_WINDOW_SECONDS: int = Field(default=60, ge=10, le=3600)
    FORM_RATE_LIMIT_REQUESTS: int = Field(default=3, ge=1, le=100)
    FORM_RATE_LIMIT_WINDOW_SECONDS: int = Field(default=300, ge=10, le=86400)
    RATE_LIMIT_MAX_ENTRIES: int = Field(default=4096, ge=64, le=65536)

    model_config = SettingsConfigDict(env_file=".env", extra="ignore")


class AppSettings(BaseSettings):
    ENVIRONMENT: str = "production"
    ENABLE_DOCS: bool = False

    model_config = SettingsConfigDict(env_file=".env", extra="ignore")


config = DBSettings()
parser_config = ParsingSettings()
mail_config = MailSettings()
security_config = SecuritySettings()
app_config = AppSettings()
network_config = NetworkSettings()
