using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Governance.Abstractions.Lifecycle.Model;
using ModularityKit.Mutator.Governance.Abstractions.Requests.Model;
using ModularityKit.Mutator.Governance.Abstractions.Resolution.Contracts;
using ModularityKit.Mutator.Governance.Abstractions.Resolution.Model;
using ModularityKit.Mutator.Governance.Abstractions.Resolution.Strategies;
using ModularityKit.Mutator.Governance.Runtime.Resolution.Evaluation;

namespace ModularityKit.Mutator.Governance.Runtime.Resolution.Execution;

/// <summary>
/// Applies explicit version-aware resolution semantics to governed mutation requests.
/// </summary>
public sealed class MutationRequestVersionResolver : IMutationRequestVersionResolver
{
    public MutationRequestVersionResolution Resolve(
        MutationRequest request,
        string currentStateVersion,
        MutationContext resolutionContext,
        VersionedRequestResolutionStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(resolutionContext);

        var evaluation = MutationRequestVersionEvaluator.Evaluate(request, currentStateVersion);

        if (!evaluation.IsStale)
            return MutationRequestVersionResolutionFactory.BuildValidated(
                request,
                evaluation,
                resolutionContext);

        return strategy switch
        {
            VersionedRequestResolutionStrategy.RejectStale => MutationRequestVersionResolutionFactory.BuildRejectedAsStale(
                request,
                evaluation,
                resolutionContext),
            VersionedRequestResolutionStrategy.RequireRenewedApproval => MutationRequestVersionResolutionFactory.BuildRenewedApprovalRequired(
                request,
                evaluation,
                resolutionContext),
            VersionedRequestResolutionStrategy.RevalidateOnLatestState => MutationRequestVersionResolutionFactory.BuildRevalidationRequired(
                request,
                evaluation,
                resolutionContext),
            _ => throw new ArgumentOutOfRangeException(nameof(strategy), strategy, "Unknown stale-resolution strategy.")
        };
    }
}
