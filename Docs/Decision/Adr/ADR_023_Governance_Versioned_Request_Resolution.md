# ADR-023: Governance Versioned Request Resolution

## Tag
#adr_023

## Status
Proposed

## Date
2026-06-21

## Scope
ModularityKit.Mutator.Governance

## Context

Pending mutation requests may be resolved long after they were created.

Example:

- request created against state version `v10`
- request enters `PendingApproval`
- approval happens after the state has advanced to `v15`

At that point, governance must define what approval means:

- execute against the latest state
- reject as stale
- require re-approval
- attempt re-validation and branch from there

This is a governance concern, not just a core mutation concern, because it only appears once requests can survive beyond immediate execution.

## Decision

The governance package should adopt explicit version-aware request resolution semantics.

Expected direction:

- `MutationRequest` keeps an `ExpectedStateVersion`
- request resolution must compare current state version with expected version
- stale requests must not silently execute without an explicit rule
- runtime resolution should choose among:
  - re-validate and execute against latest state
  - reject as stale
  - require renewed approval

The exact resolution policy is intentionally left open for the first runtime implementation.

## Design Rationale

- Approval and deferred execution are not safe without explicit state version semantics.
- Silent execution on drifted state would weaken governance guarantees.
- The model already contains the right seam through `ExpectedStateVersion`.

## Consequences

### Positive

- Governance runtime will have explicit semantics for stale approvals.
- Deferred execution becomes safer and more auditable.

### Negative

- This introduces additional policy and runtime complexity.
- Different domains may want different stale resolution strategies.

## Related ADRs

- ADR-020: Governance MutationRequest Model
- ADR-021: Governance Pending Mutation Lifecycle
