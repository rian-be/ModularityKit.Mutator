using ModularityKit.Mutator.Governance.Abstractions.Lifecycle;
using ModularityKit.Mutator.Governance.Abstractions.Requests;
using ModularityKit.Mutator.Governance.Abstractions.Storage;

namespace ModularityKit.Mutator.Governance.Tests.TestSupport;

internal sealed class StaleSnapshotMutationRequestStore(MutationRequest seedRequest) : IMutationRequestStore
{
    private readonly object _gate = new();
    private readonly MutationRequest _seedRequest = seedRequest;
    private readonly List<MutationRequest> _getSnapshots = [];
    private MutationRequest _current = seedRequest;

    public int StoreCount { get; private set; }

    public MutationRequest Current
    {
        get
        {
            lock (_gate)
            {
                return _current;
            }
        }
    }

    public IReadOnlyList<MutationRequest> GetSnapshots => _getSnapshots;

    public Task Store(
        MutationRequest request,
        CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            StoreCount++;
            _current = request;
        }

        return Task.CompletedTask;
    }

    public Task<MutationRequest?> Get(
        string requestId,
        CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            var snapshot = _seedRequest;
            _getSnapshots.Add(snapshot);

            return Task.FromResult<MutationRequest?>(snapshot);
        }
    }

    public Task<IReadOnlyList<MutationRequest>> GetByStateId(
        string stateId,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<MutationRequest>>([]);

    public Task<IReadOnlyList<MutationRequest>> GetPendingByStateId(
        string stateId,
        PendingMutationReason? reason = null,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<MutationRequest>>([]);

    public Task<IReadOnlyList<MutationRequest>> GetPending(
        PendingMutationReason? reason = null,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<MutationRequest>>([]);
}
