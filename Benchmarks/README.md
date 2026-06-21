# Benchmarks

This folder contains BenchmarkDotNet measurements for `ModularityKit.Mutator`.

## What is benchmarked

- commit execution without policy pressure
- strict engine execution with policy checks
- simulate and validate only paths
- batch execution overhead

## Run

Build first:

```bash
dotnet build Benchmarks/ModularityKit.Mutator.Benchmarks.csproj -c Release
```

Run a specific benchmark:

```bash
dotnet Benchmarks/bin/Release/net10.0/ModularityKit.Mutator.Benchmarks.dll --filter '*MutationEngineBenchmarks.Commit_Performance_NoPolicy*'
```

## Notes

- The benchmark harness is configured for the current environment.
- Results are emitted by BenchmarkDotNet when the runner can write artifacts.
