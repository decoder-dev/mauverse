from apps.database.db_connect import schedule_db_connect
from apps.database.utils import execute_query


def get_all_teachers_query(teacher_name: str) -> list[str]:
    query = """
        SELECT DISTINCT CONCAT(
            TRIM(SUBSTRING_INDEX(teacher, ' ', 1)), ' ',
            TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(teacher, ' ', 2), ' ', -1)), ' ',
            TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(teacher, ' ', 3), ' ', -1))
        ) AS teacher
        FROM 1c.teachers
        WHERE teachers.teacher LIKE %s;
    """

    params = (f"%{teacher_name.title()}%",)
    teachers = execute_query(schedule_db_connect, query, params)

    return [teacher[0] for teacher in teachers]


def get_teachers_query(teacher_name: str) -> list[str]:
    query = """
        SELECT DISTINCT CONCAT(
            TRIM(SUBSTRING_INDEX(teacher, ' ', 1)), ' ',
            TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(teacher, ' ', 2), ' ', -1)), ' ',
            TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(teacher, ' ', 3), ' ', -1))
        ) AS teacher
        FROM 1c.teachers
        WHERE teachers.teacher LIKE %s
        LIMIT 5;
    """

    params = (f"%{teacher_name.title()}%",)
    teachers = execute_query(schedule_db_connect, query, params)

    return [teacher[0] for teacher in teachers]


def get_teacher_query(teacher_name: str) -> bool:
    query = """
        SELECT DISTINCT CONCAT(
            TRIM(SUBSTRING_INDEX(teacher, ' ', 1)), ' ',
            TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(teacher, ' ', 2), ' ', -1)), ' ',
            TRIM(SUBSTRING_INDEX(SUBSTRING_INDEX(teacher, ' ', 3), ' ', -1))
        ) AS teacher
        FROM 1c.teachers
        WHERE teachers.teacher = %s
        LIMIT 1;
    """

    params = (teacher_name.title(),)
    result = execute_query(schedule_db_connect, query, params)

    return bool(result)
