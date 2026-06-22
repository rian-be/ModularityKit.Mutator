using FeatureFlags.Mutations;
using FeatureFlags.State;
using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Abstractions.Engine;
using ModularityKit.Mutator.Abstractions.Policies;
using ModularityKit.Mutator.Abstractions.Results;

namespace FeatureFlags.Scenarios;

/// <summary>
/// EnableNewCheckoutScenario
/// 
/// Demonstrates enabling the "NewCheckout" feature flag using <see cref="IMutation{TState}"/> and <see cref="IMutationEngine.ExecuteAsync{TState}"/>.
/// 
/// This scenario covers:
/// 
/// - Execution of a single feature flag mutation via <see cref="IMutationEngine"/>
/// - Policy evaluation through <see cref="FeatureFlagsState"/>
/// - Use of <see cref="EnableFeatureMutation"/> to provide user metadata and correlation ID
/// - Handling both successful and blocked mutations
/// - Updating and inspecting the <see cref="MutationContext"/>
/// 
/// Key Steps:
/// 1. Initialize <see cref="IMutationEngine.ExecuteAsync{TState}"/> with "LegacyCheckout" enabled and "NewCheckout" disabled.
/// 2. Construct an <see cref="IMutationEngine"/> with a <see cref="MutationResult{TState}.IsSuccess"/> containing
///    correlation ID and user information.
/// 3. Execute the mutation using <see cref="MutationResult{TState}"/>.
/// 4. Check <see cref="PolicyDecision"/> to determine whether the mutation was applied.
/// 5. Log any policy decisions if the mutation is blocked.
/// 6. Output the current state of all feature flags.
///
/// Example Use Case:
/// - Activating a new checkout flow for testing or gradual rollout
/// - Ensuring auditability via correlation IDs and user metadata
/// - Enforcing policies such as time-based or approval-based restrictions
/// 
/// Notes:
/// - The mutation may be denied if policies like "BusinessHoursPolicy" or "RequireTwoManApprovalPolicy" are active.
/// - Context metadata can include approvers, reasons, or other relevant audit information.
/// </summary>
internal static class EnableNewCheckoutScenario
{
    internal static async Task Run(IMutationEngine engine)
    {
        Console.WriteLine("\n=== Enable NewCheckout Scenario ===");

        var state = new FeatureFlagsState
        {
            Flags = new Dictionary<string, bool>
            {
                ["LegacyCheckout"] = true,
                ["NewCheckout"] = false
            }
        };

        var ctx = MutationContext.User("alice", "Alice", "Enable new checkout") 
            with { CorrelationId = "EnableNewCheckout" };

        var mutation = new EnableFeatureMutation("NewCheckout", ctx);

        var result = await engine.ExecuteAsync(mutation, state);

        if (result.IsSuccess)
        {
            Console.WriteLine("✓ NewCheckout enabled!");
            state = result.NewState!;
        }
        else
        {
            Console.WriteLine("✗ Mutation blocked:");
            foreach (var dec in result.PolicyDecisions)
                Console.WriteLine($"  {dec.PolicyName} – {dec.Reason}");
        }

        Console.WriteLine($"Current state: {string.Join(", ", state.Flags.Select(kv => $"{kv.Key}={kv.Value}"))}");
    }
}