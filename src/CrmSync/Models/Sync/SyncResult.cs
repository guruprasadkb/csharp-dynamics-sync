namespace CrmSync.Models.Sync;

/// <summary>
/// Result of a single entity sync operation.
/// DO NOT MODIFY.
/// </summary>
public sealed class SyncResult
{
    public bool Success { get; set; }
    public Guid? EntityId { get; set; }
    public SyncOperation Operation { get; set; }
    public string? ErrorMessage { get; set; }
    public string? LegacyId { get; set; }
}
