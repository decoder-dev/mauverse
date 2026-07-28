namespace mau.Utils.Services.Interface
{
    public readonly record struct CacheStatistics(long SizeBytes, int FileCount);

    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
        Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default);
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
        Task ClearAsync(CancellationToken cancellationToken = default);
        Task<CacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);
    }
}
