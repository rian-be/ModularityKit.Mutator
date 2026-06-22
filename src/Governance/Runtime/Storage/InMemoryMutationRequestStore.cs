using ModularityKit.Mutator.Governance.Abstractions.Lifecycle;
using ModularityKit.Mutator.Governance.Abstractions.Requests;
using ModularityKit.Mutator.Governance.Abstractions.Storage;
using ModularityKit.Mutator.Governance.Abstractions.Exceptions;

namespace ModularityKit.Mutator.Governance.Runtime.Storage;

/// <summary>
/// In-memory store for governance mutation requests.
/// Suitable for examples, tests, and local development.
/// </summary>
public sealed class InMemoryMutationRequestStore : IMutationRequestStore
{
    private readonly Dictionary<string, MutationRequest> _requests = new();
    private readonly Lock _lock = new();

    public Task<MutationRequest> Create(
        MutationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        lock (_lock)
        {
            if (_requests.ContainsKey(request.RequestId))
                throw new MutationRequestAlreadyExistsException(request.RequestId);

            var persistedRequest = request with
            {
                Revision = 0
            };

            _requests[request.RequestId] = persistedRequest;
            return Task.FromResult(persistedRequest);
        }
    }

    public Task<MutationRequest?> TryStore(
        MutationRequest request,
        long expectedRevision,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        lock (_lock)
        {
            if (!_requests.TryGetValue(request.RequestId, out var currentRequest))
                return Task.FromResult<MutationRequest?>(null);

            if (currentRequest.Revision != expectedRevision)
                return Task.FromResult<MutationRequest?>(null);

            var persistedRequest = request with
            {
                Revision = expectedRevision + 1
            };

            _requests[request.RequestId] = persistedRequest;
            return Task.FromResult<MutationRequest?>(persistedRequest);
        }
    }

    public Task<MutationRequest?> Get(
        string requestId,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _requests.TryGetValue(requestId, out var request);
            return Task.FromResult(request);
        }
    }

    public Task<IReadOnlyList<MutationRequest>> GetByStateId(
        string stateId,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var requests = _requests.Values
                .Where(request => request.StateId == stateId)
                .OrderBy(request => request.CreatedAt)
                .ToList();

            return Task.FromResult<IReadOnlyList<MutationRequest>>(requests);
        }
    }

    public Task<IReadOnlyList<MutationRequest>> GetPending(
        PendingMutationReason? reason = null,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var requests = _requests.Values
                .Where(request =>
                    request.Status == MutationRequestStatus.Pending &&
                    (reason is null || request.PendingReason == reason))
                .OrderBy(request => request.CreatedAt)
                .ToList();

            return Task.FromResult<IReadOnlyList<MutationRequest>>(requests);
        }
    }

    public Task<IReadOnlyList<MutationRequest>> GetPendingByStateId(
        string stateId,
        PendingMutationReason? reason = null,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var requests = _requests.Values
                .Where(request =>
                    request.StateId == stateId &&
                    request.Status == MutationRequestStatus.Pending &&
                    (reason is null || request.PendingReason == reason))
                .OrderBy(request => request.CreatedAt)
                .ToList();

            return Task.FromResult<IReadOnlyList<MutationRequest>>(requests);
        }
    }
}
