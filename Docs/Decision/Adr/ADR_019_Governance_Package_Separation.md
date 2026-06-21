# ADR-019: Governance Package Separation

## Tag
#adr_019

## Status
Accepted

## Date
2026-06-21

## Scope
ModularityKit.Mutator
ModularityKit.Mutator.Governance

## Context

`ModularityKit.Mutator` started as focused mutation runtime:

- mutation execution
- policy evaluation
- audit and history basics
- side effects
- metrics and interception

At the same time, the roadmap and emerging API gaps point toward broader governance model:

- mutation requests
- pending execution lifecycle
- approval workflows
- version aware deferred execution
- governance-specific persistence and query capabilities
- compensation and resolution flows

Those concerns are related to mutation execution, but they are not the same layer.

If they are added directly into the core package, the main risk is that `ModularityKit.Mutator` turns from lightweight mutation runtime into heavy workflow and governance framework. That would make the core package harder to understand, harder to keep small, and harder to adopt for users who only need deterministic mutation execution.

## Decision

- Keep `ModularityKit.Mutator` as the core execution package.
- Introduce `ModularityKit.Mutator.Governance` as separate package in the same repository.
- Place governance specific abstractions and runtime components under the governance package rather than in the core runtime.
- Keep both packages in one repository for now, with separate project files and separate package boundaries.

Current package split:

### Core: `ModularityKit.Mutator`

Responsible for:

- mutation execution
- policy evaluation
- audit and history basics
- side effects
- metrics
- interceptor pipeline

### Extension: `ModularityKit.Mutator.Governance`

Responsible for:

- `MutationRequest`
- pending mutation lifecycle
- request decisions and resolution
- approval-oriented contracts
- governance request storage contracts

## Design Rationale

- Preserves small and execution focused core package.
- Allows governance to evolve at different pace than the core runtime.
- Makes governance an opt in capability instead of default weight for all users.
- Keeps repo level development simple by avoiding an early split into multiple repositories.
- Provides a clean place for future packages such as persistence providers without polluting the core runtime surface.

## Consequences

### Positive

- The mutation engine stays focused on direct execution concerns.
- Governance can grow into a richer model without forcing all consumers to adopt it.
- Future packages such as `EntityFrameworkCore` or `PostgreSql` governance providers have natural home.
- The repository structure now reflects the architectural boundary explicitly.

### Negative

- Some concepts will span package boundaries and require careful API ownership.
- Documentation must explain clearly when a feature belongs to core versus governance.
- Cross-package evolution will need discipline to avoid circular design pressure.

## Follow up

- Keep new governance runtime features out of `ModularityKit.Mutator` unless they are fundamental to direct execution.
- Grow pending lifecycle, approval flow, and request storage inside `ModularityKit.Mutator.Governance`.
- Revisit repo/package boundaries only if governance becomes large enough to justify separate repository in the future.
