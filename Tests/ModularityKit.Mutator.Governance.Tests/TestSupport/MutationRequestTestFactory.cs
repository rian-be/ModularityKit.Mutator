using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Abstractions.Intent;
using ModularityKit.Mutator.Governance.Abstractions.Lifecycle;
using ModularityKit.Mutator.Governance.Abstractions.Requests;

namespace ModularityKit.Mutator.Governance.Tests.TestSupport;

internal static class MutationRequestTestFactory
{
    public static MutationRequest CreatePendingRequest()
    {
        return MutationRequest.Pending(
            stateId: "tenant-42:quota",
            stateType: "QuotaPolicy",
            mutationType: "IncreaseQuotaMutation",
            intent: new MutationIntent
            {
                OperationName = "IncreaseQuota",
                Category = "Billing",
                Description = "Raise quota"
            },
            context: MutationContext.User("alice", "Alice", "Need more quota"),
            pendingReason: PendingMutationReason.Approval,
            expectedStateVersion: "v12");
    }

    public static MutationRequest CreateApprovedSecurityRequest(string expectedStateVersion)
    {
        return MutationRequest.Approved(
            stateId: "tenant-42:roles",
            stateType: "IamRoleState",
            mutationType: "GrantRoleMutation",
            intent: new MutationIntent
            {
                OperationName = "GrantRole",
                Category = "Security",
                Description = "Grant elevated access"
            },
            context: MutationContext.User("requester", "Requester", "Need access"),
            expectedStateVersion: expectedStateVersion);
    }
}
