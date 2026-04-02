namespace CrmSync.Models.Sync;

/// <summary>
/// Information about a conflict between legacy and CRM records.
/// DO NOT MODIFY.
/// </summary>
public sealed class ConflictInfo
{
    public Guid EntityId { get; set; }
    public string LegacyId { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public DateTime LegacyModifiedOn { get; set; }
    public DateTime CrmModifiedOn { get; set; }
    public long ExpectedVersion { get; set; }
    public long ActualVersion { get; set; }
}
