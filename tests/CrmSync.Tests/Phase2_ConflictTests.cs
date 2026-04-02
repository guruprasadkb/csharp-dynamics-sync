using CrmSync.Infrastructure;
using CrmSync.Models.Dynamics;
using CrmSync.Models.Legacy;
using CrmSync.Models.Sync;
using CrmSync.Tests.Helpers;

namespace CrmSync.Tests;

public class Phase2_ConflictTests
{
    [Fact]
    public async Task SyncCustomer_CrmModifiedAfterLegacy_WithTargetWins_Skips()
    {
        var repo = new InMemoryCrmRepository();
        var bus = new InMemoryMessageBus();
        var service = TestFixture.CreateSyncService(repo, bus, ConflictStrategy.TargetWins);

        // Seed a contact that was modified in CRM very recently
        var existingContact = new Contact
        {
            Id = Guid.NewGuid(),
            FirstName = "CRM",
            LastName = "Version",
            EmailAddress1 = "crm@contoso.com",
            Crm_LegacyId = "CUST-300",
            Crm_LastSyncedOn = DateTimeOffset.FromUnixTimeSeconds(1700000000).UtcDateTime,
            ModifiedOn = DateTime.UtcNow,  // CRM was modified very recently
            VersionNumber = 5,
            StateCode = StateCode.Active,
            StatusCode = StatusCode.Active
        };
        repo.SeedContact(existingContact);

        // Legacy record has an older timestamp
        var customer = new LegacyCustomer
        {
            CustomerId = "CUST-300",
            FullName = "Legacy Version",
            Email = "legacy@contoso.com",
            Status = "active",
            LastUpdatedEpoch = 1700000000  // much older than CRM's ModifiedOn
        };

        var result = await service.SyncCustomerAsync(customer);

        Assert.Equal(SyncOperation.Skipped, result.Operation);

        // Verify CRM data was not changed
        var contact = await repo.GetContactByLegacyIdAsync("CUST-300");
        Assert.Equal("CRM", contact!.FirstName);
    }

    [Fact]
    public async Task SyncCustomer_CrmModifiedAfterLegacy_WithSourceWins_Updates()
    {
        var repo = new InMemoryCrmRepository();
        var bus = new InMemoryMessageBus();
        var service = TestFixture.CreateSyncService(repo, bus, ConflictStrategy.SourceWins);

        var existingContact = new Contact
        {
            Id = Guid.NewGuid(),
            FirstName = "CRM",
            LastName = "Version",
            EmailAddress1 = "crm@contoso.com",
            Crm_LegacyId = "CUST-301",
            Crm_LastSyncedOn = DateTimeOffset.FromUnixTimeSeconds(1699000000).UtcDateTime,
            ModifiedOn = DateTime.UtcNow,
            VersionNumber = 3,
            StateCode = StateCode.Active,
            StatusCode = StatusCode.Active
        };
        repo.SeedContact(existingContact);

        var customer = new LegacyCustomer
        {
            CustomerId = "CUST-301",
            FullName = "Legacy Wins",
            Email = "legacy.wins@contoso.com",
            Status = "active",
            LastUpdatedEpoch = 1700000000
        };

        var result = await service.SyncCustomerAsync(customer);

        Assert.True(result.Success);
        Assert.Equal(SyncOperation.Updated, result.Operation);

        var contact = await repo.GetContactByLegacyIdAsync("CUST-301");
        Assert.Equal("Legacy", contact!.FirstName);
        Assert.Equal("Wins", contact.LastName);
    }

    [Fact]
    public async Task SyncCustomer_LastWriterWins_LegacyNewer_Updates()
    {
        var repo = new InMemoryCrmRepository();
        var bus = new InMemoryMessageBus();
        var service = TestFixture.CreateSyncService(repo, bus, ConflictStrategy.LastWriterWins);

        // CRM record was modified at a known time in the past
        var crmModifiedTime = new DateTime(2023, 11, 10, 0, 0, 0, DateTimeKind.Utc);
        var existingContact = new Contact
        {
            Id = Guid.NewGuid(),
            FirstName = "Old",
            LastName = "CRM",
            EmailAddress1 = "old@contoso.com",
            Crm_LegacyId = "CUST-302",
            Crm_LastSyncedOn = new DateTime(2023, 11, 1, 0, 0, 0, DateTimeKind.Utc),
            ModifiedOn = crmModifiedTime,
            VersionNumber = 2,
            StateCode = StateCode.Active,
            StatusCode = StatusCode.Active
        };
        repo.SeedContact(existingContact);

        // Legacy record is newer (Nov 14 > Nov 10)
        var customer = new LegacyCustomer
        {
            CustomerId = "CUST-302",
            FullName = "Newer Legacy",
            Email = "newer@contoso.com",
            Status = "active",
            LastUpdatedEpoch = 1700000000  // 2023-11-14T22:13:20Z
        };

        var result = await service.SyncCustomerAsync(customer);

        Assert.True(result.Success);
        Assert.Equal(SyncOperation.Updated, result.Operation);

        var contact = await repo.GetContactByLegacyIdAsync("CUST-302");
        Assert.Equal("Newer", contact!.FirstName);
    }
}
