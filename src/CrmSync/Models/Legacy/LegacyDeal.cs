namespace CrmSync.Models.Legacy;

/// <summary>
/// A deal/opportunity record from the LegacyPro CRM system.
/// DO NOT MODIFY.
/// </summary>
public sealed class LegacyDeal
{
    public string DealId { get; set; } = string.Empty;           // e.g. "DEAL-0199"
    public string DealName { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public decimal? ExpectedAmount { get; set; }
    public decimal? FinalAmount { get; set; }
    public string? ExpectedCloseDate { get; set; }               // ISO date string "2025-06-15"
    public string? ActualCloseDate { get; set; }
    public string Stage { get; set; } = "qualifying";            // "qualifying"|"developing"|"proposing"|"closing"|"won"|"lost"
    public int? WinProbability { get; set; }
    public string? CompanyRef { get; set; }                      // FK to LegacyCompany
    public string? CustomerRef { get; set; }                     // FK to LegacyCustomer
    public string Status { get; set; } = "open";                 // "open"|"won"|"lost"|"canceled"
    public long LastUpdatedEpoch { get; set; }
}
