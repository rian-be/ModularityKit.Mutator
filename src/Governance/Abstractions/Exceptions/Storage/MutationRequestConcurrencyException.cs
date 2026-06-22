using ModularityKit.Mutator.Abstractions.Exceptions;

namespace ModularityKit.Mutator.Governance.Abstractions.Exceptions.Storage;

/// <summary>
/// Thrown when a governance request transition loses an optimistic concurrency race.
/// </summary>
public sealed class MutationRequestConcurrencyException(
    string requestId,
    long expectedRevision)
    : MutationException(
        $"Mutation request '{requestId}' could not be updated because revision '{expectedRevision}' is stale.")
{
    /// <summary>
    /// Stable identifier of the request that lost the concurrency race.
    /// </summary>
    public string RequestId { get; } = requestId;

    /// <summary>
    /// Revision the runtime expected to update.
    /// </summary>
    public long ExpectedRevision { get; } = expectedRevision;
}
