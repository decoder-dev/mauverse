using mau.Models;

namespace mau.Utils.API.Interaface;

public interface IScheduleRequests
{
#pragma warning disable CA1707 // Existing ViewModels use these public parameter names as named arguments.
    Task<IEnumerable<Schedule>> GetSchedulesAsync(
        string group_id,
        string subgroup_id,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Schedule>> GetSchedulesAsync(
        string[] teacher,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Schedule>> GetSchedulesAsync(
        int room_id,
        CancellationToken cancellationToken = default);
#pragma warning restore CA1707
}
