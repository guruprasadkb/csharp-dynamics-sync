namespace CrmSync.Interfaces;

using CrmSync.Models.Legacy;
using CrmSync.Models.Sync;

/// <summary>
/// Orchestrates synchronization of legacy records into Dynamics 365.
/// Candidate must implement this interface.
/// </summary>
public interface ISyncService
{
    /// <summary>
    /// Upsert a single customer. If a Contact with matching Crm_LegacyId exists,
    /// update it; otherwise create a new Contact.
    /// </summary>
    Task<SyncResult> SyncCustomerAsync(LegacyCustomer customer);

    Task<SyncResult> SyncCompanyAsync(LegacyCompany company);

    Task<SyncResult> SyncDealAsync(LegacyDeal deal);

    /// <summary>
    /// Process a batch of customers with error isolation -- a failure in one
    /// record must not prevent others from being processed.
    /// </summary>
    Task<BatchResult> SyncCustomerBatchAsync(IReadOnlyList<LegacyCustomer> customers);

    Task<BatchResult> SyncCompanyBatchAsync(IReadOnlyList<LegacyCompany> companies);

    Task<BatchResult> SyncDealBatchAsync(IReadOnlyList<LegacyDeal> deals);
}
