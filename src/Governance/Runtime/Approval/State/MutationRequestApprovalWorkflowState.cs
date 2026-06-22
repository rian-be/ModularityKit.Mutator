using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Governance.Abstractions.Approval.Model;
using ModularityKit.Mutator.Governance.Abstractions.Requests.Decisions;
using ModularityKit.Mutator.Governance.Abstractions.Requests.Model;

namespace ModularityKit.Mutator.Governance.Runtime.Approval.State;

internal static class MutationRequestApprovalWorkflowState
{
    public static MutationApprovalRequirement ApplyApproval(
        MutationApprovalRequirement requirement,
        MutationContext decisionContext,
        string? reason)
    {
        return requirement with
        {
            Status = MutationApprovalRequirementStatus.Approved,
            DecidedAt = decisionContext.Timestamp,
            DecisionContext = decisionContext,
            DecisionReason = reason ?? decisionContext.Reason
        };
    }

    public static MutationApprovalRequirement ApplyRejection(
        MutationApprovalRequirement requirement,
        MutationContext decisionContext,
        string? reason)
    {
        return requirement with
        {
            Status = MutationApprovalRequirementStatus.Rejected,
            DecidedAt = decisionContext.Timestamp,
            DecisionContext = decisionContext,
            DecisionReason = reason ?? decisionContext.Reason
        };
    }

    public static IReadOnlyList<MutationApprovalRequirement> Replace(
        IReadOnlyList<MutationApprovalRequirement> requirements,
        MutationApprovalRequirement updated)
    {
        return requirements
            .Select(requirement => requirement.ApprovalId == updated.ApprovalId ? updated : requirement)
            .ToList();
    }

    public static MutationRequestDecision CreateApprovalDecision(
        MutationRequestDecisionType decisionType,
        MutationApprovalRequirement requirement,
        MutationContext decisionContext,
        string? reason,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        var mergedMetadata = new Dictionary<string, object>
        {
            ["ApprovalId"] = requirement.ApprovalId,
            ["ApproverId"] = requirement.ApproverId,
            ["StepOrder"] = requirement.StepOrder
        };

        if (metadata is not null)
        {
            foreach (var pair in metadata)
            {
                mergedMetadata[pair.Key] = pair.Value;
            }
        }

        return MutationRequestDecision.Create(
            decisionType,
            decisionContext,
            reason ?? decisionContext.Reason,
            mergedMetadata);
    }
}
