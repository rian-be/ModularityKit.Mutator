namespace ModularityKit.Mutator.Governance;

/// <summary>
/// Stores and retrieves governed mutation requests.
/// </summary>
public interface IMutationRequestStore
{
    /// <summary>
    /// Stores or updates a mutation request.
    /// </summary>
    Task StoreAsync(
        MutationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single mutation request by its stable identifier.
    /// </summary>
    Task<MutationRequest?> GetAsync(
        string requestId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all requests for a given state.
    /// </summary>
    Task<IReadOnlyList<MutationRequest>> GetByStateIdAsync(
        string stateId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves pending requests, optionally filtered by pending reason.
    /// </summary>
    Task<IReadOnlyList<MutationRequest>> GetPendingAsync(
        PendingMutationReason? reason = null,
        CancellationToken cancellationToken = default);
}
