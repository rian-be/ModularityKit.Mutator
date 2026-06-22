using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Governance.Abstractions.Approval.Contracts;
using ModularityKit.Mutator.Governance.Abstractions.Approval.Model;
using ModularityKit.Mutator.Governance.Abstractions.Exceptions.Approval;
using ModularityKit.Mutator.Governance.Abstractions.Exceptions.Storage;
using ModularityKit.Mutator.Governance.Abstractions.Lifecycle.Model;
using ModularityKit.Mutator.Governance.Abstractions.Requests.Decisions;
using ModularityKit.Mutator.Governance.Abstractions.Requests.Model;
using ModularityKit.Mutator.Governance.Abstractions.Storage;
using ModularityKit.Mutator.Governance.Runtime.Approval.State;

namespace ModularityKit.Mutator.Governance.Runtime.Approval.Execution;

/// <summary>
/// Executes explicit approval and rejection actions for governed mutation requests.
/// </summary>
public sealed class MutationRequestApprovalWorkflowManager(IMutationRequestStore requestStore)
    : IMutationRequestApprovalWorkflowManager
{
    private readonly IMutationRequestStore _requestStore = requestStore ?? throw new ArgumentNullException(nameof(requestStore));

    /// <summary>
    /// Approves a single request-level approval requirement and advances the request when all approvals are satisfied.
    /// </summary>
    public Task<MutationRequest> ApproveRequirement(
        string requestId,
        string approvalId,
        MutationContext decisionContext,
        string? reason = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        return ApplyDecision(
            requestId,
            approvalId,
            decisionContext,
            reason,
            metadata,
            MutationRequestApprovalWorkflowState.ApplyApproval,
            MutationRequestDecisionType.Approval(MutationRequestApprovalDecisionType.Granted),
            finalizeApprovedRequest: true,
            cancellationToken);
    }

    /// <summary>
    /// Rejects a single request-level approval requirement and terminates the request lifecycle.
    /// </summary>
    public Task<MutationRequest> RejectRequirement(
        string requestId,
        string approvalId,
        MutationContext decisionContext,
        string? reason = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        return ApplyDecision(
            requestId,
            approvalId,
            decisionContext,
            reason,
            metadata,
            MutationRequestApprovalWorkflowState.ApplyRejection,
            MutationRequestDecisionType.Approval(MutationRequestApprovalDecisionType.Rejected),
            finalizeApprovedRequest: false,
            cancellationToken);
    }

    private async Task<MutationRequest> ApplyDecision(
        string requestId,
        string approvalId,
        MutationContext decisionContext,
        string? reason,
        IReadOnlyDictionary<string, object>? metadata,
        Func<MutationApprovalRequirement, MutationContext, string?, MutationApprovalRequirement> applyResolution,
        MutationRequestDecisionType decisionType,
        bool finalizeApprovedRequest,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(requestId))
            throw new ArgumentException("Request ID is required.", nameof(requestId));

        if (string.IsNullOrWhiteSpace(approvalId))
            throw new ArgumentException("Approval ID is required.", nameof(approvalId));

        ArgumentNullException.ThrowIfNull(decisionContext);

        var request = await GetRequired(requestId, cancellationToken).ConfigureAwait(false);
        ValidateApprovalWorkflowRequest(request);

        var approvalRequirement = request.ApprovalRequirements.FirstOrDefault(requirement => requirement.ApprovalId == approvalId);
        if (approvalRequirement is null)
            throw new MutationApprovalRequirementNotFoundException(request.RequestId, approvalId);

        ValidateApprovalAction(request, approvalRequirement, decisionContext);

        var resolvedRequirement = applyResolution(approvalRequirement, decisionContext, reason);
        var updatedRequirements = MutationRequestApprovalWorkflowState.Replace(request.ApprovalRequirements, resolvedRequirement);
        var approvalDecision = MutationRequestApprovalWorkflowState.CreateApprovalDecision(
            decisionType,
            resolvedRequirement,
            decisionContext,
            reason,
            metadata);

        var decisions = new List<MutationRequestDecision>(request.Decisions)
        {
            approvalDecision
        };

        var updatedRequest = request with
        {
            ApprovalRequirements = updatedRequirements
        };

        if (finalizeApprovedRequest)
        {
            var isFullyApproved = updatedRequirements.All(requirement => requirement.Status == MutationApprovalRequirementStatus.Approved);
            updatedRequest = updatedRequest with
            {
                Status = isFullyApproved ? MutationRequestStatus.Approved : MutationRequestStatus.Pending,
                PendingReason = isFullyApproved ? null : PendingMutationReason.Approval
            };

            if (isFullyApproved)
            {
                decisions.Add(MutationRequestDecision.Create(
                    MutationRequestDecisionType.Lifecycle(MutationRequestLifecycleDecisionType.Approved),
                    decisionContext,
                    reason: "All approval requirements were fulfilled."));
            }
        }
        else
        {
            updatedRequest = updatedRequest with
            {
                Status = MutationRequestStatus.Rejected,
                PendingReason = null
            };

            decisions.Add(MutationRequestDecision.Create(
                MutationRequestDecisionType.Lifecycle(MutationRequestLifecycleDecisionType.Rejected),
                decisionContext,
                reason: reason ?? decisionContext.Reason ?? "Request was rejected during approval workflow."));
        }

        updatedRequest = updatedRequest with
        {
            Decisions = decisions,
            UpdatedAt = decisions[^1].Timestamp
        };

        var persistedRequest = await _requestStore
            .TryStore(updatedRequest, request.Revision, cancellationToken)
            .ConfigureAwait(false);

        if (persistedRequest is null)
            throw new MutationRequestConcurrencyException(request.RequestId, request.Revision);

        return persistedRequest;
    }

    private static void ValidateApprovalWorkflowRequest(MutationRequest request)
    {
        if (request.Status != MutationRequestStatus.Pending || request.PendingReason != PendingMutationReason.Approval)
            throw new InvalidOperationException(
                $"Request '{request.RequestId}' is not in pending approval state.");

        if (request.ApprovalRequirements.Count == 0)
            throw new InvalidOperationException(
                $"Request '{request.RequestId}' does not define approval requirements.");
    }

    private static void ValidateApprovalAction(
        MutationRequest request,
        MutationApprovalRequirement approvalRequirement,
        MutationContext decisionContext)
    {
        if (approvalRequirement.Status != MutationApprovalRequirementStatus.Pending)
            throw new InvalidMutationApprovalActionException(
                request.RequestId,
                approvalRequirement.ApprovalId,
                $"Approval requirement '{approvalRequirement.ApprovalId}' is already {approvalRequirement.Status}.");

        if (string.IsNullOrWhiteSpace(decisionContext.ActorId))
            throw new InvalidMutationApprovalActionException(
                request.RequestId,
                approvalRequirement.ApprovalId,
                "Approval actions require a user or service actor ID.");

        if (!string.Equals(decisionContext.ActorId, approvalRequirement.ApproverId, StringComparison.Ordinal))
            throw new InvalidMutationApprovalActionException(
                request.RequestId,
                approvalRequirement.ApprovalId,
                $"Actor '{decisionContext.ActorId}' is not the expected approver '{approvalRequirement.ApproverId}'.");

        var currentStep = request.ApprovalRequirements
            .Where(requirement => requirement.Status == MutationApprovalRequirementStatus.Pending)
            .Min(requirement => requirement.StepOrder);

        if (approvalRequirement.StepOrder != currentStep)
            throw new InvalidMutationApprovalActionException(
                request.RequestId,
                approvalRequirement.ApprovalId,
                $"Approval requirement '{approvalRequirement.ApprovalId}' is in step {approvalRequirement.StepOrder}, but current active step is {currentStep}.");
    }

    private async Task<MutationRequest> GetRequired(
        string requestId,
        CancellationToken cancellationToken)
    {
        var request = await _requestStore.Get(requestId, cancellationToken).ConfigureAwait(false);

        if (request is null)
            throw new MutationRequestNotFoundException(requestId);

        return request;
    }
}
