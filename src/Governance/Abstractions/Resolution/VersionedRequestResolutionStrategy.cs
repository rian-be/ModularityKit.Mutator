namespace ModularityKit.Mutator.Governance.Abstractions.Resolution;

/// <summary>
/// Strategy to apply when a mutation request is resolved against a newer state version than expected.
/// </summary>
public enum VersionedRequestResolutionStrategy
{
    RejectStale = 0,
    RequireRenewedApproval = 1,
    RevalidateOnLatestState = 2
}
