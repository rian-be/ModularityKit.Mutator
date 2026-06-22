# BillingQuotas

This example shows how to model quota changes as mutations, keep the state immutable, and enforce policy limits before change is applied.

It is the most compact sample in the repository. Use it when you want to see the engine doing one thing well: controlled numeric state changes with validation and policy checks.

## Domain

The domain state is map of user IDs to quota values:

- each user has an integer quota
- quota changes are applied through mutations
- policies prevent quota values from moving outside allowed bounds

The example runs two scenarios:

- an emergency increase for selected users
- a scheduled monthly reset

## What this example demonstrates

- registering the mutation engine with strict options
- executing both single mutations and batch mutations
- validating mutation input before state changes occur
- enforcing maximum and minimum quota limits through policies
- collecting engine statistics after the scenarios finish

## Project structure

- [`Program.cs`](Program.cs) - composition root and scenario runner
- [`BillingQuotas.csproj`](BillingQuotas.csproj) - console project definition
- [`State/QuotaState.cs`](State/QuotaState.cs) - immutable quota state
- [`Mutations/IncreaseQuotaMutation.cs`](Mutations/IncreaseQuotaMutation.cs)
- [`Mutations/DecreaseQuotaMutation.cs`](Mutations/DecreaseQuotaMutation.cs)
- [`Mutations/ResetQuotaMutation.cs`](Mutations/ResetQuotaMutation.cs)
- [`Policies/MaxQuotaPolicy.cs`](Policies/MaxQuotaPolicy.cs)
- [`Policies/PreventNegativeQuotaPolicy.cs`](Policies/PreventNegativeQuotaPolicy.cs)
- [`Scenarios/EmergencyIncreaseScenario.cs`](Scenarios/EmergencyIncreaseScenario.cs)
- [`Scenarios/MonthlyResetScenario.cs`](Scenarios/MonthlyResetScenario.cs)

## How it works

`Program.cs` wires the engine like this:

1. create `ServiceCollection`
2. register `ModularityKit.Mutator` with `MutationEngineOptions.Strict`
3. build the service provider
4. resolve `IMutationEngine`
5. register quota policies
6. run the two scenarios
7. print statistics from `GetStatisticsAsync()`

The scenarios then create their own initial state, build mutations, and call the engine.

## Mutation flow

### Increase quota

[`IncreaseQuotaMutation`](Mutations/IncreaseQuotaMutation.cs) increments a user's quota.

- validates `UserId`
- validates that `Amount` is positive
- applies a new quota value
- emits `ChangeSet` entry for the modified user quota

### Decrease quota

[`DecreaseQuotaMutation`](Mutations/DecreaseQuotaMutation.cs) decrements a user's quota.

- validates `UserId`
- validates that `Amount` is positive
- rejects changes that would drop quota below zero
- emits `ChangeSet` entry for the modified quota

### Reset quota

[`ResetQuotaMutation`](Mutations/ResetQuotaMutation.cs) sets the user's quota back to zero.

- validates `UserId`
- applies zero value
- emits change entry for the affected user

## Policies

### Max quota

[`MaxQuotaPolicy`](Policies/MaxQuotaPolicy.cs) blocks increases that would exceed the configured maximum.

This policy is useful for:

- capping emergency overrides
- constraining tenant limits
- showing how policy evaluation can inspect the concrete mutation type

### Prevent negative quota

[`PreventNegativeQuotaPolicy`](Policies/PreventNegativeQuotaPolicy.cs) blocks quota decreases that would go below zero.

This policy is useful for:

- ensuring accounting integrity
- protecting against accidental underflow
- demonstrating simple deny decisions

## Scenarios

### Emergency increase

[`EmergencyIncreaseScenario`](Scenarios/EmergencyIncreaseScenario.cs) starts from state with multiple users and applies batch of increases.

It shows:

- batch execution
- per mutation policy decisions
- mixed success outcomes inside one batch

### Monthly reset

[`MonthlyResetScenario`](Scenarios/MonthlyResetScenario.cs) resets every user's quota in one batch.

It shows:

- system level mutation context
- batch generation from current state
- folding successful results back into final state

## What to read first

1. [`Program.cs`](Program.cs)
2. [`State/QuotaState.cs`](State/QuotaState.cs)
3. [`Mutations/IncreaseQuotaMutation.cs`](Mutations/IncreaseQuotaMutation.cs)
4. [`Mutations/DecreaseQuotaMutation.cs`](Mutations/DecreaseQuotaMutation.cs)
5. [`Policies/MaxQuotaPolicy.cs`](Policies/MaxQuotaPolicy.cs)
6. [`Policies/PreventNegativeQuotaPolicy.cs`](Policies/PreventNegativeQuotaPolicy.cs)
7. [`Scenarios/EmergencyIncreaseScenario.cs`](Scenarios/EmergencyIncreaseScenario.cs)
8. [`Scenarios/MonthlyResetScenario.cs`](Scenarios/MonthlyResetScenario.cs)

## Run

```bash
dotnet run --project Examples/Core/BillingQuotas/BillingQuotas.csproj
```

## Expected output

The sample prints:

- the emergency increase scenario
- the monthly reset scenario
- success or policy denial messages
- final quota values
- aggregate engine statistics

The exact numbers depend on the runtime and any policy thresholds you change.
