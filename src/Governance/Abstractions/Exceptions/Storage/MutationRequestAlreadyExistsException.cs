using ModularityKit.Mutator.Abstractions.Exceptions;

namespace ModularityKit.Mutator.Governance.Abstractions.Exceptions.Storage;

/// <summary>
/// Thrown when governance storage is asked to create a request that already exists.
/// </summary>
public sealed class MutationRequestAlreadyExistsException(string requestId)
    : MutationException($"Mutation request '{requestId}' already exists.")
{
    /// <summary>
    /// Stable identifier of the duplicate request.
    /// </summary>
    public string RequestId { get; } = requestId;
}
