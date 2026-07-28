from pydantic import Field

from apps.gateway.models.users import ApiModel


class DeptInfo(ApiModel):
    debt_name: str = Field(min_length=1, max_length=200)
    next_element: str = Field(min_length=1, max_length=200)


class DeptInfoNew(ApiModel):
    name: str = Field(default="", max_length=200)
    department_id: int = Field(ge=1)
