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
        MutationRequestVersionEvaluation evaluation,
        MutationContext resolutionContext)
    {
        var validatedDecision = MutationRequestDecision.Create(
            MutationRequestDecisionType.VersionValidated,
            resolutionContext,
            reason: MutationRequestVersionResolutionState.BuildValidatedReason(
                evaluation.ExpectedStateVersion,
                evaluation.CurrentStateVersion),
            metadata: MutationRequestVersionResolutionState.CreateVersionMetadata(
                evaluation.ExpectedStateVersion,
                evaluation.CurrentStateVersion));

        return new MutationRequestVersionResolution
        {
            Request = MutationRequestVersionResolutionState.AppendDecision(request, validatedDecision),
            Outcome = MutationRequestVersionResolutionOutcome.ExecuteApprovedVersion,
            ExpectedStateVersion = evaluation.ExpectedStateVersion,
            CurrentStateVersion = evaluation.CurrentStateVersion,
            IsStale = false
        };
    }

    /// <summary>
    /// Builds a stale resolution that rejects the request outright.
    /// </summary>
    public static MutationRequestVersionResolution BuildRejectedAsStale(
        MutationRequest request,
        MutationRequestVersionEvaluation evaluation,
        MutationContext resolutionContext)
    {
        var decision = MutationRequestDecision.Create(
            MutationRequestDecisionType.RejectedAsStale,
            resolutionContext,
            reason: MutationRequestVersionResolutionState.BuildStaleReason(
                evaluation.ExpectedStateVersion!,
                evaluation.CurrentStateVersion),
            metadata: MutationRequestVersionResolutionState.CreateVersionMetadata(
                evaluation.ExpectedStateVersion,
                evaluation.CurrentStateVersion));

        var updatedRequest = MutationRequestVersionResolutionState.ApplyRejectedAsStale(
            request,
            evaluation.CurrentStateVersion,
            decision);

        return new MutationRequestVersionResolution
        {
            Request = updatedRequest,
            Outcome = MutationRequestVersionResolutionOutcome.RejectedAsStale,
            ExpectedStateVersion = evaluation.ExpectedStateVersion,
            CurrentStateVersion = evaluation.CurrentStateVersion,
            IsStale = true
        };
    }

    /// <summary>
    /// Builds a stale resolution that moves the request back to pending approval on the latest version.
    /// </summary>
    public static MutationRequestVersionResolution BuildRenewedApprovalRequired(
        MutationRequest request,
        MutationRequestVersionEvaluation evaluation,
        MutationContext resolutionContext)
    {
        var decision = MutationRequestDecision.Create(
            MutationRequestDecisionType.RenewedApprovalRequired,
            resolutionContext,
            reason: MutationRequestVersionResolutionState.BuildStaleReason(
                evaluation.ExpectedStateVersion!,
                evaluation.CurrentStateVersion),
            metadata: MutationRequestVersionResolutionState.CreateVersionMetadata(
                evaluation.ExpectedStateVersion,
                evaluation.CurrentStateVersion));

        var updatedRequest = MutationRequestVersionResolutionState.ApplyRenewedApprovalRequired(
            request,
            evaluation.CurrentStateVersion,
            decision);

        return new MutationRequestVersionResolution
        {
            Request = updatedRequest,
            Outcome = MutationRequestVersionResolutionOutcome.RequiresRenewedApproval,
            ExpectedStateVersion = evaluation.ExpectedStateVersion,
            CurrentStateVersion = evaluation.CurrentStateVersion,
            IsStale = true
        };
    }

    /// <summary>
    /// Builds a stale resolution that keeps the request approved but requires revalidation on the latest version.
    /// </summary>
    public static MutationRequestVersionResolution BuildRevalidationRequired(
        MutationRequest request,
        MutationRequestVersionEvaluation evaluation,
        MutationContext resolutionContext)
    {
        var decision = MutationRequestDecision.Create(
            MutationRequestDecisionType.RevalidationRequired,
            resolutionContext,
            reason: MutationRequestVersionResolutionState.BuildStaleReason(
                evaluation.ExpectedStateVersion!,
                evaluation.CurrentStateVersion),
            metadata: MutationRequestVersionResolutionState.CreateVersionMetadata(
                evaluation.ExpectedStateVersion,
                evaluation.CurrentStateVersion));

        var updatedRequest = MutationRequestVersionResolutionState.ApplyRevalidationRequired(
            request,
            evaluation.CurrentStateVersion,
            decision);

        return new MutationRequestVersionResolution
        {
            Request = updatedRequest,
            Outcome = MutationRequestVersionResolutionOutcome.RevalidateOnLatestState,
            ExpectedStateVersion = evaluation.ExpectedStateVersion,
            CurrentStateVersion = evaluation.CurrentStateVersion,
            IsStale = true
        };
    }
}
