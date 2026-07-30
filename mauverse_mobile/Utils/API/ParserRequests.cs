using mau.DTOModels;
using mau.Models;
using mau.Utils.API.Interaface;
using mau.Utils.Services.Interface;

using Microsoft.Extensions.Caching.Memory;

using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;

namespace mau.Utils.API
{
    public sealed class ParserRequests : IParserRequests, IDisposable
    {
        private static readonly TimeSpan MemoryCacheDuration = TimeSpan.FromMinutes(10);

        private readonly IAPIService _apiService;
        private readonly IMemoryCache _memoryCache;
        private readonly ICacheService _persistentCache;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _requestLocks = new();

        public ParserRequests(
            IAPIService apiService,
            IMemoryCache memoryCache,
            ICacheService persistentCache)
        {
            _apiService = apiService;
            _memoryCache = memoryCache;
            _persistentCache = persistentCache;
        }

        public async Task<IEnumerable<DeptInfoDTO>> GetDeptsAsync(CancellationToken cancellationToken = default) =>
            await GetListAsync(
                "departments",
                token => _apiService.GetAsync<List<DeptInfoDTO>>("/get_depts_json", token),
                cancellationToken);

        public async Task<IEnumerable<RssDTO>> GetNewsAsync(
            RssData type,
            bool forceRefresh = false,
            CancellationToken cancellationToken = default) =>
            await GetListAsync(
                $"news-{(int)type}",
                token => _apiService.GetAsync<List<RssDTO>>($"/news?news_type={(int)type}", token),
                cancellationToken,
                forceRefresh);

        public async Task<IEnumerable<Room>> GetRoomsAsync(string room, CancellationToken cancellationToken = default)
        {
            var request = new { room_name = room };
            var result = await _apiService.PostAsync<RoomDTO>("/get_rooms", request, cancellationToken);
            return result?.Rooms ?? [];
        }

        public async Task<IEnumerable<Room>> GetRoomsAsync(bool isAll, CancellationToken cancellationToken = default)
        {
            if (!isAll)
                return await GetRoomsAsync(string.Empty, cancellationToken);

            return await GetListAsync("rooms-all", async token =>
            {
                var result = await _apiService.PostAsync<RoomDTO>("/get_all_rooms", new { room_name = string.Empty }, token);
                return result?.Rooms?.ToList() ?? [];
            }, cancellationToken);
        }

        public async Task<UniversityInfo> GetTeacherInfoAsync(string teacher, CancellationToken cancellationToken = default)
        {
            var teachers = teacher.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (teachers.Length < 3)
                return new UniversityInfo { Name = teacher };

            var request = new
            {
                first_name = teachers[0],
                second_name = teachers[1][0],
                last_name = teachers[2][0]
            };
            return await _apiService.PostAsync<UniversityInfo>("/get_teacher_info", request, cancellationToken);
        }

        public async Task<IEnumerable<string>> GetTeachersAsync(string teacher, CancellationToken cancellationToken = default)
        {
            var request = new { teacher_name = teacher };
            var result = await _apiService.PostAsync<TeacherDTO>("/get_teachers", request, cancellationToken);
            return result?.Teachers ?? [];
        }

        public async Task<IEnumerable<string>> GetTeachersAsync(bool isAll, CancellationToken cancellationToken = default)
        {
            if (!isAll)
                return await GetTeachersAsync(string.Empty, cancellationToken);

            return await GetListAsync("teachers-all", async token =>
            {
                var result = await _apiService.PostAsync<TeacherDTO>(
                    "/get_all_teachers",
                    new { teacher_name = string.Empty },
                    token);
                return result?.Teachers?.ToList() ?? [];
            }, cancellationToken);
        }

        public async Task<IEnumerable<TelephoneInfoDTO>> GetTelephonesAsync(DeptInfoDTO department, CancellationToken cancellationToken = default) =>
            await GetListAsync(
                $"department-contacts-{department.Id}",
                token => _apiService.PostAsync<List<TelephoneInfoDTO>>(
                    "/get_contacts_json",
                    new { department_id = department.Id, name = department.Name },
                    token),
                cancellationToken);

        private async Task<List<T>> GetListAsync<T>(
            string key,
            Func<CancellationToken, Task<List<T>>> fetch,
            CancellationToken cancellationToken,
            bool forceRefresh = false)
        {
            if (!forceRefresh &&
                _memoryCache.TryGetValue(key, out List<T>? memoryValue) &&
                memoryValue is not null)
                return memoryValue;

            var requestLock = _requestLocks.GetOrAdd(key, static _ => new SemaphoreSlim(1, 1));
            await requestLock.WaitAsync(cancellationToken);
            try
            {
                if (!forceRefresh &&
                    _memoryCache.TryGetValue(key, out memoryValue) &&
                    memoryValue is not null)
                    return memoryValue;

                Exception? requestError = null;
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    try
                    {
                        var freshValue = await fetch(cancellationToken) ?? [];
                        _memoryCache.Set(key, freshValue, MemoryCacheDuration);
                        await _persistentCache.SetAsync(key, freshValue, cancellationToken);
                        return freshValue;
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        requestError = ex;
                    }
                }

                var persistedValue = await _persistentCache.GetAsync<List<T>>(key, cancellationToken);
                if (persistedValue is not null)
                {
                    _memoryCache.Set(key, persistedValue, MemoryCacheDuration);
                    return persistedValue;
                }

                if (requestError is not null)
                    ExceptionDispatchInfo.Capture(requestError).Throw();

                throw new HttpRequestException("Нет подключения и сохраненных данных");
            }
            finally
            {
                requestLock.Release();
            }
        }

        public void Dispose()
        {
            foreach (var requestLock in _requestLocks.Values)
                requestLock.Dispose();
            _requestLocks.Clear();
            GC.SuppressFinalize(this);
        }
    }
}
