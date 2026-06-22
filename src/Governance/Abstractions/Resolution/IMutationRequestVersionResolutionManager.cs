using ModularityKit.Mutator.Abstractions.Context;

namespace ModularityKit.Mutator.Governance.Abstractions.Resolution;

/// <summary>
/// Persists version-aware resolution outcomes for governed mutation requests.
/// </summary>
public interface IMutationRequestVersionResolutionManager
{
    /// <summary>
    /// Resolves a persisted request against the current state version and stores the resulting request state and decision history.
    /// </summary>
    Task<MutationRequestVersionResolution> ResolveAndStore(
        string requestId,
        string currentStateVersion,
        MutationContext resolutionContext,
        VersionedRequestResolutionStrategy strategy,
        CancellationToken cancellationToken = default);
}
