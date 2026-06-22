using ModularityKit.Mutator.Abstractions.Exceptions;
using ModularityKit.Mutator.Governance.Abstractions.Lifecycle;

namespace ModularityKit.Mutator.Governance.Abstractions.Exceptions;

/// <summary>
/// Thrown when governance runtime attempts to apply an invalid lifecycle transition.
/// </summary>
public sealed class InvalidMutationRequestTransitionException(
    string requestId,
    MutationRequestStatus currentStatus,
    MutationRequestStatus targetStatus)
    : MutationException(
        $"Mutation request '{requestId}' cannot transition from '{currentStatus}' to '{targetStatus}'.")
{
    /// <summary>
    /// Stable identifier of the governed mutation request.
    /// </summary>
    public string RequestId { get; } = requestId;

    /// <summary>
    /// Current lifecycle status recorded on the request.
    /// </summary>
    public MutationRequestStatus CurrentStatus { get; } = currentStatus;

    /// <summary>
    /// Target lifecycle status requested by the runtime operation.
    /// </summary>
    public MutationRequestStatus TargetStatus { get; } = targetStatus;
}
