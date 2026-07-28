from pydantic import Field

from apps.gateway.models.users import CreditBook, GroupName


class StudentDebtInfo(GroupName):
    group_name: str | None = Field(default=None, max_length=64)
    first_name: str | None = Field(default=None, max_length=100)
    name: str | None = Field(default=None, max_length=100)
    last_name: str | None = Field(default=None, max_length=100)


class SemesterDTO(StudentDebtInfo, CreditBook):
    """Semester lookup request shared by student and curator flows."""


class DebtDTO(StudentDebtInfo, CreditBook):
    semester_number: int | None = Field(default=None, ge=1, le=20)
