namespace CrmSync.Models.Sync;

/// <summary>
/// Strategy for resolving conflicts between legacy and CRM data.
/// DO NOT MODIFY.
/// </summary>
public enum ConflictStrategy
{
    /// <summary>Source (legacy) always wins -- overwrite CRM.</summary>
    SourceWins,

    /// <summary>Target (CRM) always wins -- skip the update.</summary>
    TargetWins,

    /// <summary>Most recent modification wins.</summary>
    LastWriterWins
}
