namespace CrmSync.Interfaces;

using CrmSync.Models.Dynamics;
using CrmSync.Models.Legacy;

/// <summary>
/// Maps entities between legacy LegacyPro CRM format and Dynamics 365 format.
/// Candidate must implement this interface.
/// </summary>
public interface IEntityMapper
{
    /// <summary>
    /// Map a LegacyCustomer to a Dynamics 365 Contact.
    /// Must handle: name splitting, epoch-to-DateTime, status mapping,
    /// and preserving the legacy ID in Crm_LegacyId.
    /// </summary>
    Contact MapToContact(LegacyCustomer legacy);

    /// <summary>
    /// Map a LegacyCompany to a Dynamics 365 Account.
    /// </summary>
    Account MapToAccount(LegacyCompany legacy);

    /// <summary>
    /// Map a LegacyDeal to a Dynamics 365 Opportunity.
    /// Must handle: stage string to int mapping, status to StateCode/StatusCode,
    /// date string parsing, and relationship resolution.
    /// </summary>
    Opportunity MapToOpportunity(LegacyDeal legacy);
}
