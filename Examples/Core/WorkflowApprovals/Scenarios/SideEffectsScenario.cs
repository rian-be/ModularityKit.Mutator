using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Abstractions.Effects;
using ModularityKit.Mutator.Abstractions.Engine;
using WorkflowApprovals.Mutations;
using WorkflowApprovals.State;

namespace WorkflowApprovals.Scenarios;

internal static class SideEffectsScenario
{
    internal static async Task Run(IMutationEngine engine)
    {
        Console.WriteLine("\n=== Side Effects Scenario ===");

        var state = new ApprovalWorkflowState();

        var startContext = MutationContext.System("Start side effect demo", correlationId: "workflow-side-effects");
        var start = new StartApprovalMutation("initiator", ["SecurityReview", "FinanceReview"], startContext);
        var startResult = await engine.ExecuteAsync(start, state);

        if (!startResult.IsSuccess || startResult.NewState == null)
        {
            Console.WriteLine("✗ Failed to start workflow.");
            return;
        }

        PrintSideEffects("Start workflow", startResult.SideEffects);

        state = startResult.NewState;

        var rejectContext = MutationContext.User("security.lead", reason: "Reject risky request");
        var reject = new RejectWorkflowMutation("security.lead", rejectContext);
        var rejectResult = await engine.ExecuteAsync(reject, state);

        if (!rejectResult.IsSuccess || rejectResult.NewState == null)
        {
            Console.WriteLine("✗ Failed to reject workflow.");
            return;
        }

        PrintSideEffects("Reject workflow", rejectResult.SideEffects);
    }

    private static void PrintSideEffects(string operation, IReadOnlyList<SideEffect> sideEffects)
    {
        Console.WriteLine($"{operation} side effects:");

        foreach (var effect in sideEffects)
        {
            Console.WriteLine(
                $"  {effect.Type} | severity={effect.Severity} | requiresAction={effect.RequiresAction}");
            Console.WriteLine($"    {effect.Description}");

            if (effect.Data is not null)
            {
                Console.WriteLine($"    data={effect.Data}");
            }
        }
    }
}
