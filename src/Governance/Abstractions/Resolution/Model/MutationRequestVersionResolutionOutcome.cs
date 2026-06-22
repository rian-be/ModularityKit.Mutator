namespace ModularityKit.Mutator.Governance.Abstractions.Resolution.Model;

/// <summary>
/// Describes the outcome of version-aware request resolution.
/// </summary>
public enum MutationRequestVersionResolutionOutcome
{
    ExecuteApprovedVersion = 0,
    RevalidateOnLatestState = 1,
    RejectedAsStale = 2,
    RequiresRenewedApproval = 3
}
