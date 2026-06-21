# ADR-022: Governance Request Decisions and Storage

## Tag
#adr_022

## Status
Accepted

## Date
2026-06-21

## Scope
ModularityKit.Mutator.Governance.Abstractions
ModularityKit.Mutator.Governance.Runtime

## Context

Once mutation requests become durable governance objects, they need:

- a history of lifecycle decisions
- a storage contract
- a minimal runtime implementation for local development and tests

Without decisions, the request only shows current state and loses governance history.
Without a store contract, governance cannot grow into persistence or query features.

## Decision

- Introduce `MutationRequestDecision` as the record of lifecycle transition or governance action.
- Introduce `MutationRequestDecisionType` with:
  - `Submitted`
  - `Approved`
  - `Rejected`
  - `Canceled`
  - `Expired`
  - `Superseded`
  - `Executed`
- Introduce `IMutationRequestStore` with operations for:
  - storing request
  - retrieving by request id
  - retrieving by state id
  - retrieving pending requests
- Provide `InMemoryMutationRequestStore` as the first runtime implementation.

## Design Rationale

- Keeps current request state and decision history together at the model level.
- Establishes a storage seam before adding provider specific persistence packages.
- Mirrors the existing core approach of starting with in-memory runtime implementations before introducing durable adapters.

## Consequences

### Positive

- Governance now has durable decision log model.
- The storage contract is in place for future provider packages.
- Examples, tests, and local runtime flows can use `InMemoryMutationRequestStore` immediately.

### Negative

- Decision history and current status can drift if future runtime transitions are not applied carefully.
- Query capabilities are still minimal and intentionally incomplete at this stage.

## Related ADRs

- ADR-019: Governance Package Separation
- ADR-020: Governance MutationRequest Model
- ADR-021: Governance Pending Mutation Lifecycle
