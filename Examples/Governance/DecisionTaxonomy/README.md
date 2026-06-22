# Governance DecisionTaxonomy

This example shows why governance request decisions are split into separate categories instead of being kept in one flat enum.

It demonstrates:

- lifecycle decisions such as `Submitted` and `Approved`
- approval decisions such as `Requested`, `Granted`, and `Rejected`
- version-resolution decisions such as `Validated` and `RejectedAsStale`
- the shared `MutationRequestDecisionType` wrapper with:
  - `Category`
  - `Code`
  - `ToString()`

## Key files

- [`Program.cs`](Program.cs)
- [`Scenarios/GovernanceDecisionTaxonomyScenario.cs`](Scenarios/GovernanceDecisionTaxonomyScenario.cs)
- [`src/Governance/Abstractions/Requests/Decisions/MutationRequestDecisionType.cs`](../../../src/Governance/Abstractions/Requests/Decisions/MutationRequestDecisionType.cs)
- [`src/Governance/Abstractions/Requests/Decisions/MutationRequestDecisionCategory.cs`](../../../src/Governance/Abstractions/Requests/Decisions/MutationRequestDecisionCategory.cs)
- [`src/Governance/Abstractions/Requests/Decisions/MutationRequestLifecycleDecisionType.cs`](../../../src/Governance/Abstractions/Requests/Decisions/MutationRequestLifecycleDecisionType.cs)
- [`src/Governance/Abstractions/Requests/Decisions/MutationRequestApprovalDecisionType.cs`](../../../src/Governance/Abstractions/Requests/Decisions/MutationRequestApprovalDecisionType.cs)
- [`src/Governance/Abstractions/Requests/Decisions/MutationRequestVersionResolutionDecisionType.cs`](../../../src/Governance/Abstractions/Requests/Decisions/MutationRequestVersionResolutionDecisionType.cs)

## Run

```bash
dotnet run --project Examples/Governance/DecisionTaxonomy/DecisionTaxonomy.csproj
```
