namespace mau.DTOModels;

public sealed class WeekDaysDTO
{
    public string DayName { get; set; } = string.Empty;
    public string Day { get; set; } = string.Empty;
    public string Month { get; set; } = string.Empty;
    public int DayNumber { get; set; }
    public DateTime Date { get; set; }
    public bool IsWeekend { get; set; }
}
