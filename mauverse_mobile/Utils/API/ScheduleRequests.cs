using System.Globalization;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using mau.DTOModels;
using mau.Models;
using mau.Utils.API.Interaface;
using mau.Utils.Services.Interface;

namespace mau.Utils.API;

public sealed class ScheduleRequests : IScheduleRequests, IDisposable
{
    private static readonly Uri ScheduleBaseUri = new("https://api-schedule.mauniver.ru/");
    private static readonly HttpClient ScheduleTransport = new()
    {
        Timeout = TimeSpan.FromSeconds(25)
    };
    private static readonly JsonSerializerOptions ScheduleJson = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IAPIService _legacyApi;
    private readonly string _scheduleToken;

    public ScheduleRequests(IAPIService apiService)
    {
        _legacyApi = apiService;
        _scheduleToken = typeof(ScheduleRequests).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(attribute => attribute.Key == "MauverseScheduleToken")
            ?.Value
            ?.Trim() ?? string.Empty;
    }

#pragma warning disable CA1707
    public async Task<IEnumerable<Schedule>> GetSchedulesAsync(
        string group_id,
        string subgroup_id,
        CancellationToken cancellationToken = default)
    {
        var scheduleUid = !string.IsNullOrWhiteSpace(subgroup_id)
            ? subgroup_id.Trim()
            : group_id.Trim();
        if (!string.IsNullOrWhiteSpace(_scheduleToken) && !string.IsNullOrWhiteSpace(scheduleUid))
        {
            try
            {
                return await GetCurrentScheduleAsync(scheduleUid, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                // The authenticated MAUverse API remains a compatibility fallback
                // for accounts whose group identifier has not yet been migrated to UID.
                System.Diagnostics.Debug.WriteLine(exception);
            }
        }

        return await GetLegacyScheduleAsync(
            groupId: group_id,
            subgroupId: subgroup_id,
            cancellationToken: cancellationToken);
    }

    public Task<IEnumerable<Schedule>> GetSchedulesAsync(
        string[] teacher,
        CancellationToken cancellationToken = default) =>
        GetLegacyScheduleAsync(teacher: teacher, cancellationToken: cancellationToken);

    public Task<IEnumerable<Schedule>> GetSchedulesAsync(
        int room_id,
        CancellationToken cancellationToken = default) =>
        GetLegacyScheduleAsync(roomId: room_id, cancellationToken: cancellationToken);
#pragma warning restore CA1707

    private async Task<IEnumerable<Schedule>> GetCurrentScheduleAsync(
        string groupUid,
        CancellationToken cancellationToken)
    {
        var monday = GetWeekStart(DateTime.Now);
        var start = monday.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var end = monday.AddDays(13).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var escapedUid = Uri.EscapeDataString(groupUid);
        var requestUri = new Uri(
            ScheduleBaseUri,
            $"groups/{escapedUid}/schedule/{start}/{end}");

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _scheduleToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await SendScheduleAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"API расписания вернул ошибку {(int)response.StatusCode}",
                null,
                response.StatusCode);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<ScheduleApiResponse>(
            stream,
            ScheduleJson,
            cancellationToken);
        if (payload is null || payload.Success == false)
            throw new InvalidDataException("API расписания вернул некорректный ответ");

        return payload.Timetable.Select(MapSchedule).ToArray();
    }

    private static async Task<HttpResponseMessage> SendScheduleAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage? lastResponse = null;
        for (var attempt = 0; attempt < 2; attempt++)
        {
            using var attemptRequest = await CloneAsync(request, cancellationToken);
            try
            {
                var response = await ScheduleTransport.SendAsync(
                    attemptRequest,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);
                if (attempt == 0 &&
                    response.StatusCode is System.Net.HttpStatusCode.BadGateway or
                        System.Net.HttpStatusCode.ServiceUnavailable or
                        System.Net.HttpStatusCode.GatewayTimeout)
                {
                    lastResponse?.Dispose();
                    lastResponse = response;
                    await Task.Delay(350, cancellationToken);
                    continue;
                }

                lastResponse?.Dispose();
                return response;
            }
            catch (HttpRequestException) when (attempt == 0)
            {
                await Task.Delay(350, cancellationToken);
            }
        }

        return lastResponse ?? throw new HttpRequestException("API расписания недоступен");
    }

    private static async Task<HttpRequestMessage> CloneAsync(
        HttpRequestMessage source,
        CancellationToken cancellationToken)
    {
        var clone = new HttpRequestMessage(source.Method, source.RequestUri);
        foreach (var header in source.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        if (source.Content is not null)
        {
            var bytes = await source.Content.ReadAsByteArrayAsync(cancellationToken);
            clone.Content = new ByteArrayContent(bytes);
            foreach (var header in source.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
        return clone;
    }

    private static Schedule MapSchedule(ScheduleApiItem item)
    {
        var slot = (item.Slot ?? string.Empty).Split(
            " - ",
            StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        _ = DateTime.TryParseExact(
            item.Date,
            ["yyyy-MM-dd", "dd.MM.yyyy"],
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var date);

        return new Schedule
        {
            Id = item.Id,
            ExternalId = item.Id,
            Date = date,
            Name = item.Disciplines ?? string.Empty,
            Teacher = item.Teacher ?? string.Empty,
            Room = item.Room ?? string.Empty,
            PairType = item.Type ?? string.Empty,
            StartTime = slot.ElementAtOrDefault(0) ?? string.Empty,
            EndTime = slot.ElementAtOrDefault(1) ?? string.Empty
        };
    }

    private async Task<IEnumerable<Schedule>> GetLegacyScheduleAsync(
        string groupId = "",
        string subgroupId = "",
        string[]? teacher = null,
        int roomId = 0,
        CancellationToken cancellationToken = default)
    {
        var mondayDate = GetWeekStart(DateTime.Now);
        var monday = mondayDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var sunday = mondayDate.AddDays(13).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var request = CreateLegacyRequest(monday, sunday, groupId, subgroupId, teacher, roomId);
        var schedule = await _legacyApi.PostAsync<ScheduleDTO>(
            "/get_schedule",
            request,
            cancellationToken);
        return schedule.Schedules;
    }

    private static DateTime GetWeekStart(DateTime date)
    {
        var difference = date.DayOfWeek == DayOfWeek.Sunday
            ? -6
            : DayOfWeek.Monday - date.DayOfWeek;
        return date.Date.AddDays(difference);
    }

    private static Dictionary<string, string> CreateLegacyRequest(
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
            request["teacher_second_name"] = teacher.ElementAtOrDefault(1) ?? string.Empty;
            if (teacher.Length > 2)
                request["teacher_last_name"] = teacher[2];
        }
        return request;
    }

    public void Dispose() => GC.SuppressFinalize(this);

    private sealed class ScheduleApiResponse
    {
        public bool? Success { get; set; }
        public List<ScheduleApiItem> Timetable { get; set; } = [];
    }

    private sealed class ScheduleApiItem
    {
        public int Id { get; set; }
        public string? Date { get; set; }
        public string? Slot { get; set; }
        public string? Type { get; set; }
        public string? Disciplines { get; set; }
        public string? Room { get; set; }
        public string? Teacher { get; set; }
    }
}
