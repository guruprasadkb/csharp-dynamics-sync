namespace CrmSync.Models.Dynamics;

/// <summary>
/// Dynamics 365 Opportunity entity.
/// DO NOT MODIFY -- this is the target CRM schema.
/// </summary>
public sealed class Opportunity : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>Estimated revenue for this opportunity.</summary>
    public decimal? EstimatedValue { get; set; }

    /// <summary>Actual revenue -- set when the opportunity is closed as Won.</summary>
    public decimal? ActualValue { get; set; }

    /// <summary>Expected close date.</summary>
    public DateTime? EstimatedCloseDate { get; set; }

    /// <summary>Actual close date -- set on Win/Lose.</summary>
    public DateTime? ActualCloseDate { get; set; }

    /// <summary>Sales stage: 0=Qualify, 1=Develop, 2=Propose, 3=Close.</summary>
    public int SalesStage { get; set; }

    /// <summary>Win probability percentage (0-100).</summary>
    public int? CloseProbability { get; set; }

    // -- Relationships --
    /// <summary>Lookup to the parent Account.</summary>
    public Guid? ParentAccountId { get; set; }

    /// <summary>Lookup to the primary Contact associated with this deal.</summary>
    public Guid? ParentContactId { get; set; }

    // -- Sync metadata --
    public string? Crm_LegacyId { get; set; }
    public DateTime? Crm_LastSyncedOn { get; set; }
}
