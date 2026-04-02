namespace CrmSync.Models.Dynamics;

/// <summary>
/// Base class for all Dynamics 365 CRM entities.
/// Mirrors the system columns present on every Dataverse table.
/// DO NOT MODIFY this file -- it represents the target CRM schema.
/// </summary>
public abstract class EntityBase
{
    /// <summary>Primary key -- maps to the entity's logical primary ID column.</summary>
    public Guid Id { get; set; }

    /// <summary>Row creation timestamp (UTC). Set by the platform on create.</summary>
    public DateTime CreatedOn { get; set; }

    /// <summary>Last modification timestamp (UTC). Updated by the platform on every write.</summary>
    public DateTime ModifiedOn { get; set; }

    /// <summary>Record lifecycle state: 0 = Active, 1 = Inactive.</summary>
    public StateCode StateCode { get; set; } = StateCode.Active;

    /// <summary>Reason sub-status within the current StateCode.</summary>
    public StatusCode StatusCode { get; set; } = StatusCode.Active;

    /// <summary>Systemuser ID of the record owner.</summary>
    public Guid OwnerId { get; set; }

    /// <summary>
    /// Row version for optimistic concurrency. Dynamics 365 uses this
    /// to detect conflicting updates. Incremented on every write.
    /// </summary>
    public long VersionNumber { get; set; }
}
