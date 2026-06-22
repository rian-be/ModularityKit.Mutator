# Governance VersionedResolution

This example shows how `MutationRequestVersionResolver` handles requests that were approved against an older state version.

It is the direct runnable example for the semantics introduced around `ExpectedStateVersion` and stale request handling.

## What it demonstrates

- resolving a request when the current state version still matches
- resolving stale requests with `RejectStale`
- resolving stale requests with `RequireRenewedApproval`
- resolving stale requests with `RevalidateOnLatestState`
- inspecting the resulting lifecycle state and appended decision history

## Key files

- [`Program.cs`](Program.cs)
- [`Scenarios/GovernanceVersionedResolutionScenario.cs`](Scenarios/GovernanceVersionedResolutionScenario.cs)
- [`src/Governance/Runtime/MutationRequestVersionResolver.cs`](../../../src/Governance/Runtime/MutationRequestVersionResolver.cs)
- [`src/Governance/Abstractions/Resolution/VersionedRequestResolutionStrategy.cs`](../../../src/Governance/Abstractions/Resolution/VersionedRequestResolutionStrategy.cs)
- [`src/Governance/Abstractions/Resolution/MutationRequestVersionResolution.cs`](../../../src/Governance/Abstractions/Resolution/MutationRequestVersionResolution.cs)

## Run

```bash
dotnet run --project Examples/Governance/VersionedResolution/VersionedResolution.csproj
```

## Expected output

The sample prints one block per resolution strategy and shows:

- selected outcome
- whether the request was stale
- resulting request status
- updated expected version
- last decision recorded during resolution
