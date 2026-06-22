using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Governance.Abstractions.Lifecycle;
using ModularityKit.Mutator.Governance.Abstractions.Requests;
using ModularityKit.Mutator.Governance.Abstractions.Resolution;

namespace ModularityKit.Mutator.Governance.Runtime.Resolution;

/// <summary>
/// Builds concrete version-resolution outcomes and updated request snapshots for governance runtime flows.
/// </summary>
internal static class MutationRequestVersionResolutionFactory
{
    /// <summary>
    /// Builds the non-stale resolution outcome for a request that can proceed immediately.
    /// </summary>
    public static MutationRequestVersionResolution BuildValidated(
        MutationRequest request,
        string? expectedStateVersion,
        string currentStateVersion,
        MutationContext resolutionContext)
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

    /// <summary>
    /// Builds a stale resolution that rejects the request outright.
    /// </summary>
    public static MutationRequestVersionResolution BuildRejectedAsStale(
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

    /// <summary>
    /// Builds a stale resolution that moves the request back to pending approval on the latest version.
    /// </summary>
    public static MutationRequestVersionResolution BuildRenewedApprovalRequired(
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

    /// <summary>
    /// Builds a stale resolution that keeps the request approved but requires revalidation on the latest version.
    /// </summary>
    public static MutationRequestVersionResolution BuildRevalidationRequired(
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
