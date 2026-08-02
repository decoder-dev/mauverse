import unittest

from apps.gateway.utils.parser import ParserType


class ParserTypeTests(unittest.TestCase):
    def test_applicant_and_calendar_feeds_are_registered(self) -> None:
        self.assertEqual(ParserType.APPLICANT.id, 8)
        self.assertEqual(ParserType.CALENDAR.id, 9)
        self.assertEqual(ParserType.APPLICANT.url, "/abit/news/rss/")
        self.assertEqual(ParserType.CALENDAR.url, "/press/calendar/rss/")

    def test_from_id_resolves_new_feeds(self) -> None:
        self.assertIs(ParserType.from_id(8), ParserType.APPLICANT)
        self.assertIs(ParserType.from_id(9), ParserType.CALENDAR)


if __name__ == "__main__":
    unittest.main()
