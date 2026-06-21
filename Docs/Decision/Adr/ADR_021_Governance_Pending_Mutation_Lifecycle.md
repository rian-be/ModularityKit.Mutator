# ADR-021: Governance Pending Mutation Lifecycle

## Tag
#adr_021

## Status
Accepted

## Date
2026-06-21

## Scope
ModularityKit.Mutator.Governance.Abstractions

## Context

Not every governed mutation should execute immediately.

Governance needs an explicit lifecycle for requests that are:

- waiting for approval
- blocked on external checks
- delayed by scheduling
- waiting on dependencies
- held for quota or manual review

If pending execution is represented only as denial or boolean flag, the runtime cannot express expiration, cancellation, superseding, or eventual execution.

## Decision

- Introduce `MutationRequestStatus` as the lifecycle status enum for governed requests.
- Supported statuses:
  - `Created`
  - `Pending`
  - `Approved`
  - `Rejected`
  - `Canceled`
  - `Expired`
  - `Superseded`
  - `Executed`
- Introduce `PendingMutationReason` to explain why a request is pending.
- Supported pending reasons:
  - `Approval`
  - `ExternalCheck`
  - `Schedule`
  - `Dependency`
  - `Quota`
  - `ManualReview`

## Design Rationale

- Separates lifecycle state from lifecycle cause.
- Makes pending execution first class governance concept instead of terminal failure state.
- Supports future query APIs such as “all pending approvals” or “all expired requests”.
- Allows approval flow to be one specialization of pending execution, rather than the only pending model.

## Consequences

### Positive

- Governance can represent deferred execution explicitly.
- Approval workflows, external checks, and scheduling can share one lifecycle model.
- Future persistence and query layers have stable status dimensions to index.

### Negative

- Lifecycle semantics now need to be enforced consistently in runtime logic that does not exist yet.
- Some statuses, especially `Superseded` and `Executed`, will require careful definition once version-aware execution is implemented.

## Related ADRs

- ADR-019: Governance Package Separation
- ADR-020: Governance MutationRequest Model
