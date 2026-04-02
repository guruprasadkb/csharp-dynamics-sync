namespace CrmSync.Services;

using CrmSync.Interfaces;
using CrmSync.Models.Sync;

public sealed class ConflictResolver : IConflictResolver
{
    public bool ShouldApplyChange(ConflictInfo conflict, ConflictStrategy strategy)
    {
        return strategy switch
        {
            ConflictStrategy.SourceWins => true,
            ConflictStrategy.TargetWins => false,
            ConflictStrategy.LastWriterWins => conflict.LegacyModifiedOn > conflict.CrmModifiedOn,
            _ => false
        };
    }
}
