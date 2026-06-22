# ADR-024: Governance Runtime Pending Request Handling

## Tag
#adr_024

## Status
Accepted

## Date
2026-06-22

## Scope
ModularityKit.Mutator.Governance.Runtime

## Context

The governance package already defines:

- `MutationRequest`
- `MutationRequestStatus`
- `PendingMutationReason`
- `MutationRequestDecision`
- `IMutationRequestStore`

That gives the package a request model, but not yet a runtime lifecycle.

To become operational, governance must handle requests that stay pending over time instead of treating them as static records. This includes:

- entering pending state
- canceling pending requests
- expiring pending requests
- superseding older requests
- listing and resolving pending requests

Without this runtime layer, governance remains only a set of abstractions and cannot drive actual request lifecycle flows.

## Decision

The governance package should introduce first-class runtime handling for pending mutation requests.

Expected direction:

- a governance runtime service should own pending request transitions
- pending transitions must be recorded through `MutationRequestDecision`
- expiration and cancellation must be explicit runtime actions
- pending requests must remain queryable by status and pending reason
- the runtime must not collapse pending state into ordinary mutation failure semantics

The first implementation uses:

- `IMutationRequestLifecycleManager` as the runtime transition contract
- `MutationRequestLifecycleManager` as the default runtime implementation
- explicit `MutationRequestDecision` entries for pending, approval, rejection, cancellation, expiration, superseding, and execution transitions
- `IMutationRequestStore.GetPendingByStateIdAsync(...)` for listing pending requests by state and reason

## Design Rationale

- The model already distinguishes request lifecycle from direct execution.
- Pending execution is central to governance and should be handled explicitly.
- Runtime transitions should remain inside the governance package instead of leaking into the core mutation engine.

## Consequences

### Positive

- Governance can evolve from static request records into a real deferred execution runtime.
- Future approval, scheduling, and external check flows can share one pending lifecycle engine.

### Negative

- This introduces a new runtime layer with state transition rules that need consistent enforcement.
- The first implementation must define carefully which transitions are allowed from each status.

## Related ADRs

- ADR-020: Governance MutationRequest Model
- ADR-021: Governance Pending Mutation Lifecycle
- ADR-022: Governance Request Decisions and Storage
