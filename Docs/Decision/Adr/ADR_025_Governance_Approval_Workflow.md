# ADR-025: Governance Approval Workflow

## Tag
#adr_025

## Status
Accepted

## Date
2026-06-22

## Scope
ModularityKit.Mutator.Governance

## Context

The core mutation model already supports `PolicyRequirement`, and the governance package already introduces pending mutation requests.

What is still missing is the explicit approval flow that connects both concepts:

- a mutation request enters `PendingApproval`
- request-level requirements become visible and durable
- approvers can approve or reject
- decisions become part of request history
- approved requests move toward execution

Without this workflow, approval remains only a modeled intention and not an executable governance capability.

## Decision

The governance package should implement approval as a first-class specialization of pending request lifecycle.

Expected direction:

- `PolicyRequirement` should map into request-level approval requirements
- approval should not be represented only as a policy denial outcome
- approval and rejection must be explicit governance actions
- approval decisions must be recorded through `MutationRequestDecision`
- approved requests must have a defined path toward execution
- rejected requests must transition into a terminal governed status

Multi-step and multi-actor approvals should be supported by the model, even if the first runtime implementation starts with a simpler flow.

## Design Rationale

- Approval is one of the main reasons to introduce governance separately from core runtime.
- The request model already provides the right seam for deferred approval-based execution.
- Approval should build on pending lifecycle rather than creating a parallel flow model.

## Consequences

### Positive

- Governance gains a real approval process instead of a placeholder concept.
- Request history becomes meaningful for approval-driven changes.
- Future integrations with identity, ticketing, or compliance systems have a natural workflow hook.

### Negative

- Approval introduces additional lifecycle complexity and version-drift concerns.
- The runtime must define how approved requests behave when state has changed since submission.

## Related ADRs

- ADR-021: Governance Pending Mutation Lifecycle
- ADR-023: Governance Versioned Request Resolution
- ADR-024: Governance Runtime Pending Request Handling
