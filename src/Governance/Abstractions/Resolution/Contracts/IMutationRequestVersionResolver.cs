using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Governance.Abstractions.Requests.Model;
using ModularityKit.Mutator.Governance.Abstractions.Resolution.Model;
using ModularityKit.Mutator.Governance.Abstractions.Resolution.Strategies;

namespace ModularityKit.Mutator.Governance.Abstractions.Resolution.Contracts;

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
