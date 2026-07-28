import re

from pydantic import BaseModel, ConfigDict, Field, SecretStr, field_validator


class ApiModel(BaseModel):
    model_config = ConfigDict(extra="forbid")


class GroupName(ApiModel):
    group_name: str = Field(min_length=1, max_length=64)

    @field_validator("group_name")
    @classmethod
    def strip_group_name(cls, value: str) -> str:
        cleaned = value.strip()
        if not cleaned:
            raise ValueError("Группа не может быть пустой")
        return cleaned


class UserInfo(ApiModel):
    username: str = Field(min_length=1, max_length=200)

    @field_validator("username")
    @classmethod
    def strip_username(cls, value: str) -> str:
        cleaned = value.strip()
        if not cleaned:
            raise ValueError("Логин не может быть пустым")
        return cleaned


class Token(BaseModel):
    token: SecretStr = Field(min_length=1, max_length=4096)

    model_config = ConfigDict(extra="forbid")


class UserNotification(Token):
    user_id: int


class UserAuth(UserInfo):
    password: SecretStr = Field(min_length=1, max_length=256)


class TeacherName(ApiModel):
    teacher_name: str = Field(default="", max_length=200)

    @field_validator("teacher_name")
    @classmethod
    def strip_teacher_name(cls, value: str) -> str:
        return value.strip()


class CreditBook(ApiModel):
    credit_book: str | None = Field(default=None, max_length=64)


class TeacherInfo(ApiModel):
    first_name: str = Field(min_length=1, max_length=100)
    last_name: str = Field(min_length=1, max_length=100)
    second_name: str = Field(min_length=1, max_length=100)

    @field_validator("first_name", "last_name", "second_name")
    @classmethod
    def strip_name(cls, value: str) -> str:
        cleaned = value.strip()
        if not cleaned:
            raise ValueError("Часть имени не может быть пустой")
        return cleaned


class StudentFormField(ApiModel):
    title: str = Field(min_length=1, max_length=120)
    value: str = Field(min_length=1, max_length=2000)

    @field_validator("title", "value")
    @classmethod
    def strip_text(cls, value: str) -> str:
        cleaned = value.strip()
        if not cleaned:
            raise ValueError("Поле не может быть пустым")
        return cleaned


class StudentFormRequest(ApiModel):
    sender: str = Field(alias="from", min_length=5, max_length=254)
    username: str = Field(min_length=2, max_length=200)
    text: list[StudentFormField] = Field(min_length=1, max_length=24)

    model_config = ConfigDict(populate_by_name=True, extra="forbid")

    @field_validator("sender")
    @classmethod
    def validate_sender(cls, value: str) -> str:
        cleaned = value.strip()
        if not re.fullmatch(r"[^@\s]+@[^@\s]+\.[^@\s]+", cleaned):
            raise ValueError("Некорректный адрес отправителя")
        return cleaned

    @field_validator("username")
    @classmethod
    def strip_username(cls, value: str) -> str:
        cleaned = value.strip()
        if not cleaned:
            raise ValueError("Имя пользователя не может быть пустым")
        return cleaned
