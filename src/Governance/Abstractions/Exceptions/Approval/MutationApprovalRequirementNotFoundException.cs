namespace ModularityKit.Mutator.Governance.Abstractions.Exceptions.Approval;

/// <summary>
/// Raised when a governed request does not contain the targeted approval requirement.
/// </summary>
public sealed class MutationApprovalRequirementNotFoundException(
    string requestId,
    string approvalId) : KeyNotFoundException(
    $"Approval requirement '{approvalId}' was not found on request '{requestId}'.")
{
    /// <summary>
    /// Request identifier that was queried.
    /// </summary>
    public string RequestId { get; } = requestId;

    /// <summary>
    /// Missing approval identifier.
    /// </summary>
    public string ApprovalId { get; } = approvalId;
}
