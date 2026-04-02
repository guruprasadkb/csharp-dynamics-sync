using CrmSync.Infrastructure;
using CrmSync.Models.Legacy;
using CrmSync.Models.Sync;
using CrmSync.Tests.Helpers;

namespace CrmSync.Tests;

public class Phase3_EventTests
{
    [Fact]
    public async Task SyncCustomer_Create_PublishesContactSyncedEvent()
    {
        var repo = new InMemoryCrmRepository();
        var bus = new InMemoryMessageBus();
        var service = TestFixture.CreateSyncService(repo, bus);

        var customer = new LegacyCustomer
        {
            CustomerId = "CUST-500",
            FullName = "Event Test",
            Email = "event@contoso.com",
            Status = "active",
            LastUpdatedEpoch = 1700000000
        };

        await service.SyncCustomerAsync(customer);

        // Verify an event was published to "contact.synced"
        Assert.Equal(1, bus.GetMessageCount("contact.synced"));
    }

    [Fact]
    public async Task SyncCustomer_Update_PublishesContactSyncedEvent()
    {
        var repo = new InMemoryCrmRepository();
        var bus = new InMemoryMessageBus();
        var service = TestFixture.CreateSyncService(repo, bus);

        var customer = new LegacyCustomer
        {
            CustomerId = "CUST-501",
            FullName = "Event Update",
            Email = "event.update@contoso.com",
            Status = "active",
            LastUpdatedEpoch = 1700000000
        };
        await service.SyncCustomerAsync(customer);

        // Update with newer timestamp
        var updated = new LegacyCustomer
        {
            CustomerId = "CUST-501",
            FullName = "Event Updated",
            Email = "event.updated@contoso.com",
            Status = "active",
            LastUpdatedEpoch = 1700100000
        };
        await service.SyncCustomerAsync(updated);

        // Should have 2 events: one for create, one for update
        Assert.Equal(2, bus.GetMessageCount("contact.synced"));
    }

    [Fact]
    public async Task SyncCompany_PublishesAccountSyncedEvent()
    {
        var repo = new InMemoryCrmRepository();
        var bus = new InMemoryMessageBus();
        var service = TestFixture.CreateSyncService(repo, bus);

        var company = new LegacyCompany
        {
            CompanyId = "COMP-500",
            CompanyName = "Contoso Ltd",
            MainPhone = "+1-555-0200",
            Status = "active",
            LastUpdatedEpoch = 1700000000
        };

        await service.SyncCompanyAsync(company);

        Assert.Equal(1, bus.GetMessageCount("account.synced"));
    }

    [Fact]
    public async Task SyncCompany_AutoLinksContact_WhenPrimaryContactRefExists()
    {
        var repo = new InMemoryCrmRepository();
        var bus = new InMemoryMessageBus();
        var service = TestFixture.CreateSyncService(repo, bus);

        // First: sync the customer who will be the primary contact
        var customer = new LegacyCustomer
        {
            CustomerId = "CUST-600",
            FullName = "Primary Contact",
            Email = "primary@contoso.com",
            Status = "active",
            LastUpdatedEpoch = 1700000000
        };
        await service.SyncCustomerAsync(customer);

        // Then: sync the company that references this customer
        var company = new LegacyCompany
        {
            CompanyId = "COMP-600",
            CompanyName = "Linked Corp",
            PrimaryContactRef = "CUST-600",
            Status = "active",
            LastUpdatedEpoch = 1700000000
        };
        await service.SyncCompanyAsync(company);

        // Process events to trigger the auto-link handler
        await bus.ProcessPendingAsync();

        // Verify the account's PrimaryContactId is set
        var account = await repo.GetAccountByLegacyIdAsync("COMP-600");
        Assert.NotNull(account);
        Assert.NotNull(account.PrimaryContactId);

        // Verify it points to the right contact
        var contact = await repo.GetContactByLegacyIdAsync("CUST-600");
        Assert.Equal(contact!.Id, account.PrimaryContactId);
    }
}
