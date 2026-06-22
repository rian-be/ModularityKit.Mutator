namespace ModularityKit.Mutator.Governance.Abstractions.Requests.Decisions;

/// <summary>
/// Represents version-resolution decisions recorded while reconciling expected and current state versions.
/// </summary>
public enum MutationRequestVersionResolutionDecisionType
{
    Validated = 0,
    RevalidationRequired = 1,
    RenewedApprovalRequired = 2,
    RejectedAsStale = 3
}
