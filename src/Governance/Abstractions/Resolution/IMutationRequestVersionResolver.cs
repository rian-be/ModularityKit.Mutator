using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Governance.Abstractions.Resolution;
using ModularityKit.Mutator.Governance.Abstractions.Requests;

namespace ModularityKit.Mutator.Governance.Abstractions.Resolution;

/// <summary>
/// Resolves a governed mutation request against the current state version before execution.
/// </summary>
public interface IMutationRequestVersionResolver
{
    /// <summary>
    /// Resolves the request against the current state version using the selected stale-resolution strategy.
    /// </summary>
    MutationRequestVersionResolution Resolve(
        MutationRequest request,
        string currentStateVersion,
        MutationContext resolutionContext,
        VersionedRequestResolutionStrategy strategy);
}
