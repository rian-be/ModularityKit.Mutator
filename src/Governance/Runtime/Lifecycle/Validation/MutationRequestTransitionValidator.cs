using System.Collections.Frozen;
using ModularityKit.Mutator.Governance.Abstractions.Exceptions.Lifecycle;
using ModularityKit.Mutator.Governance.Abstractions.Lifecycle.Model;

namespace ModularityKit.Mutator.Governance.Runtime.Lifecycle.Validation;

/// <summary>
/// Validates whether a governed mutation request can move from one lifecycle status to another.
/// </summary>
internal static class MutationRequestTransitionValidator
{
    private static readonly FrozenDictionary<MutationRequestStatus, FrozenSet<MutationRequestStatus>> AllowedTransitions =
        new Dictionary<MutationRequestStatus, FrozenSet<MutationRequestStatus>>
        {
            [MutationRequestStatus.Created] =
            [
                MutationRequestStatus.Pending,
                MutationRequestStatus.Approved,
                MutationRequestStatus.Canceled,
                MutationRequestStatus.Superseded
            ],
            [MutationRequestStatus.Pending] =
            [
                MutationRequestStatus.Approved,
                MutationRequestStatus.Rejected,
                MutationRequestStatus.Canceled,
                MutationRequestStatus.Expired,
                MutationRequestStatus.Superseded
            ],
            [MutationRequestStatus.Approved] =
            [
                MutationRequestStatus.Pending,
                MutationRequestStatus.Rejected,
                MutationRequestStatus.Canceled,
                MutationRequestStatus.Superseded,
                MutationRequestStatus.Executed
            ]
        }.ToFrozenDictionary();

    /// <summary>
    /// Ensures the target status is allowed for the current request status.
    /// </summary>
    public static void Validate(
        MutationRequestStatus currentStatus,
        MutationRequestStatus targetStatus,
        string requestId)
    {
        if (currentStatus == targetStatus)
            throw new InvalidMutationRequestTransitionException(requestId, currentStatus, targetStatus);

        var isValid = AllowedTransitions.TryGetValue(currentStatus, out var validTargets)
            && validTargets.Contains(targetStatus);

        if (!isValid)
            throw new InvalidMutationRequestTransitionException(requestId, currentStatus, targetStatus);
    }
}
