using ModularityKit.Mutator.Abstractions.Changes;
using ModularityKit.Mutator.Abstractions.Engine;
using ModularityKit.Mutator.Abstractions.Results;
using System.Diagnostics;

namespace ModularityKit.Mutator.Runtime.Internal;

internal static class MutationBatchExecutor
{
    public static async Task<BatchMutationResult<TState>> ExecuteAsync<TState>(
        IEnumerable<IMutation<TState>> mutations,
        TState initialState,
        bool stopOnFirstFailure,
        Func<IMutation<TState>, TState, CancellationToken, Task<MutationResult<TState>>> executeAsync,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var results = new List<MutationResult<TState>>();
        var allChanges = new ChangeSet();
        var currentState = initialState;

        foreach (var mutation in mutations)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var result = await executeAsync(mutation, currentState, cancellationToken);
            results.Add(result);

            if (result.IsSuccess)
            {
                currentState = result.NewState!;
                foreach (var change in result.Changes.Changes)
                    allChanges.Add(change);

                continue;
            }

            if (stopOnFirstFailure)
                break;
        }

        stopwatch.Stop();
        var allSucceeded = results.Count > 0 && results.All(r => r.IsSuccess);

        return new BatchMutationResult<TState>
        {
            IsSuccess = allSucceeded,
            FinalState = allSucceeded ? currentState : default,
            Results = results,
            AggregatedChanges = allChanges,
            TotalExecutionTime = stopwatch.Elapsed
        };
    }
}
