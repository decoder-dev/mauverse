from fastapi import APIRouter, FastAPI, status

from apps.gateway.routers.api import (
    auth,
    check_group,
    check_teacher,
    get_all_rooms,
    get_all_teachers,
    get_debts,
    get_groups,
    get_news,
    get_notifications,
    get_room,
    get_schedule,
    get_semester,
    get_session,
    get_subgroup_info,
    get_teachers,
    get_user_info,
    send_order,
)
from apps.gateway.utils.parser import contact_parser


def init(app: FastAPI) -> None:
    router = APIRouter(prefix="/dev/mauverse", tags=["main_api"])

    router.add_api_route("/auth", methods=["POST"], status_code=status.HTTP_200_OK, endpoint=auth)

    router.add_api_route(
        "/session", methods=["GET"], status_code=status.HTTP_200_OK, endpoint=get_session
    )

    router.add_api_route(
        "/get_notifications",
        methods=["POST"],
        status_code=status.HTTP_200_OK,
        endpoint=get_notifications,
    )

    router.add_api_route(
        "/send_order", methods=["POST"], status_code=status.HTTP_200_OK, endpoint=send_order
    )

    router.add_api_route(
        "/get_user_info", methods=["POST"], status_code=status.HTTP_200_OK, endpoint=get_user_info
    )

    router.add_api_route(
        "/news", methods=["GET"], status_code=status.HTTP_200_OK, endpoint=get_news
    )

    router.add_api_route(
        "/get_subgroups",
        methods=["POST"],
        status_code=status.HTTP_200_OK,
        endpoint=get_subgroup_info,
    )

    router.add_api_route(
        "/get_groups", methods=["POST"], status_code=status.HTTP_200_OK, endpoint=get_groups
    )

    router.add_api_route(
        "/check_group", methods=["POST"], status_code=status.HTTP_200_OK, endpoint=check_group
    )

    router.add_api_route(
        "/get_rooms", methods=["POST"], status_code=status.HTTP_200_OK, endpoint=get_room
    )

    router.add_api_route(
        "/get_schedule", methods=["POST"], status_code=status.HTTP_200_OK, endpoint=get_schedule
    )

    router.add_api_route(
        "/get_teachers", methods=["POST"], status_code=status.HTTP_200_OK, endpoint=get_teachers
    )

    router.add_api_route(
        "/get_all_teachers",
        methods=["POST"],
        status_code=status.HTTP_200_OK,
        endpoint=get_all_teachers,
    )

    router.add_api_route(
        "/get_all_rooms", methods=["POST"], status_code=status.HTTP_200_OK, endpoint=get_all_rooms
    )

    router.add_api_route(
        "/check_teacher", methods=["POST"], status_code=status.HTTP_200_OK, endpoint=check_teacher
    )

    router.add_api_route(
        "/get_teacher_info",
        methods=["POST"],
        status_code=status.HTTP_200_OK,
        endpoint=contact_parser.get_person_info,
    )

    router.add_api_route(
        "/get_teacher_info_new",
        methods=["POST"],
        status_code=status.HTTP_200_OK,
        endpoint=contact_parser.get_teacher_info_json,
    )

    router.add_api_route(
        "/get_contacts",
        methods=["POST"],
        status_code=status.HTTP_200_OK,
        endpoint=contact_parser.get_contacts,
    )

    router.add_api_route(
        "/get_debts", methods=["POST"], status_code=status.HTTP_200_OK, endpoint=get_debts
    )

    router.add_api_route(
        "/get_semesters", methods=["POST"], status_code=status.HTTP_200_OK, endpoint=get_semester
    )

    router.add_api_route(
        "/get_depts",
        methods=["GET"],
        status_code=status.HTTP_200_OK,
        endpoint=contact_parser.get_depts,
    )

    router.add_api_route(
        "/get_depts_json",
        methods=["GET"],
        status_code=status.HTTP_200_OK,
        endpoint=contact_parser.get_depts_json,
    )

    router.add_api_route(
        "/get_contacts_json",
        methods=["POST"],
        status_code=status.HTTP_200_OK,
        endpoint=contact_parser.get_contacts_json,
    )
    app.include_router(router)
