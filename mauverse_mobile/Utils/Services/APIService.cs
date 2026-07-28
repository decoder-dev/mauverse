using System.Net.Http.Json;
using System.Text.Json;
using mau.DTOModels;
using mau.Utils.Services.Interface;

namespace mau.Utils.Services;

public sealed class APIService : IAPIService
{
    private static readonly HttpClient Transport = new()
    {
        Timeout = TimeSpan.FromSeconds(20)
    };

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    private readonly object _authLock = new();
    private string _username = string.Empty;
    private string _token = string.Empty;

    public Task<T> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default) =>
        SendAsync<T>(HttpMethod.Get, endpoint, data: null, cancellationToken);

    public Task<T> PostAsync<T>(
        string endpoint,
        object? data = null,
        CancellationToken cancellationToken = default) =>
        SendAsync<T>(HttpMethod.Post, endpoint, data, cancellationToken);

    public void SetHttpHeaders(string username, string token)
    {
        lock (_authLock)
        {
            _username = username?.Trim() ?? string.Empty;
            _token = token?.Trim() ?? string.Empty;
        }
    }

    public void RemoveHttpHeaders()
    {
        lock (_authLock)
        {
            _username = string.Empty;
            _token = string.Empty;
        }
    }

    private async Task<T> SendAsync<T>(
        HttpMethod method,
        string endpoint,
        object? data,
        CancellationToken cancellationToken)
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            throw new HttpRequestException("Нет интернет-соединения");

        try
        {
            using var request = CreateRequest(method, endpoint);
            if (method == HttpMethod.Post)
            {
                request.Content = data is null
                    ? new StringContent(string.Empty)
                    : JsonContent.Create(data, options: SerializerOptions);
            }

            using var response = await Transport.SendAsync(
                request,
                HttpCompletionOption.ResponseContentRead,
                cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);

            var result = await response.Content.ReadFromJsonAsync<T>(
                SerializerOptions,
                cancellationToken);
            if (result is null)
                throw new InvalidDataException("Сервер вернул пустой ответ");
            if (result is BaseDTO error && !string.IsNullOrWhiteSpace(error.Error))
                throw new InvalidOperationException($"{error.Error}: {error.Detail ?? "нет деталей"}");

            return result;
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("Сервер не ответил вовремя", exception);
        }
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            throw new ArgumentException("API endpoint must be a relative path", nameof(endpoint));

        var trimmedEndpoint = endpoint.Trim();
        if (trimmedEndpoint.StartsWith("//", StringComparison.Ordinal) ||
            trimmedEndpoint.Contains('\\'))
        {
            throw new ArgumentException("API endpoint must be a relative path", nameof(endpoint));
        }

        var relativeEndpoint = trimmedEndpoint.TrimStart('/');
        if (relativeEndpoint.Length == 0 || Uri.TryCreate(relativeEndpoint, UriKind.Absolute, out _))
            throw new ArgumentException("API endpoint must be a relative path", nameof(endpoint));

        var requestUri = new Uri(ApiConfiguration.BaseUri, relativeEndpoint);
        if (!string.Equals(requestUri.Scheme, ApiConfiguration.BaseUri.Scheme, StringComparison.Ordinal) ||
            !string.Equals(requestUri.Host, ApiConfiguration.BaseUri.Host, StringComparison.OrdinalIgnoreCase) ||
            requestUri.Port != ApiConfiguration.BaseUri.Port ||
            !requestUri.AbsolutePath.StartsWith(ApiConfiguration.BaseUri.AbsolutePath, StringComparison.Ordinal))
        {
            throw new ArgumentException("API endpoint escapes the configured base path", nameof(endpoint));
        }

        var request = new HttpRequestMessage(method, requestUri);
        string username;
        string token;
        lock (_authLock)
        {
            username = _username;
            token = _token;
        }

        if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Add("X-Auth-Token", token);
            request.Headers.Add("X-Auth-Username", username);
        }

        return request;
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var message = TryReadApiError(content) ?? $"Сервер вернул ошибку {(int)response.StatusCode}";
        throw new HttpRequestException(message, null, response.StatusCode);
    }

    private static string? TryReadApiError(string content)
    {
        if (string.IsNullOrWhiteSpace(content) || content.Length > 16_384)
            return null;

        try
        {
            var error = JsonSerializer.Deserialize<BaseDTO>(content, SerializerOptions);
            return !string.IsNullOrWhiteSpace(error?.Detail)
                ? error.Detail
                : error?.Error;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
