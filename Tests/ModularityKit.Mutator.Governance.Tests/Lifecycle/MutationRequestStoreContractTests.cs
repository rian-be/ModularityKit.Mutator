using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Governance.Abstractions.Lifecycle;
using ModularityKit.Mutator.Governance.Abstractions.Requests;
using ModularityKit.Mutator.Governance.Runtime.Storage;
using ModularityKit.Mutator.Governance.Tests.TestSupport;
using Xunit;

namespace ModularityKit.Mutator.Governance.Tests.Lifecycle;

public sealed class MutationRequestStoreContractTests
{
    [Fact]
    public async Task Store_contract_allows_blind_overwrite_without_expected_revision_or_status()
    {
        var store = new InMemoryMutationRequestStore();
        var request = MutationRequestTestFactory.CreatePendingRequest();

        await store.Store(request);

        var overwritten = request with
        {
            Status = MutationRequestStatus.Canceled,
            PendingReason = null,
            Decisions =
            [
                .. request.Decisions,
                MutationRequestDecision.Create(
                    MutationRequestDecisionType.Canceled,
                    MutationContext.User("operator", "Operator", "Canceled without guard"))
            ]
        };

        await store.Store(overwritten);

        var loaded = await store.Get(request.RequestId);

        Assert.NotNull(loaded);
        Assert.Equal(MutationRequestStatus.Canceled, loaded.Status);
        Assert.Equal(overwritten.Decisions.Count, loaded.Decisions.Count);
    }
}
