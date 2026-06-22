namespace ModularityKit.Mutator.Governance.Abstractions.Approval.Model;

/// <summary>
/// Represents the current state of a request-level approval requirement.
/// </summary>
public enum MutationApprovalRequirementStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}
