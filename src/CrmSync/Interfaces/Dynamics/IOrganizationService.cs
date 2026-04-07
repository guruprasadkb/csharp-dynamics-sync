namespace CrmSync.Interfaces.Dynamics;

using CrmSync.Models.Dynamics;

/// <summary>
/// Simplified mock of Microsoft.Xrm.Sdk.IOrganizationService.
/// In a real Dynamics environment, this would handle generic Entities.
/// For this assessment, we use our specific Contact, Account, and Opportunity models.
/// </summary>
public interface IOrganizationService
{
    Task<Guid> CreateAsync(object entity);
    Task UpdateAsync(object entity);
    Task DeleteAsync(string entityName, Guid id);
    Task<T?> RetrieveAsync<T>(string entityName, Guid id) where T : class;
}
