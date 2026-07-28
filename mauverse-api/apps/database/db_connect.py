import logging
import time

import mysql.connector
from mysql.connector.abstracts import MySQLConnectionAbstract

from apps.database.settings import config
from apps.gateway.errors import DatabaseUnavailableError

logger = logging.getLogger(__name__)


class ScheduleDBConnect:
    database_name = "schedule"

    def connect(self, attempts: int | None = None) -> MySQLConnectionAbstract:
        max_attempts = attempts if attempts is not None else config.DB_CONNECT_ATTEMPTS
        for attempt in range(max_attempts):
            try:
                return mysql.connector.connect(
                    host=config.SCHEDULE_DB_HOST,
                    port=config.SCHEDULE_DB_PORT,
                    user=config.SCHEDULE_DB_USER,
                    password=config.SCHEDULE_DB_PASSWORD.get_secret_value(),
                    database=config.SCHEDULE_DB_NAME,
                    connection_timeout=config.DB_CONNECT_TIMEOUT_SECONDS,
                    read_timeout=config.DB_READ_TIMEOUT_SECONDS,
                    write_timeout=config.DB_WRITE_TIMEOUT_SECONDS,
                )
            except mysql.connector.Error as exc:
                logger.warning(
                    "Database connection failed database=%s failure_type=%s attempt=%s",
                    self.database_name,
                    type(exc).__name__,
                    attempt + 1,
                )
                if attempt + 1 < max_attempts:
                    time.sleep(0.25 * (attempt + 1))
        raise DatabaseUnavailableError("Schedule database is unavailable")

    def is_ready(self) -> bool:
        database = None
        try:
            database = self.connect(attempts=1)
            with database.cursor() as cursor:
                cursor.execute("SELECT 1")
                cursor.fetchone()
            return True
        except (DatabaseUnavailableError, mysql.connector.Error):
            return False
        finally:
            if database is not None:
                database.close()


class DeptDBConnect:
    database_name = "debt"

    def connect(self, attempts: int | None = None) -> MySQLConnectionAbstract:
        max_attempts = attempts if attempts is not None else config.DB_CONNECT_ATTEMPTS
        for attempt in range(max_attempts):
            try:
                return mysql.connector.connect(
                    host=config.DEBT_DB_HOST,
                    port=config.DEBT_DB_PORT,
                    user=config.DEBT_DB_USER,
                    password=config.DEBT_DB_PASSWORD.get_secret_value(),
                    database=config.DEBT_DB_NAME,
                    connection_timeout=config.DB_CONNECT_TIMEOUT_SECONDS,
                    read_timeout=config.DB_READ_TIMEOUT_SECONDS,
                    write_timeout=config.DB_WRITE_TIMEOUT_SECONDS,
                )
            except mysql.connector.Error as exc:
                logger.warning(
                    "Database connection failed database=%s failure_type=%s attempt=%s",
                    self.database_name,
                    type(exc).__name__,
                    attempt + 1,
                )
                if attempt + 1 < max_attempts:
                    time.sleep(0.25 * (attempt + 1))
        raise DatabaseUnavailableError("Debt database is unavailable")

    def is_ready(self) -> bool:
        database = None
        try:
            database = self.connect(attempts=1)
            with database.cursor() as cursor:
                cursor.execute("SELECT 1")
                cursor.fetchone()
            return True
        except (DatabaseUnavailableError, mysql.connector.Error):
            return False
        finally:
            if database is not None:
                database.close()


debt_db_connect = DeptDBConnect()
schedule_db_connect = ScheduleDBConnect()
