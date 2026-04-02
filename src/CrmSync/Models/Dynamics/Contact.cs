namespace CrmSync.Models.Dynamics;

/// <summary>
/// Dynamics 365 Contact entity.
/// Field names follow Dynamics logical naming conventions exactly.
/// DO NOT MODIFY -- this is the target CRM schema.
/// </summary>
public sealed class Contact : EntityBase
{
    // -- Name fields (Dynamics splits into first/last) --
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    /// <summary>Computed. In real Dynamics this is auto-composed from first + last.</summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    // -- Communication --
    public string? EmailAddress1 { get; set; }
    public string? EmailAddress2 { get; set; }
    public string? Telephone1 { get; set; }     // Business phone
    public string? MobilePhone { get; set; }

    // -- Address (composite address 1) --
    public string? Address1_Line1 { get; set; }
    public string? Address1_City { get; set; }
    public string? Address1_StateOrProvince { get; set; }
    public string? Address1_PostalCode { get; set; }
    public string? Address1_Country { get; set; }

    // -- Job info --
    public string? JobTitle { get; set; }
    public string? Department { get; set; }

    // -- Relationships --
    /// <summary>
    /// Lookup to the parent Account. In Dynamics this is a polymorphic
    /// lookup (could also point to Contact) but for this challenge it
    /// always points to Account.
    /// </summary>
    public Guid? ParentCustomerId { get; set; }

    // -- Sync metadata (custom fields added for migration tracking) --
    /// <summary>Custom field: ID from the legacy system, used for dedup.</summary>
    public string? Crm_LegacyId { get; set; }

    /// <summary>Custom field: timestamp of last successful inbound sync.</summary>
    public DateTime? Crm_LastSyncedOn { get; set; }
}
