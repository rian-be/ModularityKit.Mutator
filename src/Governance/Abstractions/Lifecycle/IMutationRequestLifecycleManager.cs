using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Governance.Abstractions.Requests;

namespace ModularityKit.Mutator.Governance.Abstractions.Lifecycle;

/// <summary>
/// Moves governed mutation requests through the runtime pending lifecycle.
/// </summary>
public interface IMutationRequestLifecycleManager
{
    /// <summary>
    /// Stores a newly created request in governance persistence.
    /// </summary>
    Task<MutationRequest> Submit(
        MutationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves an active request into the pending lifecycle with an explicit pending reason.
    /// </summary>
    Task<MutationRequest> MoveToPending(
        string requestId,
        PendingMutationReason pendingReason,
        MutationContext decisionContext,
        string? reason = null,
        DateTimeOffset? expiresAt = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a request as approved and ready for execution.
    /// </summary>
    Task<MutationRequest> Approve(
        string requestId,
        MutationContext decisionContext,
        string? reason = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a request as rejected.
    /// </summary>
    Task<MutationRequest> Reject(
        string requestId,
        MutationContext decisionContext,
        string? reason = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels an active request through an explicit runtime path.
    /// </summary>
    Task<MutationRequest> Cancel(
        string requestId,
        MutationContext decisionContext,
        string? reason = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Expires a pending request.
    /// </summary>
    Task<MutationRequest> Expire(
        string requestId,
        MutationContext decisionContext,
        string? reason = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Expires all pending requests whose expiration time has passed.
    /// </summary>
    Task<IReadOnlyList<MutationRequest>> ExpireDueRequests(
        DateTimeOffset now,
        MutationContext decisionContext,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a request as superseded by a newer request.
    /// </summary>
    Task<MutationRequest> Supersede(
        string requestId,
        string supersedingRequestId,
        MutationContext decisionContext,
        string? reason = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an approved request as executed.
    /// </summary>
    Task<MutationRequest> MarkExecuted(
        string requestId,
        MutationContext decisionContext,
        string? reason = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists pending requests, optionally filtered by reason.
    /// </summary>
    Task<IReadOnlyList<MutationRequest>> GetPending(
        PendingMutationReason? reason = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists pending requests for a specific state, optionally filtered by reason.
    /// </summary>
    Task<IReadOnlyList<MutationRequest>> GetPendingByStateId(
        string stateId,
        PendingMutationReason? reason = null,
        CancellationToken cancellationToken = default);
}
