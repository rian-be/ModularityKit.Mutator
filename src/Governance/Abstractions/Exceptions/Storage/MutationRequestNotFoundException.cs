using ModularityKit.Mutator.Abstractions.Exceptions;

namespace ModularityKit.Mutator.Governance.Abstractions.Exceptions.Storage;

/// <summary>
/// Thrown when governance runtime cannot find a mutation request by its stable identifier.
/// </summary>
public sealed class MutationRequestNotFoundException(string requestId)
    : MutationException($"Mutation request '{requestId}' was not found.")
{
    /// <summary>
    /// Stable identifier of the missing request.
    /// </summary>
    public string RequestId { get; } = requestId;
}
