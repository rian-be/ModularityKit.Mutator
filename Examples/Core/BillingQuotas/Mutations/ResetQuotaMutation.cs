using BillingQuotas.State;
using ModularityKit.Mutator.Abstractions.Changes;
using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Abstractions.Engine;
using ModularityKit.Mutator.Abstractions.Intent;
using ModularityKit.Mutator.Abstractions.Results;
using ValidationResult = ModularityKit.Mutator.Abstractions.Results.ValidationResult;

namespace BillingQuotas.Mutations;

/// <summary>
/// Mutation that resets user's quota to zero.
/// </summary>
internal sealed record ResetQuotaMutation(
    string UserId,
    MutationContext Context
) : IMutation<QuotaState>
{
    public MutationIntent Intent { get; } = new()
    {
        OperationName = "ResetQuota",
        Category = "Billing",
        RiskLevel = MutationRiskLevel.High,
        Description = "Reset user quota to zero"
    };

    public ValidationResult Validate(QuotaState state)
    {
        var result = new ValidationResult();

        if (string.IsNullOrEmpty(UserId))
            result.AddError("UserId", "UserId cannot be empty");

        return result;
    }

    public MutationResult<QuotaState> Apply(QuotaState state)
    {
        var quotas = state.UserQuotas.ToDictionary(kv => kv.Key, kv => kv.Value);
        quotas[UserId] = 0;

        var newState = state with { UserQuotas = quotas };

        var changes = ChangeSet.Single(
            StateChange.Modified($"UserQuotas.{UserId}", null, 0)
        );

        return MutationResult<QuotaState>.Success(newState, changes);
    }

    public MutationResult<QuotaState> Simulate(QuotaState state) => Apply(state);
}