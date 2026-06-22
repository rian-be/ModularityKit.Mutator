# ADR-023: Governance Versioned Request Resolution

## Tag
#adr_023

## Status
Accepted

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

The governance package adopts explicit version-aware request resolution semantics.

- `MutationRequest` keeps an `ExpectedStateVersion`
- request resolution compares current state version with expected version
- stale requests do not silently execute
- governance runtime resolves stale requests through one of three explicit strategies:
  - `RejectStale`
  - `RequireRenewedApproval`
  - `RevalidateOnLatestState`

Current runtime contract:

- matching version, or no expected version:
  - request receives `VersionValidated`
  - outcome is `ExecuteApprovedVersion`
- stale request with `RejectStale`:
  - request becomes `Rejected`
  - request receives `RejectedAsStale`
- stale request with `RequireRenewedApproval`:
  - request returns to `Pending`
  - `PendingReason` becomes `Approval`
  - `ExpectedStateVersion` is updated to the current version
  - request receives `RenewedApprovalRequired`
- stale request with `RevalidateOnLatestState`:
  - request stays `Approved`
  - `ExpectedStateVersion` is updated to the current version
  - request receives `RevalidationRequired`

## Design Rationale

- Approval and deferred execution are not safe without explicit state version semantics.
- Silent execution on drifted state would weaken governance guarantees.
- The model already contains the right seam through `ExpectedStateVersion`.

## Consequences

### Positive

- Governance runtime now has explicit semantics for stale approvals.
- Deferred execution becomes safer and more auditable.
- Request decision history reflects stale detection and final resolution path.

### Negative

- This introduces additional policy and runtime complexity.
- Different domains may want different stale resolution strategies.
- Revalidation itself is still a separate runtime step beyond this version-resolution contract.

## Related ADRs

- ADR-020: Governance MutationRequest Model
- ADR-021: Governance Pending Mutation Lifecycle
- ADR-022: Governance Request Decisions and Storage
