using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Governance.Abstractions.Lifecycle;
using ModularityKit.Mutator.Governance.Runtime.Lifecycle;
using ModularityKit.Mutator.Governance.Tests.TestSupport;
using Xunit;

namespace ModularityKit.Mutator.Governance.Tests.Lifecycle;

public sealed class MutationRequestLifecycleAtomicityTests
{
    [Fact]
    public async Task Stale_snapshot_transition_can_succeed_after_a_prior_lifecycle_update()
    {
        var request = MutationRequestTestFactory.CreatePendingRequest();
        var store = new StaleSnapshotMutationRequestStore(request);
        var manager = new MutationRequestLifecycleManager(store);

        var approved = await manager.Approve(
            request.RequestId,
            MutationContext.User("approver", "Approver", "Approve request"));

        var canceled = await manager.Cancel(
            request.RequestId,
            MutationContext.User("operator", "Operator", "Cancel request"));

        Assert.Equal(2, store.StoreCount);
        Assert.All(store.GetSnapshots, snapshot => Assert.Equal(MutationRequestStatus.Pending, snapshot.Status));
        Assert.Equal(MutationRequestStatus.Approved, approved.Status);
        Assert.Equal(MutationRequestStatus.Canceled, canceled.Status);
        Assert.NotEqual(approved.Status, canceled.Status);
        Assert.Contains(store.Current.Status, new[] { MutationRequestStatus.Approved, MutationRequestStatus.Canceled });
    }
}
