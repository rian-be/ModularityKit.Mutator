# Execution Model

This document describes the execution model that `ModularityKit.Mutator` is moving toward as governance features become first-class runtime capabilities.

It complements the roadmap by explaining the lifecycle of a mutation once the engine supports pending execution, approvals, versioning, and compensation.

## Current Model

Today, the engine is centered around direct mutation execution:

1. Receive a mutation
2. Evaluate policies
3. Validate
4. Execute or block
5. Audit
6. Persist history

This model is strong for immediate execution flows, but it is intentionally narrow:

- blocked mutations are terminal outcomes
- approval requirements are modeled but not yet lifecycle-driven
- concurrency is not yet part of a first-class execution contract
- compensation and re-execution are not yet modeled as governed transitions

## Target Model

As governance features expand, execution becomes a lifecycle rather than a single pass.

The target shape is:

1. Create a mutation request
2. Evaluate policies and requirements
3. Enter pending state when execution is deferred
4. Resolve requirements through approvals or external checks
5. Re-validate against the current state version
6. Execute or reject
7. Emit side effects
8. Audit and persist history
9. Optionally compensate or reverse

This is the key conceptual shift in the project:

- from direct mutation execution
- to governed mutation request execution

## Core Runtime Concepts

### Mutation Request

Represents a request to execute a mutation under governance.

Expected responsibilities:

- carry the original mutation intent and context
- keep a stable request identifier
- track creation time, current status, and owning state
- record required approvals or checks
- retain the state version or snapshot contract used for re-evaluation

### Pending Mutation

Represents a mutation request that cannot execute immediately.

Possible pending reasons:

- `PendingApproval`
- `PendingExternalCheck`
- `PendingSchedule`
- `PendingDependency`
- `PendingQuota`

Pending state should be treated as a first-class runtime state, not just a flag in `MutationResult`.

### Resolution

A pending mutation must eventually transition through an explicit resolution path.

Possible outcomes:

- approved and executed
- rejected
- canceled
- expired
- superseded by a newer request or state version

### Versioned Execution

Approval and deferred execution require explicit version handling.

When a request is approved against a later state than the one originally evaluated, the runtime must define one of the following behaviors:

- re-execute against the latest state after re-validation
- reject as stale
- require re-approval

This behavior must be explicit and consistent across all pending mutation types.

### Compensation

Once the engine supports governed execution over time, compensation becomes part of the execution model rather than a simple utility.

Compensation should describe:

- what original execution it is linked to
- whether it is automatic or operator-triggered
- whether it restores prior state or applies a forward corrective mutation

## Why This Needs Its Own Document

The roadmap explains priorities and release grouping.

The execution model explains semantics.

That distinction matters because several features are tightly coupled:

- pending mutation lifecycle
- approval workflow
- versioned execution
- concurrency control
- undo and compensation

Without an explicit execution model, these features risk being implemented as isolated additions instead of one coherent runtime contract.

## Design Pressure Points

These are the architectural questions that should stay visible as implementation starts:

- Is the primary unit of governance a mutation or a mutation request?
- What state version contract is required for deferred execution?
- When does a pending mutation become stale?
- Can approvals survive state drift, or must they be renewed?
- Are side effects emitted on request creation, on execution, or both?
- How are compensation flows represented in audit and history?

## Relationship to the Roadmap

- `v1.1 Governance Runtime` introduces pending mutation lifecycle, approval workflow, versioned execution, and concurrency control.
- `v1.2 Governance Data` adds persistence, queryability, metadata handling, and typed side effects around that runtime model.
- `v1.3 Integration` expands the model to async policy evaluation and external governance dependencies.
- `v2.0 Governance Platform` extends the lifecycle with compensation and richer policy composition.
