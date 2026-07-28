import logging
from typing import Any, Protocol

import mysql.connector

from apps.gateway.errors import DatabaseUnavailableError

logger = logging.getLogger(__name__)


class DatabaseProvider(Protocol):
    database_name: str

    def connect(self) -> Any: ...


def execute_query(
    connection: DatabaseProvider,
    query: str,
    params: tuple[object, ...] | None,
) -> list[tuple[Any, ...]]:
    database = connection.connect()
    try:
        with database.cursor() as cursor:
            cursor.execute(query, params)
            return cursor.fetchall()
    except mysql.connector.Error as exc:
        logger.warning(
            "Database query failed database=%s failure_type=%s",
            connection.database_name,
            type(exc).__name__,
        )
        raise DatabaseUnavailableError("Database query failed") from exc
    finally:
        database.close()
