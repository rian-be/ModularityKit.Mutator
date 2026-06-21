using ModularityKit.Mutator.Abstractions.Audit;
using ModularityKit.Mutator.Abstractions.Changes;
using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Abstractions.Engine;
using ModularityKit.Mutator.Abstractions.Effects;
using ModularityKit.Mutator.Abstractions.History;
using ModularityKit.Mutator.Abstractions.Policies;
using ModularityKit.Mutator.Abstractions.Results;

namespace ModularityKit.Mutator.Runtime.Internal;

internal static class MutationAuditEntryFactory
{
    public static MutationAuditEntry CreateSuccess<TState>(
        IMutation<TState> mutation,
        MutationResult<TState> result,
        PolicyDecision policyDecision,
        string executionId,
        TimeSpan duration)
    {
        return Create(
            mutation,
            executionId,
            duration,
            isSuccess: true,
            changes: result.Changes,
            policyDecisions: result.PolicyDecisions.Count > 0 ? result.PolicyDecisions : [policyDecision],
            sideEffects: result.SideEffects,
            sourceIpAddress: mutation.Context.SourceIpAddress,
            userAgent: mutation.Context.UserAgent);
    }

    public static MutationAuditEntry CreateFailure<TState>(
        IMutation<TState> mutation,
        MutationResult<TState> result,
        string executionId,
        TimeSpan duration)
    {
        return Create(
            mutation,
            executionId,
            duration,
            isSuccess: false,
            changes: result.Changes,
            errorMessage: string.Join("; ", result.ValidationResult.Errors.Select(e => e.Message)),
            policyDecisions: result.PolicyDecisions,
            sideEffects: result.SideEffects);
    }

    public static MutationAuditEntry CreateException<TState>(
        IMutation<TState> mutation,
        Exception exception,
        string executionId,
        TimeSpan duration)
    {
        return Create(
            mutation,
            executionId,
            duration,
            isSuccess: false,
            errorMessage: exception.Message);
    }

    public static MutationHistoryEntry CreateHistoryEntry<TState>(
        IMutation<TState> mutation,
        MutationResult<TState> result,
        string executionId,
        string stateId,
        TimeSpan duration)
    {
        return new MutationHistoryEntry
        {
            ExecutionId = executionId,
            StateId = stateId,
            Intent = mutation.Intent,
            Context = mutation.Context,
            Changes = result.Changes,
            SideEffects = result.SideEffects.ToList(),
            Timestamp = mutation.Context.Timestamp,
            ExecutionTime = duration
        };
    }

    public static string? ResolveStateId(MutationContext context) =>
        context.StateId ?? context.CorrelationId;

    private static MutationAuditEntry Create<TState>(
        IMutation<TState> mutation,
        string executionId,
        TimeSpan duration,
        bool isSuccess,
        ChangeSet? changes = null,
        string? errorMessage = null,
        IReadOnlyList<PolicyDecision>? policyDecisions = null,
        IReadOnlyList<SideEffect>? sideEffects = null,
        string? sourceIpAddress = null,
        string? userAgent = null)
    {
        return new MutationAuditEntry
        {
            ExecutionId = executionId,
            StateId = ResolveStateId(mutation.Context),
            StateType = typeof(TState).Name,
            MutationIntent = mutation.Intent,
            Context = mutation.Context,
            Changes = changes ?? ChangeSet.Empty,
            IsSuccess = isSuccess,
            ErrorMessage = errorMessage,
            PolicyDecisions = policyDecisions ?? [],
            SideEffects = sideEffects ?? [],
            Timestamp = mutation.Context.Timestamp,
            Duration = duration,
            SourceIpAddress = sourceIpAddress,
            UserAgent = userAgent
        };
    }
}
