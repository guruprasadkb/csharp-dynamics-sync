namespace CrmSync.Models.Sync;

/// <summary>
/// Aggregate result of a batch sync operation.
/// DO NOT MODIFY.
/// </summary>
public sealed class BatchResult
{
    public int TotalRecords { get; set; }
    public int Created { get; set; }
    public int Updated { get; set; }
    public int Skipped { get; set; }
    public int Failed { get; set; }
    public int Conflicts { get; set; }
    public List<SyncResult> Results { get; set; } = [];
    public TimeSpan Duration { get; set; }
}
