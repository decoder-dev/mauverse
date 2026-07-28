using System.Globalization;
using mau.DTOModels;
using mau.Models;
using mau.Utils.API.Interaface;
using mau.Utils.Services.Interface;

namespace mau.Utils.API;

public sealed class ScheduleRequests(IAPIService apiService) : IScheduleRequests, IDisposable
{
#pragma warning disable CA1707 // Existing ViewModels use these public parameter names as named arguments.
    public Task<IEnumerable<Schedule>> GetSchedulesAsync(
        string group_id,
        string subgroup_id,
        CancellationToken cancellationToken = default) =>
        GetScheduleAsync(
            groupId: group_id,
            subgroupId: subgroup_id,
            cancellationToken: cancellationToken);

    public Task<IEnumerable<Schedule>> GetSchedulesAsync(
        string[] teacher,
        CancellationToken cancellationToken = default) =>
        GetScheduleAsync(teacher: teacher, cancellationToken: cancellationToken);

    public Task<IEnumerable<Schedule>> GetSchedulesAsync(
        int room_id,
        CancellationToken cancellationToken = default) =>
        GetScheduleAsync(roomId: room_id, cancellationToken: cancellationToken);
#pragma warning restore CA1707

    private async Task<IEnumerable<Schedule>> GetScheduleAsync(
        string groupId = "",
        string subgroupId = "",
        string[]? teacher = null,
        int roomId = 0,
        CancellationToken cancellationToken = default)
    {
        var mondayDate = GetWeekStart(DateTime.Now);
        var monday = mondayDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var sunday = mondayDate.AddDays(13).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var request = CreateScheduleRequest(monday, sunday, groupId, subgroupId, teacher, roomId);
        var schedule = await apiService.PostAsync<ScheduleDTO>("/get_schedule", request, cancellationToken);
        return schedule.Schedules;
    }

    private static DateTime GetWeekStart(DateTime date)
    {
        var difference = date.DayOfWeek == DayOfWeek.Sunday
            ? -6
            : DayOfWeek.Monday - date.DayOfWeek;
        return date.Date.AddDays(difference);
    }

    private static Dictionary<string, string> CreateScheduleRequest(
        string monday,
        string sunday,
        string groupId,
        string subgroupId,
        string[]? teacher,
        int roomId)
    {
        var request = new Dictionary<string, string>
        {
            ["start_date"] = monday,
            ["end_date"] = sunday
        };

        if (!string.IsNullOrEmpty(groupId))
            request["group_id"] = groupId;
        if (!string.IsNullOrEmpty(subgroupId))
            request["subgroup_id"] = subgroupId;
        if (roomId != 0)
            request["room_id"] = roomId.ToString(CultureInfo.InvariantCulture);

        if (teacher is { Length: > 0 })
        {
            request["teacher_first_name"] = teacher[0];
            request["teacher_second_name"] = teacher.Length > 1 ? teacher[1] : string.Empty;
            if (teacher.Length > 2)
                request["teacher_last_name"] = teacher[2];
        }

        return request;
    }

    public void Dispose() => GC.SuppressFinalize(this);
}
