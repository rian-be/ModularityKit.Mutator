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
        {
            var validatedDecision = MutationRequestDecision.Create(
                MutationRequestDecisionType.VersionValidated,
                resolutionContext,
                reason: string.IsNullOrWhiteSpace(expectedStateVersion)
                    ? "No expected state version was provided. Request can proceed."
                    : $"State version '{currentStateVersion}' matches the expected version.",
                metadata: CreateVersionMetadata(expectedStateVersion, currentStateVersion));

            return new MutationRequestVersionResolution
            {
                Request = AppendDecision(request, validatedDecision),
                Outcome = MutationRequestVersionResolutionOutcome.ExecuteApprovedVersion,
                ExpectedStateVersion = expectedStateVersion,
                CurrentStateVersion = currentStateVersion,
                IsStale = false
            };
        }

        return strategy switch
        {
            VersionedRequestResolutionStrategy.RejectStale => BuildRejectedAsStale(
                request,
                currentStateVersion,
                resolutionContext),
            VersionedRequestResolutionStrategy.RequireRenewedApproval => BuildRenewedApprovalRequired(
                request,
                currentStateVersion,
                resolutionContext),
            VersionedRequestResolutionStrategy.RevalidateOnLatestState => BuildRevalidationRequired(
                request,
                currentStateVersion,
                resolutionContext),
            _ => throw new ArgumentOutOfRangeException(nameof(strategy), strategy, "Unknown stale-resolution strategy.")
        };
    }

    private static MutationRequestVersionResolution BuildRejectedAsStale(
        MutationRequest request,
        string currentStateVersion,
        MutationContext resolutionContext)
    {
        var decision = MutationRequestDecision.Create(
            MutationRequestDecisionType.RejectedAsStale,
            resolutionContext,
            reason: BuildStaleReason(request.ExpectedStateVersion!, currentStateVersion),
            metadata: CreateVersionMetadata(request.ExpectedStateVersion, currentStateVersion));

        var updatedRequest = AppendDecision(
            request with
            {
                Status = MutationRequestStatus.Rejected,
                PendingReason = null,
                UpdatedAt = decision.Timestamp
            },
            decision);

        return new MutationRequestVersionResolution
        {
            Request = updatedRequest,
            Outcome = MutationRequestVersionResolutionOutcome.RejectedAsStale,
            ExpectedStateVersion = request.ExpectedStateVersion,
            CurrentStateVersion = currentStateVersion,
            IsStale = true
        };
    }

    private static MutationRequestVersionResolution BuildRenewedApprovalRequired(
        MutationRequest request,
        string currentStateVersion,
        MutationContext resolutionContext)
    {
        var decision = MutationRequestDecision.Create(
            MutationRequestDecisionType.RenewedApprovalRequired,
            resolutionContext,
            reason: BuildStaleReason(request.ExpectedStateVersion!, currentStateVersion),
            metadata: CreateVersionMetadata(request.ExpectedStateVersion, currentStateVersion));

        var updatedRequest = AppendDecision(
            request with
            {
                Status = MutationRequestStatus.Pending,
                PendingReason = PendingMutationReason.Approval,
                ExpectedStateVersion = currentStateVersion,
                UpdatedAt = decision.Timestamp
            },
            decision);

        return new MutationRequestVersionResolution
        {
            Request = updatedRequest,
            Outcome = MutationRequestVersionResolutionOutcome.RequiresRenewedApproval,
            ExpectedStateVersion = request.ExpectedStateVersion,
            CurrentStateVersion = currentStateVersion,
            IsStale = true
        };
    }

    private static MutationRequestVersionResolution BuildRevalidationRequired(
        MutationRequest request,
        string currentStateVersion,
        MutationContext resolutionContext)
    {
        var decision = MutationRequestDecision.Create(
            MutationRequestDecisionType.RevalidationRequired,
            resolutionContext,
            reason: BuildStaleReason(request.ExpectedStateVersion!, currentStateVersion),
            metadata: CreateVersionMetadata(request.ExpectedStateVersion, currentStateVersion));

        var updatedRequest = AppendDecision(
            request with
            {
                Status = MutationRequestStatus.Approved,
                PendingReason = null,
                ExpectedStateVersion = currentStateVersion,
                UpdatedAt = decision.Timestamp
            },
            decision);

        return new MutationRequestVersionResolution
        {
            Request = updatedRequest,
            Outcome = MutationRequestVersionResolutionOutcome.RevalidateOnLatestState,
            ExpectedStateVersion = request.ExpectedStateVersion,
            CurrentStateVersion = currentStateVersion,
            IsStale = true
        };
    }

    private static MutationRequest AppendDecision(
        MutationRequest request,
        MutationRequestDecision decision)
    {
        return request with
        {
            Decisions = [.. request.Decisions, decision]
        };
    }

    private static string BuildStaleReason(string expectedStateVersion, string currentStateVersion)
    {
        return $"Request expected state version '{expectedStateVersion}' but current version is '{currentStateVersion}'.";
    }

    private static IReadOnlyDictionary<string, object> CreateVersionMetadata(
        string? expectedStateVersion,
        string currentStateVersion)
    {
        return new Dictionary<string, object>
        {
            ["ExpectedStateVersion"] = expectedStateVersion ?? string.Empty,
            ["CurrentStateVersion"] = currentStateVersion
        };
    }
}
