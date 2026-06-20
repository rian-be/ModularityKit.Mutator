using ModularityKit.Mutator.Abstractions.Effects;
using ModularityKit.Mutator.Abstractions.Results;

namespace ModularityKit.Mutator.Runtime.Internal;

internal static class PolicyModificationApplier
{
    public static MutationResult<TState> Apply<TState>(
        MutationResult<TState> result,
        IReadOnlyDictionary<string, object>? modifications)
    {
        if (modifications is null || modifications.Count == 0 || !result.IsSuccess)
            return result;

        var newState = result.NewState;
        var sideEffects = result.SideEffects.ToList();

        foreach (var modification in modifications)
        {
            switch (modification.Key)
            {
                case "State" when modification.Value is TState stateValue:
                    newState = stateValue;
                    break;
                case "SideEffect" when modification.Value is SideEffect effect:
                    sideEffects.Add(effect);
                    break;
                case "SideEffects" when modification.Value is IEnumerable<SideEffect> effects:
                    sideEffects.AddRange(effects);
                    break;
            }
        }

        return new MutationResult<TState>
        {
            IsSuccess = result.IsSuccess,
            NewState = newState,
            Changes = result.Changes,
            ValidationResult = result.ValidationResult,
            PolicyDecisions = result.PolicyDecisions,
            SideEffects = sideEffects,
            Metrics = result.Metrics,
            Exception = result.Exception,
            CompletedAt = result.CompletedAt
        };
    }
}
