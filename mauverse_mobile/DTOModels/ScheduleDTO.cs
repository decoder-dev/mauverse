using mau.Models;

namespace mau.DTOModels;

public sealed class ScheduleDTO : BaseDTO
{
    public IEnumerable<Schedule> Schedules { get; set; } = [];
}
