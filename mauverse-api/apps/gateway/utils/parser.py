import enum
import html
import re
from typing import Any
from urllib.parse import quote

from babel.dates import format_date
from bs4 import BeautifulSoup
from dateutil import parser as date_parser

from apps.database.settings import parser_config
from apps.gateway.errors import UpstreamResponseError
from apps.gateway.models.dept import DeptInfo, DeptInfoNew
from apps.gateway.models.users import TeacherInfo
from apps.gateway.utils.http_client import create_http_session, request_timeout

CYRILLIC_TO_LATIN = str.maketrans(
    {
        "п": "p",
        "е": "e",
        "т": "t",
        "р": "r",
        "а": "a",
        "б": "b",
        "в": "v",
        "г": "g",
        "д": "d",
        "ё": "yo",
        "ж": "zh",
        "з": "z",
        "и": "i",
        "й": "y",
        "к": "k",
        "л": "l",
        "м": "m",
        "н": "n",
        "о": "o",
        "с": "s",
        "у": "u",
        "ф": "f",
        "х": "kh",
        "ц": "ts",
        "ч": "ch",
        "ш": "sh",
        "щ": "shch",
        "ъ": '"',
        "ы": "y",
        "ь": "'",
        "э": "e",
        "ю": "yu",
        "я": "ya",
    }
)


class ParserType(enum.Enum):
    DEFAULT = 0, parser_config.NEWS_URL
    DEPTS = 1, parser_config.DEPTS_URL
    SPORTS = 2, parser_config.SPORTS_URL
    STUDENTS = 3, parser_config.STUDENTS_URL
    SCIENCE = 4, parser_config.SCIENCE_URL
    INTERNATIONAL = 5, parser_config.INTERNATIONAL_URL
    EVENTS = 6, parser_config.EVENTS_URL
    OTHER = 7, parser_config.OTHER_URL
    APPLICANT = 8, parser_config.APPLICANT_URL
    CALENDAR = 9, parser_config.CALENDAR_URL

    def __init__(self, parser_id: int, url: str) -> None:
        self._id = parser_id
        self._url = url

    @property
    def id(self) -> int:
        return self._id

    @property
    def url(self) -> str:
        return self._url

    @classmethod
    def from_id(cls, parser_id: int) -> "ParserType":
        for member in cls:
            if member.id == parser_id:
                return member
        raise ValueError(f"Invalid id {parser_id} for {cls.__name__}")

    def __str__(self) -> str:
        return self.name


class ContactsParser:
    def __init__(self) -> None:
        self.table_url = f"{parser_config.MAIN_URL}{parser_config.CONTACTS_PATH}"
        self.person_url = f"{parser_config.MAIN_URL}{parser_config.TEACHER_PATH}"
        self.depts_url = f"{parser_config.MAIN_URL}{parser_config.API_DEPTS_URL}"
        self.contacts_url = f"{parser_config.MAIN_URL}{parser_config.API_CONTACTS_URL}"
        self._http = create_http_session()

    def close(self) -> None:
        self._http.close()

    def get_table_page(self) -> BeautifulSoup:
        response = self._http.get(self.table_url, timeout=request_timeout())
        response.raise_for_status()
        return BeautifulSoup(response.text, "html.parser")

    def get_person_info(self, name: TeacherInfo) -> dict[str, Any]:
        formatted_name = (
            f"{name.first_name.lower()}{name.second_name.lower()[0]}{name.last_name.lower()[0]}"
        )
        search_text = formatted_name.translate(CYRILLIC_TO_LATIN)
        response = self._http.get(
            f"{self.person_url}/{quote(search_text, safe='')}",
            timeout=request_timeout(),
        )
        response.raise_for_status()

        soup = BeautifulSoup(response.text, "html.parser")
        heading = soup.find("h1")
        if heading is None:
            raise UpstreamResponseError("Teacher page does not contain a heading")

        info_container = soup.find("div", class_="about-txt")
        if info_container is None:
            return {"Error": "Не найдена карточка преподавателя"}

        info = info_container.find_all("p")
        additional_info = soup.find_all("div", class_="desc")[:4]
        extras = [
            ":\n".join(paragraph.text for paragraph in additional.find_all("p"))
            for additional in additional_info
        ]
        return {
            "name": heading.text,
            "post": info[0].text.title() if info else "Не указано",
            "email": info[-1].text if info else "Не указано",
            "extras": "\n\n".join(extras),
        }

    def get_depts(self) -> list[dict[str, str]]:
        rows = [
            row
            for row in self.get_table_page().find_all("tr", class_="dep")
            if row.find("td", class_="title")
        ]
        departments: list[dict[str, str]] = []
        for index, row in enumerate(rows):
            title = row.find("td", class_="title")
            if title is None:
                continue
            next_element = rows[index + 1].get("id", "-1") if index + 1 < len(rows) else "-1"
            departments.append(
                {
                    "title": title.text,
                    "nextelement": str(next_element),
                }
            )
        return departments

    def get_contacts(self, dept: DeptInfo) -> list[dict[str, str]]:
        rows = [
            row
            for row in self.get_table_page().find_all("tr")
            if not row.find("td", class_="title2")
        ]
        start_index = 0
        end_index = len(rows)
        for index, row in enumerate(rows):
            if row.get("id", False) == dept.next_element:
                end_index = index
                break
            if row.find("td", class_="title") and dept.debt_name in row.text:
                start_index = index + 1

        people: list[dict[str, str]] = []
        for row in rows[start_index:end_index]:
            cells = row.find_all("td")
            if len(cells) < 4:
                continue
            person = {
                "post": cells[0].text,
                "name": cells[1].text,
                "email": cells[3].text,
            }
            if cells[2].text not in ("\xa0", "\xa0\xa0"):
                person["telephone"] = cells[2].text
            if person["post"] and person["name"] and (person["email"] or person.get("telephone")):
                people.append(person)
        return people

    def get_depts_json(self) -> list[Any]:
        response = self._http.get(self.depts_url, timeout=request_timeout())
        response.raise_for_status()
        payload = response.json()
        if not isinstance(payload, list):
            raise UpstreamResponseError("Department API returned an invalid response shape")
        return payload

    def get_contacts_json(self, dept: DeptInfoNew) -> list[dict[str, Any]]:
        response = self._http.get(self.contacts_url, timeout=request_timeout())
        response.raise_for_status()
        payload = response.json()
        if not isinstance(payload, list):
            raise UpstreamResponseError("Contacts API returned an invalid response shape")
        return [
            person
            for person in payload
            if isinstance(person, dict) and person.get("departmentId") == dept.department_id
        ]

    def get_teacher_info_json(self, teacher: TeacherInfo) -> list[Any] | dict[str, Any]:
        response = self._http.get(
            self.contacts_url,
            params={"person": f"{teacher.first_name} {teacher.last_name} {teacher.second_name}"},
            timeout=request_timeout(),
        )
        response.raise_for_status()
        payload = response.json()
        if not isinstance(payload, list | dict):
            raise UpstreamResponseError("Teacher API returned an invalid response shape")
        return payload


_THIN_SPACES = (
    "\xa0",  # nbsp
    "\u2002",  # ensp
    "\u2003",  # emsp
    "\u2009",  # thinsp
    "\u200a",
    "\u202f",
    "\u205f",
)


def _clean_rss_text(value: str) -> str:
    text = html.unescape(value or "")
    for space in _THIN_SPACES:
        text = text.replace(space, " ")
    text = re.sub(r"<[^>]+>", "", text)
    text = re.sub(r"\s+", " ", text).strip()
    return text.replace("\n", "")


class RssParser:
    def __init__(self) -> None:
        self.base_url = parser_config.MAIN_URL
        self._http = create_http_session()

    def close(self) -> None:
        self._http.close()

    def get_rss_data(self, parser_type: ParserType) -> list[dict[str, Any]]:
        response = self._http.get(
            f"{self.base_url}{parser_type.url}",
            timeout=request_timeout(),
        )
        response.raise_for_status()

        document = BeautifulSoup(response.text, features="xml")
        result: list[dict[str, Any]] = []
        for item in document.find_all("item")[:100]:
            image = item.find("enclosure")
            title_node = item.find("title")
            link_node = item.find("link")
            description_node = item.find("description")
            date_node = item.find("pubDate")
            if any(node is None for node in (title_node, link_node, description_node, date_node)):
                continue

            title = _clean_rss_text(title_node.text)
            description = _clean_rss_text(description_node.text)
            try:
                local_date = date_parser.parse(date_node.text)
            except (OverflowError, ValueError):
                continue

            result.append(
                {
                    "title": title,
                    "link": link_node.text,
                    "description": description,
                    "publish": format_date(local_date, "d LLL", locale="ru"),
                    "image": image.get("url") if image else "placeholder.png",
                }
            )
        return result


contact_parser = ContactsParser()
rss_parser = RssParser()
