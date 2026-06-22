using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Abstractions.Intent;
using ModularityKit.Mutator.Governance.Abstractions.Requests;
using ModularityKit.Mutator.Governance.Abstractions.Resolution;
using ModularityKit.Mutator.Governance.Runtime.Resolution;

namespace VersionedResolution.Scenarios;

internal static class GovernanceVersionedResolutionScenario
{
    public static void Run()
    {
        var resolver = new MutationRequestVersionResolver();

        PrintSection("Current Version Matches Expected Version");
        PrintResolution(
            resolver.Resolve(
                CreateApprovedRequest("v10"),
                currentStateVersion: "v10",
                resolutionContext: MutationContext.User("approver-1", "Approver One", "Current version verified"),
                strategy: VersionedRequestResolutionStrategy.RejectStale));

        PrintSection("Reject Stale");
        PrintResolution(
            resolver.Resolve(
                CreateApprovedRequest("v10"),
                currentStateVersion: "v15",
                resolutionContext: MutationContext.User("approver-2", "Approver Two", "Reject stale request"),
                strategy: VersionedRequestResolutionStrategy.RejectStale));

        PrintSection("Require Renewed Approval");
        PrintResolution(
            resolver.Resolve(
                CreateApprovedRequest("v10"),
                currentStateVersion: "v15",
                resolutionContext: MutationContext.User("approver-3", "Approver Three", "Request renewed approval"),
                strategy: VersionedRequestResolutionStrategy.RequireRenewedApproval));

        PrintSection("Revalidate On Latest State");
        PrintResolution(
            resolver.Resolve(
                CreateApprovedRequest("v10"),
                currentStateVersion: "v15",
                resolutionContext: MutationContext.User("approver-4", "Approver Four", "Revalidate on the latest state"),
                strategy: VersionedRequestResolutionStrategy.RevalidateOnLatestState));
    }

    private static MutationRequest CreateApprovedRequest(string expectedStateVersion)
    {
        return MutationRequest.Approved(
            stateId: "tenant-42:roles",
            stateType: "IamRoleState",
            mutationType: "GrantRoleMutation",
            intent: new MutationIntent
            {
                OperationName = "GrantRole",
                Category = "Security",
                Description = "Grant elevated role to tenant operator"
            },
            context: MutationContext.User("requester-1", "Requester One", "Need elevated access for incident"),
            expectedStateVersion: expectedStateVersion);
    }

    private static void PrintSection(string title)
    {
        Console.WriteLine();
        Console.WriteLine($"=== {title} ===");
    }

    private static void PrintResolution(MutationRequestVersionResolution resolution)
    {
        var decision = resolution.Request.Decisions[^1];

        Console.WriteLine($"Outcome: {resolution.Outcome}");
        Console.WriteLine($"Was stale: {resolution.IsStale}");
        Console.WriteLine($"Expected version: {resolution.ExpectedStateVersion ?? "-"}");
        Console.WriteLine($"Current version: {resolution.CurrentStateVersion}");
        Console.WriteLine($"Request status: {resolution.Request.Status}");
        Console.WriteLine($"Next expected version: {resolution.Request.ExpectedStateVersion ?? "-"}");
        Console.WriteLine($"Last decision: {decision.Type}");
        Console.WriteLine($"Decision reason: {decision.Reason}");
    }
}
