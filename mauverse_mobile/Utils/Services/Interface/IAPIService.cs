#pragma warning disable CA1716 // Kept for source compatibility with existing page and ViewModel imports.
namespace mau.Utils.Services.Interface;
#pragma warning restore CA1716

public interface IAPIService
{
    Task<T> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default);
    Task<T> PostAsync<T>(string endpoint, object? data = null, CancellationToken cancellationToken = default);
    void SetHttpHeaders(string username, string token);
    void RemoveHttpHeaders();
}
