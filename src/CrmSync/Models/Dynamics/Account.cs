namespace CrmSync.Models.Dynamics;

/// <summary>
/// Dynamics 365 Account entity.
/// DO NOT MODIFY -- this is the target CRM schema.
/// </summary>
public sealed class Account : EntityBase
{
    public string Name { get; set; } = string.Empty;

    public string? Telephone1 { get; set; }
    public string? Fax { get; set; }
    public string? WebSiteUrl { get; set; }
    public string? EmailAddress1 { get; set; }

    // -- Address --
    public string? Address1_Line1 { get; set; }
    public string? Address1_City { get; set; }
    public string? Address1_StateOrProvince { get; set; }
    public string? Address1_PostalCode { get; set; }
    public string? Address1_Country { get; set; }

    // -- Classification --
    public int? NumberOfEmployees { get; set; }
    public decimal? Revenue { get; set; }
    public string? IndustryCode { get; set; }

    // -- Relationships --
    /// <summary>Lookup to the primary contact for this account.</summary>
    public Guid? PrimaryContactId { get; set; }

    // -- Sync metadata --
    public string? Crm_LegacyId { get; set; }
    public DateTime? Crm_LastSyncedOn { get; set; }
}
