using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using ModularityKit.Mutator.Abstractions;
using ModularityKit.Mutator.Abstractions.Changes;
using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Abstractions.Engine;
using ModularityKit.Mutator.Abstractions.Intent;
using ModularityKit.Mutator.Abstractions.Policies;
using ModularityKit.Mutator.Abstractions.Results;
using ModularityKit.Mutator.Runtime;

namespace ModularityKit.Mutator.Benchmarks;

[MemoryDiagnoser]
[InProcess]
public class MutationEngineBenchmarks
{
    private const string StateId = "benchmark-counter";

    private IMutationEngine _performanceEngine = null!;
    private IMutationEngine _strictEngine = null!;
    private CounterState _state = null!;
    private IncrementCounterMutation _commitMutation = null!;
    private IncrementCounterMutation _simulateMutation = null!;
    private IncrementCounterMutation _validateMutation = null!;
    private IReadOnlyList<IMutation<CounterState>> _batchMutations = null!;

    [GlobalSetup]
    public void Setup()
    {
        _performanceEngine = BuildEngine(MutationEngineOptions.Performance, addAllowPolicy: false);
        _strictEngine = BuildEngine(MutationEngineOptions.Strict, addAllowPolicy: true);

        _state = new CounterState(42);
        _commitMutation = CreateMutation(MutationMode.Commit, "commit-one");
        _simulateMutation = CreateMutation(MutationMode.Simulate, "simulate-one");
        _validateMutation = CreateMutation(MutationMode.Validate, "validate-one");
        _batchMutations = Enumerable.Range(0, 10)
            .Select(i => CreateMutation(MutationMode.Commit, $"batch-{i}"))
            .ToArray();
    }

    [Benchmark(Baseline = true)]
    public async Task Commit_Performance_NoPolicy()
    {
        var result = await _performanceEngine.ExecuteAsync(_commitMutation, _state);
        GC.KeepAlive(result);
    }

    [Benchmark]
    public async Task Commit_Strict_WithPolicy()
    {
        var result = await _strictEngine.ExecuteAsync(_commitMutation, _state);
        GC.KeepAlive(result);
    }

    [Benchmark]
    public async Task Simulate_Strict_WithPolicy()
    {
        var result = await _strictEngine.ExecuteAsync(_simulateMutation, _state);
        GC.KeepAlive(result);
    }

    [Benchmark]
    public async Task ValidateOnly_Strict_WithPolicy()
    {
        var result = await _strictEngine.ExecuteAsync(_validateMutation, _state);
        GC.KeepAlive(result);
    }

    [Benchmark]
    public async Task Batch_Commit_Performance_NoPolicy()
    {
        var result = await _performanceEngine.ExecuteBatchAsync(_batchMutations, _state);
        GC.KeepAlive(result);
    }

    private static IMutationEngine BuildEngine(
        MutationEngineOptions options,
        bool addAllowPolicy)
    {
        var services = new ServiceCollection();
        services.AddMutators(options);
        var provider = services.BuildServiceProvider();
        var engine = provider.GetRequiredService<IMutationEngine>();

        if (addAllowPolicy)
            engine.RegisterPolicy(new AllowAllCounterPolicy());

        return engine;
    }

    private static IncrementCounterMutation CreateMutation(MutationMode mode, string operationSuffix)
    {
        var context = MutationContext.System("benchmark")
            with
            {
                StateId = StateId,
                Mode = mode,
                CorrelationId = $"{StateId}:{operationSuffix}"
            };

        return new IncrementCounterMutation(context);
    }

    private sealed record CounterState(int Value);

    private sealed class IncrementCounterMutation(MutationContext context) : IMutation<CounterState>
    {
        public MutationIntent Intent { get; } = new()
        {
            OperationName = "IncrementCounter",
            Category = "Benchmark",
            Description = "Increment the benchmark counter by one",
            RiskLevel = MutationRiskLevel.Low,
            IsReversible = true
        };

        public MutationContext Context { get; } = context;

        public MutationResult<CounterState> Apply(CounterState state)
        {
            var next = state with { Value = state.Value + 1 };

            return MutationResult<CounterState>.Success(
                next,
                ChangeSet.Single(StateChange.Modified(nameof(CounterState.Value), state.Value, next.Value)));
        }

        public ValidationResult Validate(CounterState state)
        {
            var result = ValidationResult.Success();

            if (state.Value < 0)
                result.AddError(nameof(CounterState.Value), "Counter value must be non-negative.");

            return result;
        }

        public MutationResult<CounterState> Simulate(CounterState state)
        {
            var next = state with { Value = state.Value + 1 };

            return MutationResult<CounterState>.Success(
                next,
                ChangeSet.Single(StateChange.Modified(nameof(CounterState.Value), state.Value, next.Value)));
        }
    }

    private sealed class AllowAllCounterPolicy : IMutationPolicy<CounterState>
    {
        public string Name => nameof(AllowAllCounterPolicy);

        public int Priority => 0;

        public string? Description => "Always allows the benchmark counter mutation.";

        public PolicyDecision Evaluate(IMutation<CounterState> mutation, CounterState state)
            => PolicyDecision.Allow(Name, "Benchmark policy allows all mutations.");
    }
}
