namespace CrmSync.Infrastructure;

using CrmSync.Interfaces;
using CrmSync.Models.Dynamics;
using System.Collections.Concurrent;

/// <summary>
/// In-memory implementation of ICrmRepository simulating Dynamics 365 Dataverse.
/// Handles VersionNumber auto-increment and timestamp management.
/// DO NOT MODIFY -- this simulates the CRM platform behavior.
/// </summary>
public sealed class InMemoryCrmRepository : ICrmRepository
{
    private readonly ConcurrentDictionary<Guid, Contact> _contacts = new();
    private readonly ConcurrentDictionary<Guid, Account> _accounts = new();
    private readonly ConcurrentDictionary<Guid, Opportunity> _opportunities = new();

    // ===== Contacts =====

    public Task<Contact?> GetContactByIdAsync(Guid id)
    {
        _contacts.TryGetValue(id, out var contact);
        return Task.FromResult(contact);
    }

    public Task<Contact?> GetContactByLegacyIdAsync(string legacyId)
    {
        var contact = _contacts.Values
            .FirstOrDefault(c => c.Crm_LegacyId == legacyId);
        return Task.FromResult(contact);
    }

    public Task<Contact> CreateContactAsync(Contact contact)
    {
        if (contact.Id == Guid.Empty)
            contact.Id = Guid.NewGuid();
        contact.CreatedOn = DateTime.UtcNow;
        contact.ModifiedOn = DateTime.UtcNow;
        contact.VersionNumber = 1;

        if (!_contacts.TryAdd(contact.Id, contact))
            throw new InvalidOperationException($"Contact with ID {contact.Id} already exists.");

        return Task.FromResult(contact);
    }

    public Task<Contact> UpdateContactAsync(Contact contact)
    {
        if (!_contacts.TryGetValue(contact.Id, out var existing))
            throw new KeyNotFoundException($"Contact {contact.Id} not found.");

        // Simulate Dynamics optimistic concurrency
        if (contact.VersionNumber != 0 && contact.VersionNumber != existing.VersionNumber)
            throw new InvalidOperationException(
                $"Concurrency conflict on Contact {contact.Id}. " +
                $"Expected version {contact.VersionNumber}, actual {existing.VersionNumber}.");

        contact.ModifiedOn = DateTime.UtcNow;
        contact.VersionNumber = existing.VersionNumber + 1;
        contact.CreatedOn = existing.CreatedOn; // preserve original
        _contacts[contact.Id] = contact;

        return Task.FromResult(contact);
    }

    // ===== Accounts =====

    public Task<Account?> GetAccountByIdAsync(Guid id)
    {
        _accounts.TryGetValue(id, out var account);
        return Task.FromResult(account);
    }

    public Task<Account?> GetAccountByLegacyIdAsync(string legacyId)
    {
        var account = _accounts.Values
            .FirstOrDefault(a => a.Crm_LegacyId == legacyId);
        return Task.FromResult(account);
    }

    public Task<Account> CreateAccountAsync(Account account)
    {
        if (account.Id == Guid.Empty)
            account.Id = Guid.NewGuid();
        account.CreatedOn = DateTime.UtcNow;
        account.ModifiedOn = DateTime.UtcNow;
        account.VersionNumber = 1;

        if (!_accounts.TryAdd(account.Id, account))
            throw new InvalidOperationException($"Account with ID {account.Id} already exists.");

        return Task.FromResult(account);
    }

    public Task<Account> UpdateAccountAsync(Account account)
    {
        if (!_accounts.TryGetValue(account.Id, out var existing))
            throw new KeyNotFoundException($"Account {account.Id} not found.");

        if (account.VersionNumber != 0 && account.VersionNumber != existing.VersionNumber)
            throw new InvalidOperationException(
                $"Concurrency conflict on Account {account.Id}.");

        account.ModifiedOn = DateTime.UtcNow;
        account.VersionNumber = existing.VersionNumber + 1;
        account.CreatedOn = existing.CreatedOn;
        _accounts[account.Id] = account;

        return Task.FromResult(account);
    }

    // ===== Opportunities =====

    public Task<Opportunity?> GetOpportunityByIdAsync(Guid id)
    {
        _opportunities.TryGetValue(id, out var opp);
        return Task.FromResult(opp);
    }

    public Task<Opportunity?> GetOpportunityByLegacyIdAsync(string legacyId)
    {
        var opp = _opportunities.Values
            .FirstOrDefault(o => o.Crm_LegacyId == legacyId);
        return Task.FromResult(opp);
    }

    public Task<Opportunity> CreateOpportunityAsync(Opportunity opportunity)
    {
        if (opportunity.Id == Guid.Empty)
            opportunity.Id = Guid.NewGuid();
        opportunity.CreatedOn = DateTime.UtcNow;
        opportunity.ModifiedOn = DateTime.UtcNow;
        opportunity.VersionNumber = 1;

        if (!_opportunities.TryAdd(opportunity.Id, opportunity))
            throw new InvalidOperationException($"Opportunity with ID {opportunity.Id} already exists.");

        return Task.FromResult(opportunity);
    }

    public Task<Opportunity> UpdateOpportunityAsync(Opportunity opportunity)
    {
        if (!_opportunities.TryGetValue(opportunity.Id, out var existing))
            throw new KeyNotFoundException($"Opportunity {opportunity.Id} not found.");

        if (opportunity.VersionNumber != 0 && opportunity.VersionNumber != existing.VersionNumber)
            throw new InvalidOperationException(
                $"Concurrency conflict on Opportunity {opportunity.Id}.");

        opportunity.ModifiedOn = DateTime.UtcNow;
        opportunity.VersionNumber = existing.VersionNumber + 1;
        opportunity.CreatedOn = existing.CreatedOn;
        _opportunities[opportunity.Id] = opportunity;

        return Task.FromResult(opportunity);
    }

    // ===== Query =====

    public Task<IReadOnlyList<Contact>> GetContactsByAccountIdAsync(Guid accountId)
    {
        var contacts = _contacts.Values
            .Where(c => c.ParentCustomerId == accountId)
            .ToList();
        return Task.FromResult<IReadOnlyList<Contact>>(contacts);
    }

    public Task<IReadOnlyList<Opportunity>> GetOpportunitiesByAccountIdAsync(Guid accountId)
    {
        var opps = _opportunities.Values
            .Where(o => o.ParentAccountId == accountId)
            .ToList();
        return Task.FromResult<IReadOnlyList<Opportunity>>(opps);
    }

    // ===== Test helpers (not on the interface -- access via cast in tests) =====

    public void SeedContact(Contact contact) => _contacts[contact.Id] = contact;
    public void SeedAccount(Account account) => _accounts[account.Id] = account;
    public void SeedOpportunity(Opportunity opportunity) => _opportunities[opportunity.Id] = opportunity;
}
