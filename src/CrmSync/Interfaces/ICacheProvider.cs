namespace CrmSync.Interfaces;

/// <summary>
/// Simulates a Redis cache for frequently accessed entities.
/// Candidate implements this interface.
/// </summary>
public interface ICacheProvider
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class;
    Task RemoveAsync(string key);
    Task<bool> ExistsAsync(string key);
}
