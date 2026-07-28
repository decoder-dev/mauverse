from typing import Any

from apps.database.db_connect import debt_db_connect
from apps.database.utils import execute_query


def get_credit_book_owners_query(credit_book: str) -> list[dict[str, Any]]:
    query = """
            SELECT DISTINCT
                students.student_id,
                students.surname,
                students.name,
                students.middle_name,
                students.study_group,
                (
                    SELECT COUNT(DISTINCT identity_student.student_id)
                    FROM lko.students AS identity_student
                    WHERE identity_student.surname <=> students.surname
                      AND identity_student.name <=> students.name
                      AND identity_student.middle_name <=> students.middle_name
                      AND identity_student.study_group <=> students.study_group
                ) AS identity_matches
            FROM lko.grade
            JOIN lko.students ON grade.student_id = students.student_id
            WHERE grade.credit_book = %s;
        """
    owners = execute_query(debt_db_connect, query, (credit_book,))

    return [
        {
            "student_id": owner[0],
            "surname": owner[1],
            "name": owner[2],
            "middle_name": owner[3],
            "study_group": owner[4],
            "identity_matches": owner[5],
        }
        for owner in owners
    ]


def get_semester_debts_by_credit_book_query(
    credit_book: str, student_id: int
) -> list[dict[str, Any]]:
    query = """
            SELECT semester, semester_number
            FROM lko.grade
            JOIN lko.students ON grade.student_id = students.student_id
            WHERE grade.credit_book = %s
              AND students.student_id = %s
            GROUP BY semester, semester_number;
        """
    debts = execute_query(debt_db_connect, query, (credit_book, student_id))

    return [{"semester": debt[0], "semesternumber": debt[1]} for debt in debts]


def get_debts_by_credit_book_query(
    semester_number: int, credit_book: str, student_id: int
) -> list[dict[str, Any]]:
    query = """
            SELECT semester, semester_number, discipline, mark_type
            FROM lko.grade
            JOIN lko.students ON grade.student_id = students.student_id
            WHERE grade.credit_book = %s
              AND students.student_id = %s
              AND grade.semester_number = %s;
        """
    params = (credit_book, student_id, semester_number)
    debts = execute_query(debt_db_connect, query, params)

    return [{"discipline": debt[2], "marktype": debt[3]} for debt in debts]


def _get_latest_semester(group_name: str) -> int | None:
    query = """
            SELECT semester_number
            FROM lko.grade, lko.students
            WHERE grade.student_id = students.student_id
              AND study_group = %s
            GROUP BY semester_number
            ORDER BY semester_number;
        """
    params = (group_name,)
    semesters = execute_query(debt_db_connect, query, params)

    if not semesters:
        return None

    semester_number = semesters[-1][0]
    return int(semester_number) if semester_number is not None else None


def get_total_debts_by_group_query(group_name: str) -> list[dict[str, Any]]:
    semester_number = _get_latest_semester(group_name)
    if semester_number is None:
        return []

    query = """
        SELECT surname, name, middle_name, COUNT(discipline)
        FROM lko.grade, lko.students
        WHERE grade.student_id = students.student_id AND
              study_group = %s AND
              semester_number <= %s
        GROUP BY surname, name, middle_name;
    """

    params = (group_name, semester_number)
    students = execute_query(debt_db_connect, query, params)

    result = []
    for student in students:
        student_debt = {
            "firstname": student[0],
            "name": student[1],
            "lastname": student[2],
            "totaldebts": student[3],
        }
        result.append(student_debt)

    return result


def get_semester_debts_by_group_query(
    surname: str,
    name: str,
    middle_name: str,
    group_name: str,
) -> list[dict[str, Any]]:
    query = """
        SELECT semester, semester_number
        FROM lko.grade, lko.students
        WHERE grade.student_id = students.student_id AND
              study_group = %s AND
              surname = %s AND
              name = %s AND
              middle_name = %s
        GROUP BY semester;
    """

    params = (group_name, surname, name, middle_name)
    debts = execute_query(debt_db_connect, query, params)

    result = []
    for debt in debts:
        student_debt = {
            "semester": debt[0],
            "semesternumber": debt[1],
        }
        result.append(student_debt)

    return result


def get_debts_by_student_and_group_query(
    surname: str, name: str, middle_name: str, group_name: str, semester_number: int
) -> list[dict[str, Any]]:
    query = """
        SELECT semester, semester_number, discipline, mark_type
        FROM lko.grade
        JOIN lko.students ON grade.student_id = students.student_id
        WHERE study_group = %s AND
              surname = %s AND
              name = %s AND
              middle_name = %s AND
              semester_number = %s;
    """

    params = (group_name, surname, name, middle_name, semester_number)
    debts = execute_query(debt_db_connect, query, params)

    result = []
    for debt in debts:
        student_debt = {
            "discipline": debt[2],
            "marktype": debt[3],
        }
        result.append(student_debt)

    return result
