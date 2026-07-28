using System.ComponentModel.DataAnnotations.Schema;

namespace mau.Models;

public sealed class ButtonParameters
{
    public Border Button { get; init; } = null!;
    public int Id { get; init; }
}

public class Schedule
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Teacher { get; set; } = string.Empty;
    public string Room { get; set; } = string.Empty;
    public string PairType { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int ExternalId { get; set; }

    [NotMapped]
    public bool HaveNote { get; set; }
}

public sealed class ScheduleNote : Schedule
{
}
