namespace CrmSync.Interfaces;

using CrmSync.Models.Sync;

/// <summary>
/// Resolves conflicts when a legacy record and CRM record have both been modified.
/// Candidate must implement this interface.
/// </summary>
public interface IConflictResolver
{
    /// <summary>
    /// Determine whether the incoming (legacy) change should be applied,
    /// given the conflict details and the chosen strategy.
    /// Returns true if the legacy change should overwrite CRM.
    /// </summary>
    bool ShouldApplyChange(ConflictInfo conflict, ConflictStrategy strategy);
}
