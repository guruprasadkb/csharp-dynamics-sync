namespace CrmSync.Services;

using CrmSync.Interfaces;
using System.Collections.Concurrent;

public sealed class InMemoryCacheProvider : ICacheProvider
{
    private readonly ConcurrentDictionary<string, CacheEntry> _store = new();

    public Task<T?> GetAsync<T>(string key) where T : class
    {
        if (_store.TryGetValue(key, out var entry))
        {
            if (entry.ExpiresAt.HasValue && entry.ExpiresAt.Value < DateTime.UtcNow)
            {
                _store.TryRemove(key, out _);
                return Task.FromResult<T?>(null);
            }
            return Task.FromResult(entry.Value as T);
        }
        return Task.FromResult<T?>(null);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
    {
        var expiresAt = expiry.HasValue ? DateTime.UtcNow + expiry.Value : (DateTime?)null;
        _store[key] = new CacheEntry(value, expiresAt);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _store.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key)
    {
        if (_store.TryGetValue(key, out var entry))
        {
            if (entry.ExpiresAt.HasValue && entry.ExpiresAt.Value < DateTime.UtcNow)
            {
                _store.TryRemove(key, out _);
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    private sealed record CacheEntry(object Value, DateTime? ExpiresAt);
}
