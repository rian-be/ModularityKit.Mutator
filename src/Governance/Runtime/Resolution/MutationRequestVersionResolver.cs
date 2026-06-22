using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Governance.Abstractions.Lifecycle;
using ModularityKit.Mutator.Governance.Abstractions.Requests;
using ModularityKit.Mutator.Governance.Abstractions.Resolution;

namespace ModularityKit.Mutator.Governance.Runtime.Resolution;

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

        if (string.IsNullOrWhiteSpace(currentStateVersion))
            throw new ArgumentException("Current state version is required.", nameof(currentStateVersion));

        var expectedStateVersion = request.ExpectedStateVersion;
        var isStale = !string.IsNullOrWhiteSpace(expectedStateVersion) &&
                      !string.Equals(expectedStateVersion, currentStateVersion, StringComparison.Ordinal);

        if (!isStale)
            return MutationRequestVersionResolutionFactory.BuildValidated(
                request,
                expectedStateVersion,
                currentStateVersion,
                resolutionContext);

        return strategy switch
        {
            VersionedRequestResolutionStrategy.RejectStale => MutationRequestVersionResolutionFactory.BuildRejectedAsStale(
                request,
                currentStateVersion,
                resolutionContext),
            VersionedRequestResolutionStrategy.RequireRenewedApproval => MutationRequestVersionResolutionFactory.BuildRenewedApprovalRequired(
                request,
                currentStateVersion,
                resolutionContext),
            VersionedRequestResolutionStrategy.RevalidateOnLatestState => MutationRequestVersionResolutionFactory.BuildRevalidationRequired(
                request,
                currentStateVersion,
                resolutionContext),
            _ => throw new ArgumentOutOfRangeException(nameof(strategy), strategy, "Unknown stale-resolution strategy.")
        };
    }
}
