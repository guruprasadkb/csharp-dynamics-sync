namespace CrmSync.Interfaces;

using CrmSync.Models.Dynamics;

/// <summary>
/// Repository abstraction for Dynamics 365 CRM entity operations.
/// An in-memory implementation is provided -- do not modify it.
/// </summary>
public interface ICrmRepository
{
    // -- Contact operations --
    Task<Contact?> GetContactByIdAsync(Guid id);
    Task<Contact?> GetContactByLegacyIdAsync(string legacyId);
    Task<Contact> CreateContactAsync(Contact contact);
    Task<Contact> UpdateContactAsync(Contact contact);

    // -- Account operations --
    Task<Account?> GetAccountByIdAsync(Guid id);
    Task<Account?> GetAccountByLegacyIdAsync(string legacyId);
    Task<Account> CreateAccountAsync(Account account);
    Task<Account> UpdateAccountAsync(Account account);

    // -- Opportunity operations --
    Task<Opportunity?> GetOpportunityByIdAsync(Guid id);
    Task<Opportunity?> GetOpportunityByLegacyIdAsync(string legacyId);
    Task<Opportunity> CreateOpportunityAsync(Opportunity opportunity);
    Task<Opportunity> UpdateOpportunityAsync(Opportunity opportunity);

    // -- Query --
    Task<IReadOnlyList<Contact>> GetContactsByAccountIdAsync(Guid accountId);
    Task<IReadOnlyList<Opportunity>> GetOpportunitiesByAccountIdAsync(Guid accountId);
}
