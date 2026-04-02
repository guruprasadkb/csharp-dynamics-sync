namespace CrmSync.Models.Sync;

/// <summary>
/// The type of operation performed during sync.
/// DO NOT MODIFY.
/// </summary>
public enum SyncOperation
{
    Created,
    Updated,
    Skipped,
    Failed,
    Conflict
}
