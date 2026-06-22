using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Governance.Abstractions.Exceptions;
using ModularityKit.Mutator.Governance.Abstractions.Lifecycle;
using ModularityKit.Mutator.Governance.Abstractions.Requests;
using ModularityKit.Mutator.Governance.Abstractions.Storage;

namespace ModularityKit.Mutator.Governance.Runtime.Lifecycle;

/// <summary>
/// Applies explicit runtime transitions to governed mutation requests and persists decision history.
/// </summary>
public sealed class MutationRequestLifecycleManager(IMutationRequestStore requestStore) : IMutationRequestLifecycleManager
{
    private readonly IMutationRequestStore _requestStore = requestStore ?? throw new ArgumentNullException(nameof(requestStore));

    public async Task<MutationRequest> Submit(
        MutationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await _requestStore.Store(request, cancellationToken).ConfigureAwait(false);
        return request;
    }

    public Task<MutationRequest> MoveToPending(
        string requestId,
        PendingMutationReason pendingReason,
        MutationContext decisionContext,
        string? reason = null,
        DateTimeOffset? expiresAt = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        return Transition(
            requestId,
            MutationRequestStatus.Pending,
            MutationRequestDecisionType.Pending,
            decisionContext,
            reason,
            request => request with
            {
                PendingReason = pendingReason,
                ExpiresAt = expiresAt
            },
            metadata,
            cancellationToken);
    }

    public Task<MutationRequest> Approve(
        string requestId,
        MutationContext decisionContext,
        string? reason = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        return Transition(
            requestId,
            MutationRequestStatus.Approved,
            MutationRequestDecisionType.Approved,
            decisionContext,
            reason,
            request => request with
            {
                PendingReason = null
            },
            metadata,
            cancellationToken);
    }

    public Task<MutationRequest> Reject(
        string requestId,
        MutationContext decisionContext,
        string? reason = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        return Transition(
            requestId,
            MutationRequestStatus.Rejected,
            MutationRequestDecisionType.Rejected,
            decisionContext,
            reason,
            ClearPendingState,
            metadata,
            cancellationToken);
    }

    public Task<MutationRequest> Cancel(
        string requestId,
        MutationContext decisionContext,
        string? reason = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        return Transition(
            requestId,
            MutationRequestStatus.Canceled,
            MutationRequestDecisionType.Canceled,
            decisionContext,
            reason,
            ClearPendingState,
            metadata,
            cancellationToken);
    }

    public Task<MutationRequest> Expire(
        string requestId,
        MutationContext decisionContext,
        string? reason = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        return Transition(
            requestId,
            MutationRequestStatus.Expired,
            MutationRequestDecisionType.Expired,
            decisionContext,
            reason,
            ClearPendingState,
            metadata,
            cancellationToken);
    }

    public async Task<IReadOnlyList<MutationRequest>> ExpireDueRequests(
        DateTimeOffset now,
        MutationContext decisionContext,
        CancellationToken cancellationToken = default)
    {
        var pendingRequests = await _requestStore
            .GetPending(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var expiredRequests = new List<MutationRequest>();

        foreach (var request in pendingRequests)
        {
            if (request.ExpiresAt is null || request.ExpiresAt > now)
                continue;

            var reason = $"Pending request expired at '{request.ExpiresAt:O}'.";

            var expiredRequest = await Expire(
                request.RequestId,
                decisionContext,
                reason,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            expiredRequests.Add(expiredRequest);
        }

        return expiredRequests;
    }

    public Task<MutationRequest> Supersede(
        string requestId,
        string supersedingRequestId,
        MutationContext decisionContext,
        string? reason = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(supersedingRequestId))
            throw new ArgumentException("Superseding request ID is required.", nameof(supersedingRequestId));

        var transitionMetadata = MergeMetadata(
            metadata,
            new Dictionary<string, object>
            {
                ["SupersedingRequestId"] = supersedingRequestId
            });

        return Transition(
            requestId,
            MutationRequestStatus.Superseded,
            MutationRequestDecisionType.Superseded,
            decisionContext,
            reason ?? $"Superseded by request '{supersedingRequestId}'.",
            ClearPendingState,
            transitionMetadata,
            cancellationToken);
    }

    public Task<MutationRequest> MarkExecuted(
        string requestId,
        MutationContext decisionContext,
        string? reason = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        return Transition(
            requestId,
            MutationRequestStatus.Executed,
            MutationRequestDecisionType.Executed,
            decisionContext,
            reason,
            ClearPendingState,
            metadata,
            cancellationToken);
    }

    public Task<IReadOnlyList<MutationRequest>> GetPending(
        PendingMutationReason? reason = null,
        CancellationToken cancellationToken = default)
    {
        return _requestStore.GetPending(reason, cancellationToken);
    }

    public Task<IReadOnlyList<MutationRequest>> GetPendingByStateId(
        string stateId,
        PendingMutationReason? reason = null,
        CancellationToken cancellationToken = default)
    {
        return _requestStore.GetPendingByStateId(stateId, reason, cancellationToken);
    }

    private async Task<MutationRequest> Transition(
        string requestId,
        MutationRequestStatus targetStatus,
        MutationRequestDecisionType decisionType,
        MutationContext decisionContext,
        string? reason,
        Func<MutationRequest, MutationRequest> applyState,
        IReadOnlyDictionary<string, object>? metadata,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(requestId))
            throw new ArgumentException("Request ID is required.", nameof(requestId));

        ArgumentNullException.ThrowIfNull(decisionContext);
        ArgumentNullException.ThrowIfNull(applyState);

        var request = await GetRequired(requestId, cancellationToken).ConfigureAwait(false);

        ValidateTransition(request.Status, targetStatus, request.RequestId);

        var decision = MutationRequestDecision.Create(
            decisionType,
            decisionContext,
            reason,
            metadata);

        var updatedRequest = applyState(request) with
        {
            Status = targetStatus,
            UpdatedAt = decision.Timestamp,
            Decisions = [.. request.Decisions, decision]
        };

        await _requestStore.Store(updatedRequest, cancellationToken).ConfigureAwait(false);
        return updatedRequest;
    }

    private async Task<MutationRequest> GetRequired(
        string requestId,
        CancellationToken cancellationToken)
    {
        var request = await _requestStore.Get(requestId, cancellationToken).ConfigureAwait(false);

        if (request is null)
            throw new MutationRequestNotFoundException(requestId);

        return request;
    }

    private static void ValidateTransition(
        MutationRequestStatus currentStatus,
        MutationRequestStatus targetStatus,
        string requestId)
    {
        if (currentStatus == targetStatus)
            throw new InvalidMutationRequestTransitionException(requestId, currentStatus, targetStatus);

        var isValid = currentStatus switch
        {
            MutationRequestStatus.Created => targetStatus is
                MutationRequestStatus.Pending or
                MutationRequestStatus.Approved or
                MutationRequestStatus.Canceled or
                MutationRequestStatus.Superseded,
            MutationRequestStatus.Pending => targetStatus is
                MutationRequestStatus.Approved or
                MutationRequestStatus.Rejected or
                MutationRequestStatus.Canceled or
                MutationRequestStatus.Expired or
                MutationRequestStatus.Superseded,
            MutationRequestStatus.Approved => targetStatus is
                MutationRequestStatus.Pending or
                MutationRequestStatus.Rejected or
                MutationRequestStatus.Canceled or
                MutationRequestStatus.Superseded or
                MutationRequestStatus.Executed,
            MutationRequestStatus.Rejected => false,
            MutationRequestStatus.Canceled => false,
            MutationRequestStatus.Expired => false,
            MutationRequestStatus.Superseded => false,
            MutationRequestStatus.Executed => false,
            _ => false
        };

        if (!isValid)
            throw new InvalidMutationRequestTransitionException(requestId, currentStatus, targetStatus);
    }

    private static MutationRequest ClearPendingState(MutationRequest request)
    {
        return request with
        {
            PendingReason = null,
            ExpiresAt = null
        };
    }

    private static IReadOnlyDictionary<string, object> MergeMetadata(
        IReadOnlyDictionary<string, object>? metadata,
        IReadOnlyDictionary<string, object> appended)
    {
        var merged = new Dictionary<string, object>();

        if (metadata is not null)
        {
            foreach (var pair in metadata)
            {
                merged[pair.Key] = pair.Value;
            }
        }

        foreach (var pair in appended)
        {
            merged[pair.Key] = pair.Value;
        }

        return merged;
    }
}
