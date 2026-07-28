import datetime

from fastapi import HTTPException, Query

from apps.gateway.utils.parser import ParserType

MOSCOW_TIMEZONE = datetime.timezone(datetime.timedelta(hours=3))


def format_unix_time(unix_time: int | float) -> str:
    timestamp = datetime.datetime.fromtimestamp(unix_time, tz=MOSCOW_TIMEZONE)
    return timestamp.strftime("от %d %B %Y года")


def get_parser_type(news_type: int = Query(1)) -> ParserType:
    try:
        return ParserType.from_id(news_type)
    except ValueError:
        raise HTTPException(status_code=400, detail=f"Invalid news_type: {news_type}") from None
