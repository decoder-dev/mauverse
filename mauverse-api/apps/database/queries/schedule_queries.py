from typing import Any

from apps.database.db_connect import schedule_db_connect
from apps.database.utils import execute_query
from apps.gateway.models.schedule import GroupSchedule, RoomSchedule, TeacherSchedule
from apps.gateway.utils.pair_number_converter import convert_pair_number


def get_groups_query(group_name: str) -> list[str]:
    query = """
        SELECT groups.group
        FROM 1c.groups
        WHERE groups.group LIKE %s
        LIMIT 5;
    """

    params = (f"%{group_name.upper()}%",)
    groups = execute_query(schedule_db_connect, query, params)
    return [group[0] for group in groups]


def get_group_query(group_name: str) -> bool:
    query = """
        SELECT groups.group
        FROM 1c.groups
        WHERE groups.group = %s
        LIMIT 1;
    """

    params = (group_name.upper(),)
    groups = execute_query(schedule_db_connect, query, params)

    return bool(groups)


def get_all_rooms_query(room_name: str) -> list[dict[str, Any]]:
    query = """
        SELECT rooms.room_id, rooms.room
        FROM 1c.rooms
        WHERE rooms.room LIKE %s;
    """

    params = (f"%{room_name.upper()}%",)
    rooms = execute_query(schedule_db_connect, query, params)

    return [{"roomid": room[0], "name": room[1]} for room in rooms]


def get_room_query(room_name: str) -> list[dict[str, Any]]:
    query = """
        SELECT rooms.room_id, rooms.room
        FROM 1c.rooms
        WHERE rooms.room LIKE %s
        LIMIT 5;
    """
    params = (f"%{room_name.upper()}%",)
    rooms = execute_query(schedule_db_connect, query, params)
    return [{"roomid": room[0], "name": room[1]} for room in rooms]


def get_schedule_by_group_query(schedule: GroupSchedule) -> list[dict[str, Any]]:
    if schedule.subgroup_id is not None:
        query = """
        SELECT 1c.disciplines.disc, 1c.rooms.room, 1c.teachers.teacher,
            pair,
            pair_type,
            pair_date,
            1c.groups.group,
            1c.schedule.id
        FROM 1c.schedule
        JOIN 1c.groups ON 1c.schedule.group_id = 1c.groups.group_id
        JOIN 1c.disciplines ON 1c.disciplines.disc_id = 1c.schedule.disc_id
        JOIN 1c.rooms ON 1c.schedule.room_id = 1c.rooms.room_id
        JOIN 1c.buildings ON 1c.rooms.bui_id = 1c.buildings.bui_id
        JOIN 1c.teachers ON 1c.teachers.teacher_id = 1c.schedule.teacher_id
        WHERE 1c.schedule.pair_date BETWEEN %s AND %s
          AND (1c.groups.UID = %s OR 1c.groups.UID = %s)
        ORDER BY pair_date, pair ASC;
        """
        params = (
            schedule.start_date,
            schedule.end_date,
            schedule.group_id,
            schedule.subgroup_id,
        )
    else:
        query = """
        SELECT 1c.disciplines.disc, 1c.rooms.room, 1c.teachers.teacher,
            pair,
            pair_type,
            pair_date,
            1c.groups.group,
            1c.schedule.id
        FROM 1c.schedule
        JOIN 1c.groups ON 1c.schedule.group_id = 1c.groups.group_id
        JOIN 1c.disciplines ON 1c.disciplines.disc_id = 1c.schedule.disc_id
        JOIN 1c.rooms ON 1c.schedule.room_id = 1c.rooms.room_id
        JOIN 1c.buildings ON 1c.rooms.bui_id = 1c.buildings.bui_id
        JOIN 1c.teachers ON 1c.teachers.teacher_id = 1c.schedule.teacher_id
        WHERE 1c.schedule.pair_date BETWEEN %s AND %s
          AND 1c.groups.UID = %s
        ORDER BY pair_date, pair ASC;
        """
        params = (schedule.start_date, schedule.end_date, schedule.group_id)
    pairs = execute_query(schedule_db_connect, query, params)
    result = []
    for pair in pairs:
        converted_pair_type = convert_pair_number(pair[3])
        current_pair = {
            "name": pair[0],
            "teacher": pair[2],
            "room": pair[1],
            "pairtype": pair[4],
            "starttime": converted_pair_type.start_date,
            "endtime": converted_pair_type.end_date,
            "date": pair[5],
            "externalid": pair[7],
        }
        result.append(current_pair)

    return result


def get_schedule_by_room_query(room: RoomSchedule) -> list[dict[str, Any]]:
    query = """
        SELECT 1c.disciplines.disc, 1c.rooms.room, 1c.teachers.teacher, pair, pair_type, pair_date
        FROM 1c.schedule
        JOIN 1c.groups ON 1c.schedule.group_id = 1c.groups.group_id
        JOIN 1c.disciplines ON 1c.disciplines.disc_id = 1c.schedule.disc_id
        JOIN 1c.rooms ON 1c.schedule.room_id = 1c.rooms.room_id
        JOIN 1c.buildings ON 1c.rooms.bui_id = 1c.buildings.bui_id
        JOIN 1c.teachers ON 1c.teachers.teacher_id = 1c.schedule.teacher_id
        WHERE pair_date BETWEEN %s AND %s
          AND 1c.rooms.room_id = %s
        ORDER BY pair_date, pair ASC;
    """

    params = (room.start_date, room.end_date, room.room_id)
    pairs = execute_query(schedule_db_connect, query, params)

    result = []
    for pair in pairs:
        converted_pair_type = convert_pair_number(pair[3])
        current_pair = {
            "name": pair[0],
            "teacher": pair[2],
            "room": pair[1],
            "pairtype": pair[4],
            "starttime": converted_pair_type.start_date,
            "endtime": converted_pair_type.end_date,
            "date": pair[5],
        }
        result.append(current_pair)

    return result


def get_schedule_by_teacher_query(schedule: TeacherSchedule) -> list[dict[str, Any]]:
    query = """
        SELECT 1c.disciplines.disc, GROUP_CONCAT(1c.groups.group),
            1c.rooms.room, pair, pair_type, pair_date
        FROM 1c.schedule
        JOIN 1c.groups ON 1c.schedule.group_id = 1c.groups.group_id
        JOIN 1c.disciplines ON 1c.disciplines.disc_id = 1c.schedule.disc_id
        JOIN 1c.rooms ON 1c.schedule.room_id = 1c.rooms.room_id
        JOIN 1c.buildings ON 1c.rooms.bui_id = 1c.buildings.bui_id
        JOIN 1c.teachers ON 1c.teachers.teacher_id = 1c.schedule.teacher_id
        WHERE pair_date BETWEEN %s AND %s
          AND 1c.teachers.teacher LIKE %s
          AND 1c.teachers.teacher LIKE %s
          AND 1c.teachers.teacher LIKE %s
        GROUP BY pair_date, pair
        ORDER BY pair_date, pair ASC;
    """

    params = (
        schedule.start_date,
        schedule.end_date,
        f"%{schedule.teacher_first_name}%",
        f"%{schedule.teacher_second_name}%",
        f"%{schedule.teacher_last_name}%",
    )

    pairs = execute_query(schedule_db_connect, query, params)

    result = []
    for pair in pairs:
        groups = ", ".join(pair[1].split(","))
        converted_pair_type = convert_pair_number(pair[3])
        current_pair = {
            "name": pair[0],
            "room": pair[2],
            "teacher": groups,
            "starttime": converted_pair_type.start_date,
            "endtime": converted_pair_type.end_date,
            "pairtype": pair[4],
            "date": pair[5],
        }
        result.append(current_pair)

    return result
