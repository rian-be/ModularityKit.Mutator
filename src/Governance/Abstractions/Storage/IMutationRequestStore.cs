using ModularityKit.Mutator.Governance.Abstractions.Lifecycle;
using ModularityKit.Mutator.Governance.Abstractions.Requests;

namespace ModularityKit.Mutator.Governance.Abstractions.Storage;

/// <summary>
/// Stores and retrieves governed mutation requests.
/// </summary>
public interface IMutationRequestStore
{
    /// <summary>
    /// Stores or updates a mutation request.
    /// </summary>
    Task Store(
        MutationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single mutation request by its stable identifier.
    /// </summary>
    Task<MutationRequest?> Get(
        string requestId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all requests for a given state.
    /// </summary>
    Task<IReadOnlyList<MutationRequest>> GetByStateId(
        string stateId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves pending requests for a given state, optionally filtered by pending reason.
    /// </summary>
    Task<IReadOnlyList<MutationRequest>> GetPendingByStateId(
        string stateId,
        PendingMutationReason? reason = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves pending requests, optionally filtered by pending reason.
    /// </summary>
    Task<IReadOnlyList<MutationRequest>> GetPending(
        PendingMutationReason? reason = null,
        CancellationToken cancellationToken = default);
}
