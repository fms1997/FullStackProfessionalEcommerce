using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace FulSpectrum.Api.Services;

public interface ICatalogCacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct);
    Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct);
    Task<long> GetCatalogVersionAsync(CancellationToken ct);
    Task InvalidateCatalogAsync(CancellationToken ct);
    string BuildEtag(string payload);
}

public sealed class CatalogCacheService : ICatalogCacheService
{
    private const string CatalogVersionKey = "catalog:version";
    private readonly IDistributedCache _cache;
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public CatalogCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct)
    {
        var json = await _cache.GetStringAsync(key, ct);
        return json is null ? default : JsonSerializer.Deserialize<T>(json, SerializerOptions);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(value, SerializerOptions);
        return _cache.SetStringAsync(key, json, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        }, ct);
    }

    public async Task<long> GetCatalogVersionAsync(CancellationToken ct)
    {
        var value = await _cache.GetStringAsync(CatalogVersionKey, ct);
        return long.TryParse(value, out var parsed) ? parsed : 1;
    }

    public async Task InvalidateCatalogAsync(CancellationToken ct)
    {
        var currentVersion = await GetCatalogVersionAsync(ct);
        await _cache.SetStringAsync(CatalogVersionKey, (currentVersion + 1).ToString(), ct);
    }

    public string BuildEtag(string payload)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return $"\"{Convert.ToHexString(bytes)}\"";
    }

}
