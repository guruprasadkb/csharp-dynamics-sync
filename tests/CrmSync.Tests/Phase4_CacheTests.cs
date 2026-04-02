using CrmSync.Infrastructure;
using CrmSync.Models.Legacy;
using CrmSync.Models.Sync;
using CrmSync.Tests.Helpers;

namespace CrmSync.Tests;

public class Phase4_CacheTests
{
    [Fact]
    public async Task CacheProvider_SetAndGet_ReturnsValue()
    {
        var cache = TestFixture.CreateCacheProvider();

        var testData = new TestRecord("key-1", "value-1");
        await cache.SetAsync("test:key-1", testData, TimeSpan.FromMinutes(5));

        var result = await cache.GetAsync<TestRecord>("test:key-1");

        Assert.NotNull(result);
        Assert.Equal("key-1", result.Key);
        Assert.Equal("value-1", result.Value);
    }

    [Fact]
    public async Task CacheProvider_Remove_InvalidatesEntry()
    {
        var cache = TestFixture.CreateCacheProvider();

        var testData = new TestRecord("key-2", "value-2");
        await cache.SetAsync("test:key-2", testData, TimeSpan.FromMinutes(5));

        // Invalidate
        await cache.RemoveAsync("test:key-2");

        var result = await cache.GetAsync<TestRecord>("test:key-2");
        Assert.Null(result);
    }

    [Fact]
    public async Task SyncCustomerBatch_ProcessesAllRecords_WithErrorIsolation()
    {
        var repo = new InMemoryCrmRepository();
        var bus = new InMemoryMessageBus();
        var service = TestFixture.CreateSyncService(repo, bus);

        var customers = new List<LegacyCustomer>
        {
            new()
            {
                CustomerId = "BATCH-001",
                FullName = "Batch One",
                Email = "batch1@contoso.com",
                Status = "active",
                LastUpdatedEpoch = 1700000000
            },
            new()
            {
                CustomerId = "BATCH-002",
                FullName = "Batch Two",
                Email = "batch2@contoso.com",
                Status = "active",
                LastUpdatedEpoch = 1700000000
            },
            new()
            {
                CustomerId = "BATCH-003",
                FullName = "Batch Three",
                Email = "batch3@contoso.com",
                Status = "active",
                LastUpdatedEpoch = 1700000000
            }
        };

        var result = await service.SyncCustomerBatchAsync(customers);

        Assert.Equal(3, result.TotalRecords);
        Assert.Equal(3, result.Created);
        Assert.Equal(0, result.Failed);
        Assert.Equal(3, result.Results.Count);
        Assert.All(result.Results, r => Assert.True(r.Success));
    }

    private sealed record TestRecord(string Key, string Value);
}
