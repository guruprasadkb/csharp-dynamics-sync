namespace CrmSync.Models.Legacy;

/// <summary>
/// A contact record from the LegacyPro CRM system.
/// Notice the flat structure and different naming conventions compared to Dynamics 365.
/// DO NOT MODIFY -- this represents the source system's data format.
/// </summary>
public sealed class LegacyCustomer
{
    public string CustomerId { get; set; } = string.Empty;       // e.g. "CUST-00042"
    public string FullName { get; set; } = string.Empty;          // "Jane Doe" -- must be split
    public string Email { get; set; } = string.Empty;
    public string? SecondaryEmail { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public string? Country { get; set; }
    public string? Title { get; set; }                            // Job title
    public string? Dept { get; set; }
    public string? CompanyRef { get; set; }                       // FK to LegacyCompany.CompanyId
    public string Status { get; set; } = "active";               // "active" | "inactive" | "deleted"
    public long LastUpdatedEpoch { get; set; }                    // Unix epoch seconds
}
