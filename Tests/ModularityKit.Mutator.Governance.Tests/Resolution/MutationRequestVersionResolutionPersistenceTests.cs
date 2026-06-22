using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Governance.Abstractions.Lifecycle;
using ModularityKit.Mutator.Governance.Abstractions.Resolution;
using ModularityKit.Mutator.Governance.Runtime.Resolution;
using ModularityKit.Mutator.Governance.Runtime.Storage;
using ModularityKit.Mutator.Governance.Tests.TestSupport;
using Xunit;

namespace ModularityKit.Mutator.Governance.Tests.Resolution;

public sealed class MutationRequestVersionResolutionPersistenceTests
{
    [Fact]
    public async Task Resolve_does_not_persist_decision_history_unless_caller_saves_the_result()
    {
        var store = new InMemoryMutationRequestStore();
        var resolver = new MutationRequestVersionResolver();
        var request = MutationRequestTestFactory.CreateApprovedSecurityRequest("v10");

        await store.Create(request);

        var resolution = resolver.Resolve(
            request,
            currentStateVersion: "v15",
            resolutionContext: MutationContext.User("approver", "Approver", "Resolve request"),
            strategy: VersionedRequestResolutionStrategy.RejectStale);

        var loaded = await store.Get(request.RequestId);

        Assert.NotNull(loaded);
        Assert.Equal(2, loaded.Decisions.Count);
        Assert.Equal(3, resolution.Request.Decisions.Count);
        Assert.Equal(MutationRequestStatus.Approved, loaded.Status);
        Assert.Equal(MutationRequestStatus.Rejected, resolution.Request.Status);
    }
}
