# Examples

This folder contains runnable console apps that show how to use `ModularityKit.Mutator` in concrete domain flows.

The examples are intentionally small and focused. Each project demonstrates a different style of mutation workflow, policy enforcement, and engine usage without requiring you to read the whole library first.

## What is here

| Example | Focus | Readme |
| --- | --- | --- |
| `BillingQuotas` | quota changes, validation, and policy limits | [`Examples/BillingQuotas/README.md`](BillingQuotas/README.md) |
| `FeatureFlags` | feature toggles, audit/history, and batch execution | [`Examples/FeatureFlags/README.md`](FeatureFlags/README.md) |
| `IamRoles` | role changes, approval rules, and batch migration | [`Examples/IamRoles/README.md`](IamRoles/README.md) |
| `WorkflowApprovals` | ordered workflow state transitions and approvals | [`Examples/WorkflowApprovals/README.md`](WorkflowApprovals/README.md) |

## How to use these examples

Start with the project-specific README for the domain you care about. Each one explains:

- the domain model used in the sample
- the mutation types and policies involved
- the main scenarios the app runs
- the key files to inspect first

Then open `Program.cs` in that example. That is the entry point that wires up the engine, registers policies, and runs the scenarios.

## Build

Build the full solution from the repository root:

```bash
dotnet build ModularityKit.Mutator.slnx -c Release
```

You can also build just one example:

```bash
dotnet build Examples/BillingQuotas/BillingQuotas.csproj -c Release
dotnet build Examples/FeatureFlags/FeatureFlags.csproj -c Release
dotnet build Examples/IamRoles/IamRoles.csproj -c Release
dotnet build Examples/WorkflowApprovals/WorkflowApprovals.csproj -c Release
```

## Run

Each example is a separate console app.

From the repository root:

```bash
dotnet run --project Examples/BillingQuotas/BillingQuotas.csproj
dotnet run --project Examples/FeatureFlags/FeatureFlags.csproj
dotnet run --project Examples/IamRoles/IamRoles.csproj
dotnet run --project Examples/WorkflowApprovals/WorkflowApprovals.csproj
```

If you want to run one sample repeatedly while changing code, stay in its folder:

```bash
cd Examples/BillingQuotas
dotnet run
```

## Suggested reading order

1. Open the project README for the example you care about.
2. Read `Program.cs` to see how the engine is composed.
3. Inspect the `State/` types to understand the domain model.
4. Read the `Mutations/` files to see how changes are applied.
5. Read the `Policies/` files to see what is allowed or blocked.
6. Read the `Scenarios/` files to see how everything is exercised together.

## Common structure

Most examples follow the same layout:

- `Program.cs` - console entry point and composition root
- `State/` - immutable or mostly immutable domain state
- `Mutations/` - mutation implementations
- `Policies/` - business rules and safeguards
- `Scenarios/` - runnable flows that use the engine
- `README.md` - example-specific documentation

That layout is deliberate. It makes each sample easy to scan and keeps the mutation engine usage visible instead of hidden behind helper abstractions.

## Example summaries

### BillingQuotas

Shows quota management workflows such as increasing quotas, applying limits, and resetting values on schedule. It is the simplest place to look if you want a compact example of validation plus policy enforcement.

See [`BillingQuotas/README.md`](BillingQuotas/README.md).

### FeatureFlags

Shows feature toggle workflows with policy checks and history logging. This example is useful if you want to see how the engine behaves when you care about auditability and batch changes.

See [`FeatureFlags/README.md`](FeatureFlags/README.md).

### IamRoles

Shows role grant and revoke workflows with approval-style rules. This is the example to read if you care about protection against unsafe privilege changes.

See [`IamRoles/README.md`](IamRoles/README.md).

### WorkflowApprovals

Shows a multi-step approval process with ordered execution and rejection handling. This example is the best fit if you want to study state transitions that must happen in a strict sequence.

See [`WorkflowApprovals/README.md`](WorkflowApprovals/README.md).

## Notes

- The examples are separate console applications, not libraries.
- They all reference the same mutation engine project under `src/`.
- The sample code is meant to be readable and runnable, not minimal for its own sake.
- Benchmarking lives in [`Benchmarks/`](../Benchmarks/README.md) and is separate from the examples.
- Tests, when added, should live in [`Tests/`](../Tests/).

## Related documentation

- Root project overview: [`README.md`](../README.md)
- Mutation engine source: [`src/`](../src/)
- Benchmark project: [`Benchmarks/README.md`](../Benchmarks/README.md)
