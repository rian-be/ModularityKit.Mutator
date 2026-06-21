# ADR-020: Governance MutationRequest Model

## Tag
#adr_020

## Status
Accepted

## Date
2026-06-21

## Scope
ModularityKit.Mutator.Governance.Abstractions

## Context

The governance layer needs primary unit that can represent more than direct mutation execution.

Core runtime mutations are immediate execution objects. Governance needs model that can survive over time and carry:

- request identity
- state targeting
- mutation classification
- request context
- pending requirements
- request decisions
- version expectations

Without such a model, deferred execution would have to be represented through ad hoc flags on mutation results, which is too weak for approval, expiration, cancellation, and re-resolution scenarios.

## Decision

- Introduce `MutationRequest` as the primary governance aggregate.
- `MutationRequest` carries:
  - `RequestId`
  - `StateId`
  - `StateType`
  - `MutationType`
  - `Intent`
  - `Context`
  - `Status`
  - `PendingReason`
  - `Requirements`
  - `Decisions`
  - `ExpectedStateVersion`
  - `ExpiresAt`
  - `CreatedAt`
  - `UpdatedAt`
  - request-level `Metadata`
- Provide factory methods for:
  - `Pending(...)`
  - `Approved(...)`

## Design Rationale

- Gives governance durable model that is distinct from direct mutation execution.
- Keeps request lifecycle concerns out of the core `MutationResult`.
- Preserves enough context to support future approval, persistence, and re-execution flows.
- Makes version-aware deferred execution possible without leaking governance concerns into the core package.

## Consequences

### Positive

- Governance now has clear primary aggregate.
- Future approval and pending lifecycle features have stable model to build on.
- Request level persistence and query APIs have a natural root entity.

### Negative

- Some mutation data is now represented in both core and governance layers, with different purposes.
- Governance logic will need discipline to avoid turning `MutationRequest` into an unbounded bag of state.

## Related ADRs

- ADR-019: Governance Package Separation
