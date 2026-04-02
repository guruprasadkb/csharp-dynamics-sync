namespace CrmSync.Models.Legacy;

/// <summary>
/// A company record from the LegacyPro CRM system.
/// DO NOT MODIFY.
/// </summary>
public sealed class LegacyCompany
{
    public string CompanyId { get; set; } = string.Empty;        // e.g. "COMP-0007"
    public string CompanyName { get; set; } = string.Empty;
    public string? MainPhone { get; set; }
    public string? FaxNumber { get; set; }
    public string? Website { get; set; }
    public string? ContactEmail { get; set; }
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public string? Country { get; set; }
    public int? EmployeeCount { get; set; }
    public decimal? AnnualRevenue { get; set; }
    public string? Industry { get; set; }
    public string? PrimaryContactRef { get; set; }               // FK to LegacyCustomer.CustomerId
    public string Status { get; set; } = "active";
    public long LastUpdatedEpoch { get; set; }
}
