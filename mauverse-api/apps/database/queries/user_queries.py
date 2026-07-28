from typing import Any

from apps.database.db_connect import debt_db_connect, schedule_db_connect
from apps.database.utils import execute_query


def get_user_info_query(username: str) -> dict[str, Any]:
    user_query = """
            SELECT roleid, groupname, username
            FROM lko.users
            WHERE username = %s
            LIMIT 2;
        """

    user_info = execute_query(debt_db_connect, user_query, (username,))

    if len(user_info) != 1:
        return {}

    result = {
        "roleid": user_info[0][0],
        "groupname": user_info[0][1],
        "username": user_info[0][2],
        "groupid": "",
        "speciality": "",
    }

    if not user_info[0][1]:
        return result
    group_info = get_group_id(result["groupname"])
    return {**result, **group_info}


def get_group_id(group_name: str) -> dict[str, Any]:
    result = {}
    group_query = """
                    SELECT UID, groups.group, speciality
                    FROM 1c.groups
                    WHERE groups.group = %s;
                """
    group_info = execute_query(schedule_db_connect, group_query, (group_name,))

    if len(group_info) == 1:
        result["groupid"] = group_info[0][0]
        result["speciality"] = group_info[0][2]
    else:
        result["error"] = "Группа не найдена"
    return result


def get_subgroups(group_name: str) -> dict[str, Any]:
    result = {}
    group_query = """
                SELECT UID, groups.group, speciality, UID_mg
                FROM 1c.groups
                WHERE groups.group LIKE %s
                Order by maingroup_id asc;
            """

    group_info = execute_query(schedule_db_connect, group_query, (f"%{group_name}%",))

    if len(group_info) > 0:
        result["groupid"] = group_info[0][0]
        result["speciality"] = group_info[0][2]

    result["subgroups"] = []

    if len(group_info) > 1:
        for group in group_info:
            if group[3] != "":
                result["subgroups"].append({"groupid": group[0], "name": group[1]})
    return result
