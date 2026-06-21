# Roadmap

This document captures the most valuable next steps for `ModularityKit.Mutator` based on the current state of the API, runtime, and examples.

The goal is not to add features for their own sake. The goal is to close gaps between the public model and the runtime behavior, then extend the engine into a more complete governance platform for state mutations.

## Principles

- Prefer features that complete existing abstractions over adding parallel APIs.
- Prioritize runtime behavior that the public API already implies.
- Keep new capabilities observable through audit, history, and examples.
- Add focused tests alongside each roadmap item before broadening the surface further.

## Current Gaps

These areas already exist in the model but are only partially realized in the runtime:

- `PolicyRequirement` exists, but there is no approval lifecycle around it.
- `MutationIntent.IsReversible` exists, but there is no undo or compensation mechanism.
- `MutationEngineOptions.MaxConcurrentMutations` exists, but concurrency control is not enforced in runtime execution.
- `MutationIntent.EstimatedBlastRadius`, `Tags`, and `Metadata` exist, but the engine does not yet use them for governance or query workflows.
- The project has examples and benchmarks, but no dedicated test project yet.

## v1.1 Governance Runtime

Focus: establish a first-class governance runtime centered around mutation requests, pending execution, and version-aware decision making.

### 1. Pending Mutation Lifecycle

Add a first-class lifecycle around deferred mutation requests.

Scope:

- Introduce a `PendingMutation` or `MutationRequest` model for governed execution.
- Support pending reasons beyond approval, such as external checks, scheduling, or dependencies.
- Support approval expiration and explicit cancellation.
- Persist approval decisions and approval history.
- Define re-execution rules when a pending mutation is resolved against a newer state version.
- Support listing and resolving pending mutation records.

Why this matters:

- Approval workflow is only one specialization of pending execution.
- A single `PendingApproval` status in `MutationResult` is not enough once the engine owns a real governance process.

### 2. Approval Workflow for `PolicyRequirement`

Add first-class support for approval-based policy outcomes.

Scope:

- Introduce a pending approval result state for blocked mutations that require action rather than hard denial.
- Persist approval requirements to audit and history.
- Add follow-up mutations such as `ApproveRequirement` and `RejectRequirement`.
- Support multi-step approvals such as two-man approval and role-based approval chains.

Why this matters:

- The policy model already supports `RequireApproval`.
- This turns the engine from allow/deny enforcement into an actual governance workflow engine.

### 3. Versioned Execution and Concurrency Control

Add explicit optimistic concurrency handling around mutation execution.

Scope:

- Introduce version-aware mutation execution based on `stateId` and expected version.
- Use `ConcurrencyException` as a real runtime outcome instead of a dormant abstraction.
- Add optional lock-per-state execution for high-contention scenarios.
- Define how batch execution behaves when a mid-batch concurrency conflict occurs.

Why this matters:

- The library claims deterministic and async-safe behavior.
- Without explicit concurrency semantics, that promise is incomplete for shared state workloads.

## v1.2 Governance Data

Focus: make governed execution queryable, persistent, and classification-aware.

### 1. Persistent History and Audit Stores

Add production-ready adapters beyond in-memory implementations.

Scope:

- Entity Framework Core store for audit and history.
- PostgreSQL-oriented store or provider package.
- Optional Redis-backed recent-history cache for hot paths.

Why this matters:

- In-memory implementations are suitable for examples, tests, and development only.
- Production integration is the next natural adoption step.

### 2. Query API for Audit and History

Expose richer retrieval primitives than state-id-only history lookup.

Scope:

- Query by `actorId`, `category`, `riskLevel`, `sideEffectSeverity`, and time range.
- Query by `tags`, governance metadata, and estimated blast radius.
- Support recent activity queries and filtered timelines.
- Add approval-oriented queries such as pending approvals and recent approval decisions.
- Support risk-oriented filtering and reporting views.
- Define storage-agnostic query contracts first, then implement provider-specific adapters.

Why this matters:

- Audit and history become much more useful once users can answer operational questions, not just replay a single state stream.
- Persistence without queryability is only a storage layer, not an operational feature.

### 3. Governance Metadata

Turn intent classification fields into runtime-usable governance data.

Scope:

- Persist `Tags`, `Metadata`, and `EstimatedBlastRadius` into audit and history records.
- Expose them through query APIs.
- Enable policy decisions and reporting based on governance metadata.

Why this matters:

- These fields already exist in the public model.
- The roadmap should explicitly close the gap instead of only calling it out in `Current Gaps`.

### 4. Typed Side Effects

Evolve side effects beyond `object? Data`.

Scope:

- Add a typed side effect variant such as `SideEffect<TData>`, or
- Add serialization and registration contracts for side effect payloads.

Why this matters:

- Persistence and queryability make side effect payload contracts much more important.
- This is the point where side effects stop being just runtime output and become integration data.

## v1.3 Integration

Focus: connect governance runtime behaviors to external systems and asynchronous policy sources.

### 1. Async Policies

Support policy evaluation that depends on external systems.

Scope:

- Introduce `EvaluateAsync(...)` policy support.
- Preserve a sync path for lightweight policies.
- Define ordering and timeout semantics when multiple async policies are involved.
- Allow approval and governance checks to rely on external identity, ticketing, or compliance systems.

Why this matters:

- Real approval and compliance checks often depend on external identity, ticketing, quota, or feature control systems.

## v2.0 Governance Platform

Focus: make the engine a complete platform for governed mutations.

### 1. Undo and Compensation

Add explicit reversal and compensation support for reversible mutations.

Scope:

- Introduce a reversible mutation contract or compensation contract.
- Record links between original execution and compensating execution.
- Support compensation plans for failed batches and operator-driven rollback flows.

Why this matters:

- `MutationIntent.IsReversible` already exists.
- This is one of the biggest jumps in real operational value for change governance.

### 2. Governance-Aware Policy Composition

Add composition primitives for complex policy sets.

Scope:

- `AllOf`, `AnyOf`, and priority-based composition.
- Merging rules for severity, requirements, side effects, and metadata.
- Clear conflict rules when multiple policies modify the same mutation result.

Why this matters:

- Today, policy complexity is pushed into handwritten policy classes.
- Composition will make complex governance easier to express and reuse.

## Cross-Cutting Work

These tasks should accompany every milestone:

- Improve README accuracy where examples still drift from the current package and namespace layout.
- Keep `Examples/` aligned with newly added runtime features.
- Extend `Benchmarks/` when new runtime behavior could affect hot paths.
- Maintain comprehensive test coverage for all runtime behaviors.
- Add regression tests for every bug fix.
- Require tests for all new governance features.
- Add ADRs for every public or behavioral change that alters execution semantics.

## Execution Model

The longer-term lifecycle model behind these milestones is documented in [`Docs/ExecutionModel.md`](ExecutionModel.md).

## Recommended Build Order

1. Add pending mutation lifecycle support.
2. Implement approval workflow support for `PolicyRequirement`.
3. Add versioned execution and concurrency handling to the runtime.
4. Add persistent audit/history providers.
5. Add query APIs over persisted governance data.
6. Persist and expose governance metadata.
7. Add typed side effects once persistence and query contracts are stable.
8. Add async policy support, unless external approval integrations force it earlier.
9. Add undo/compensation support once approval and persistence semantics are stable.

## Not Recommended Yet

These ideas may be useful later, but they are not the best next investment:

- Distributed execution features before concurrency semantics are explicit.
- More examples before the runtime contract around approvals and concurrency is complete.
