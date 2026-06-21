using ModularityKit.Mutator.Governance;

namespace ModularityKit.Mutator.Governance.Runtime;

/// <summary>
/// In-memory store for governance mutation requests.
/// Suitable for examples, tests, and local development.
/// </summary>
public sealed class InMemoryMutationRequestStore : IMutationRequestStore
{
    private readonly Dictionary<string, MutationRequest> _requests = new();
    private readonly Lock _lock = new();

    public Task StoreAsync(
        MutationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        lock (_lock)
        {
            _requests[request.RequestId] = request;
        }

        return Task.CompletedTask;
    }

    public Task<MutationRequest?> GetAsync(
        string requestId,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _requests.TryGetValue(requestId, out var request);
            return Task.FromResult(request);
        }
    }

    public Task<IReadOnlyList<MutationRequest>> GetByStateIdAsync(
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

    public Task<IReadOnlyList<MutationRequest>> GetPendingAsync(
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
}
