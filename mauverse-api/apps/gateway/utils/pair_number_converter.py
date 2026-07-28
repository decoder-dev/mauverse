from pydantic import BaseModel


class PairConverter(BaseModel):
    start_date: str
    end_date: str


def convert_pair_number(pair_number: int) -> PairConverter:
    times = {
        1: ("09:00", "10:35"),
        2: ("10:45", "12:20"),
        3: ("12:40", "14:15"),
        4: ("14:45", "16:20"),
        5: ("16:30", "18:05"),
        6: ("18:15", "19:50"),
        7: ("20:00", "21:35"),
    }
    if pair_number not in times:
        raise ValueError(f"Unknown pair number: {pair_number}")
    start_date, end_date = times[pair_number]
    return PairConverter(start_date=start_date, end_date=end_date)
