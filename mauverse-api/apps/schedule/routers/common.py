from fastapi import APIRouter, FastAPI, status

from apps.gateway.routers.api import (
    check_teacher,
    get_all_rooms,
    get_groups,
    get_room,
    get_schedule,
)
from apps.gateway.utils.parser import contact_parser


def init(app: FastAPI) -> None:
    router = APIRouter(prefix="/schedule", tags=["schedule_api"])

    router.add_api_route(
        "/get_groups", methods=["GET", "POST"], status_code=status.HTTP_200_OK, endpoint=get_groups
    )

    router.add_api_route(
        "/get_rooms", methods=["GET", "POST"], status_code=status.HTTP_200_OK, endpoint=get_room
    )

    router.add_api_route(
        "/get_all_rooms",
        methods=["GET", "POST"],
        status_code=status.HTTP_200_OK,
        endpoint=get_all_rooms,
    )

    router.add_api_route(
        "/get_schedule",
        methods=["GET", "POST"],
        status_code=status.HTTP_200_OK,
        endpoint=get_schedule,
    )

    router.add_api_route(
        "/check_teacher",
        methods=["GET", "POST"],
        status_code=status.HTTP_200_OK,
        endpoint=check_teacher,
    )

    router.add_api_route(
        "/get_teacher_info",
        methods=["GET", "POST"],
        status_code=status.HTTP_200_OK,
        endpoint=contact_parser.get_person_info,
    )

    app.include_router(router)
