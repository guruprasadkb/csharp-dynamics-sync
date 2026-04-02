namespace CrmSync.Models.Dynamics;

/// <summary>
/// Dynamics 365 statuscode -- reason code within a StateCode.
/// Real Dynamics environments define these per entity; this is a simplified
/// union covering the entities in this challenge.
/// DO NOT MODIFY.
/// </summary>
public enum StatusCode
{
    // -- Contact / Account statuses --
    Active = 1,
    Inactive = 2,

    // -- Opportunity-specific statuses --
    // Note: In real Dynamics, Open maps to value 1 (same int as Active).
    // For this challenge we use distinct values to avoid enum collisions.
    Won = 3,
    Lost = 4,
    Canceled = 5
}
