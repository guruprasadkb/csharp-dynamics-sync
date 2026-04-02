using CrmSync.Infrastructure;
using CrmSync.Models.Sync;
using CrmSync.Models.Legacy;
using CrmSync.Tests.Helpers;

namespace CrmSync.Tests;

public class Phase1_SyncTests
{
    [Fact]
    public async Task SyncCustomer_NewRecord_CreatesContact()
    {
        var repo = new InMemoryCrmRepository();
        var bus = new InMemoryMessageBus();
        var service = TestFixture.CreateSyncService(repo, bus);

        var customer = new LegacyCustomer
        {
            CustomerId = "CUST-100",
            FullName = "Alice Johnson",
            Email = "alice@contoso.com",
            Phone = "+1-555-0100",
            Status = "active",
            LastUpdatedEpoch = 1700000000
        };

        var result = await service.SyncCustomerAsync(customer);

        Assert.True(result.Success);
        Assert.Equal(SyncOperation.Created, result.Operation);
        Assert.Equal("CUST-100", result.LegacyId);
        Assert.NotNull(result.EntityId);

        // Verify persisted
        var contact = await repo.GetContactByLegacyIdAsync("CUST-100");
        Assert.NotNull(contact);
        Assert.Equal("Alice", contact.FirstName);
        Assert.Equal("Johnson", contact.LastName);
        Assert.Equal("alice@contoso.com", contact.EmailAddress1);
    }

    [Fact]
    public async Task SyncCustomer_ExistingRecord_UpdatesContact()
    {
        var repo = new InMemoryCrmRepository();
        var bus = new InMemoryMessageBus();
        var service = TestFixture.CreateSyncService(repo, bus);

        // First sync -- creates
        var customer = new LegacyCustomer
        {
            CustomerId = "CUST-200",
            FullName = "Bob Wilson",
            Email = "bob@contoso.com",
            Status = "active",
            LastUpdatedEpoch = 1700000000
        };
        await service.SyncCustomerAsync(customer);

        // Second sync -- updates with newer timestamp
        var updated = new LegacyCustomer
        {
            CustomerId = "CUST-200",
            FullName = "Robert Wilson",
            Email = "robert@contoso.com",
            Status = "active",
            LastUpdatedEpoch = 1700100000
        };
        var result = await service.SyncCustomerAsync(updated);

        Assert.True(result.Success);
        Assert.Equal(SyncOperation.Updated, result.Operation);

        var contact = await repo.GetContactByLegacyIdAsync("CUST-200");
        Assert.NotNull(contact);
        Assert.Equal("Robert", contact.FirstName);
        Assert.Equal("robert@contoso.com", contact.EmailAddress1);
    }
}
