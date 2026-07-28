using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using mau.Utils.Services.Interface;

namespace mau.Utils.Services;

public sealed class JsonFileCacheService : ICacheService, IDisposable
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);
    private readonly string _cacheDirectory;

    public JsonFileCacheService()
    {
        _cacheDirectory = Path.Combine(FileSystem.AppDataDirectory, "content-cache");
        Directory.CreateDirectory(_cacheDirectory);
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var path = GetPath(key);
        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (!File.Exists(path))
                return default;

            await using var stream = File.OpenRead(path);
            var envelope = await JsonSerializer.DeserializeAsync<CacheEnvelope<T>>(
                stream,
                _serializerOptions,
                cancellationToken);
            return envelope is null ? default : envelope.Value;
        }
        catch (JsonException)
        {
            TryDelete(path);
            return default;
        }
        catch (IOException)
        {
            return default;
        }
        catch (UnauthorizedAccessException)
        {
            return default;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        var path = GetPath(key);
        var temporaryPath = path + ".tmp";
        var envelope = new CacheEnvelope<T>(DateTimeOffset.UtcNow, value);

        await _gate.WaitAsync(cancellationToken);
        try
        {
            await using (var stream = File.Create(temporaryPath))
            {
                await JsonSerializer.SerializeAsync(
                    stream,
                    envelope,
                    _serializerOptions,
                    cancellationToken);
            }

            // Moving a completed temporary file preserves the previous cache value
            // if Android terminates the process while serialization is in progress.
            File.Move(temporaryPath, path, overwrite: true);
        }
        finally
        {
            TryDelete(temporaryPath);
            _gate.Release();
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        var path = GetPath(key);
        await _gate.WaitAsync(cancellationToken);
        try
        {
            TryDelete(path);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            foreach (var path in Directory.EnumerateFiles(_cacheDirectory, "*.json"))
            {
                cancellationToken.ThrowIfCancellationRequested();
                TryDelete(path);
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<CacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            long sizeBytes = 0;
            var fileCount = 0;
            foreach (var path in Directory.EnumerateFiles(_cacheDirectory, "*.json"))
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    sizeBytes += new FileInfo(path).Length;
                    fileCount++;
                }
                catch (IOException)
                {
                    // A file may disappear while Android is reclaiming cache storage.
                }
                catch (UnauthorizedAccessException)
                {
                }
            }

            return new CacheStatistics(sizeBytes, fileCount);
        }
        finally
        {
            _gate.Release();
        }
    }

    public void Dispose()
    {
        _gate.Dispose();
        GC.SuppressFinalize(this);
    }

    private string GetPath(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Cache key cannot be empty", nameof(key));

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        return Path.Combine(_cacheDirectory, $"{Convert.ToHexString(hash).ToLowerInvariant()}.json");
    }

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private sealed record CacheEnvelope<T>(DateTimeOffset UpdatedAt, T Value);
}
