using CrmSync.Infrastructure;
using CrmSync.Models.Dynamics;
using CrmSync.Models.Legacy;
using CrmSync.Models.Sync;
using CrmSync.Tests.Helpers;

namespace CrmSync.Tests;

public class Phase2_IdempotencyTests
{
    [Fact]
    public async Task SyncCustomer_SameTimestampTwice_SkipsSecondSync()
    {
        var repo = new InMemoryCrmRepository();
        var bus = new InMemoryMessageBus();
        var service = TestFixture.CreateSyncService(repo, bus);

        var customer = new LegacyCustomer
        {
            CustomerId = "CUST-400",
            FullName = "Idempotent User",
            Email = "idem@contoso.com",
            Status = "active",
            LastUpdatedEpoch = 1700000000
        };

        // First sync -- creates
        var result1 = await service.SyncCustomerAsync(customer);
        Assert.Equal(SyncOperation.Created, result1.Operation);

        // Second sync -- same record, same timestamp -- should skip
        var result2 = await service.SyncCustomerAsync(customer);
        Assert.Equal(SyncOperation.Skipped, result2.Operation);
    }

    [Fact]
    public async Task SyncCustomer_UpdatePassesVersionNumber_NoConflict()
    {
        var repo = new InMemoryCrmRepository();
        var bus = new InMemoryMessageBus();
        var service = TestFixture.CreateSyncService(repo, bus);

        // Create
        var customer = new LegacyCustomer
        {
            CustomerId = "CUST-401",
            FullName = "Version Test",
            Email = "version@contoso.com",
            Status = "active",
            LastUpdatedEpoch = 1700000000
        };
        await service.SyncCustomerAsync(customer);

        // Update with newer timestamp
        var updated = new LegacyCustomer
        {
            CustomerId = "CUST-401",
            FullName = "Version Updated",
            Email = "version2@contoso.com",
            Status = "active",
            LastUpdatedEpoch = 1700100000
        };
        var result = await service.SyncCustomerAsync(updated);

        // Should succeed without concurrency exception
        Assert.True(result.Success);
        Assert.Equal(SyncOperation.Updated, result.Operation);

        // Verify version incremented
        var contact = await repo.GetContactByLegacyIdAsync("CUST-401");
        Assert.NotNull(contact);
        Assert.True(contact.VersionNumber >= 2);
    }
}
