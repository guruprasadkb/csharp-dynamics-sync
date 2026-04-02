namespace CrmSync.Models.Dynamics;

/// <summary>
/// Dynamics 365 statecode -- governs whether a record is active or deactivated.
/// Applies to Contact, Account, and Opportunity entities.
/// DO NOT MODIFY.
/// </summary>
public enum StateCode
{
    Active = 0,
    Inactive = 1
}
