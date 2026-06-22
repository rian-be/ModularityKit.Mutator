namespace ModularityKit.Mutator.Governance.Abstractions.Exceptions.Approval;

/// <summary>
/// Raised when an approval action is not valid for the current request or approval state.
/// </summary>
public sealed class InvalidMutationApprovalActionException(
    string requestId,
    string approvalId,
    string message) : InvalidOperationException(message)
{
    /// <summary>
    /// Request identifier on which the invalid approval action was attempted.
    /// </summary>
    public string RequestId { get; } = requestId;

    /// <summary>
    /// Approval identifier on which the invalid action was attempted.
    /// </summary>
    public string ApprovalId { get; } = approvalId;
}
