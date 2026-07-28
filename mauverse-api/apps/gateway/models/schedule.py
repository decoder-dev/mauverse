from datetime import date
from typing import Self

from pydantic import Field, model_validator

from apps.gateway.models.users import ApiModel


class BaseSchedule(ApiModel):
    start_date: date
    end_date: date

    @model_validator(mode="after")
    def validate_period(self) -> Self:
        period = (self.end_date - self.start_date).days
        if period < 0:
            raise ValueError("Дата окончания раньше даты начала")
        if period > 180:
            raise ValueError("Период расписания не может превышать 180 дней")
        return self


class GroupSchedule(BaseSchedule):
    group_id: str | None = Field(default=None, max_length=64)
    subgroup_id: str | None = Field(default=None, max_length=64)


class RoomSchedule(BaseSchedule):
    room_id: str | None = Field(default=None, max_length=64)


class TeacherSchedule(BaseSchedule):
    teacher_first_name: str | None = Field(default=None, max_length=100)
    teacher_second_name: str | None = Field(default=None, max_length=100)
    teacher_last_name: str | None = Field(default=None, max_length=100)


class ScheduleDTO(TeacherSchedule, RoomSchedule, GroupSchedule):
    """Combined schedule request accepted by the legacy mobile contract."""


class Room(ApiModel):
    room_name: str = Field(default="", max_length=100)
