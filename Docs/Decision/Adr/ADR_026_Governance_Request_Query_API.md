# ADR-026: Governance Request Query API

## Tag
#adr_026

## Status
Proposed

## Date
2026-06-22

## Scope
ModularityKit.Mutator.Governance

## Context

The governance package is moving toward durable request lifecycle, approval history, and future persistence providers.

Point lookups by request id are not enough for operational governance scenarios. Users need queries such as:

- all pending approvals
- requests for a given state
- requests by actor
- requests by category or risk level
- requests filtered by tags, metadata, or blast radius
- recent approval decisions

Without a query API, persistence becomes only storage and governance data remains hard to use operationally.

## Decision

The governance package should define a storage-agnostic request query API.

Expected direction:

- expose query-oriented contracts in the governance package
- support filtering by status, pending reason, actor, category, and time range
- support governance-specific filters such as tags, metadata, and blast radius
- support approval-oriented views such as pending approval queues and recent decisions
- keep the query surface provider-neutral so future persistence packages can implement it consistently

The exact query object model is intentionally left open for the first implementation.

## Design Rationale

- Governance data becomes useful only when it is operationally queryable.
- Query semantics should be owned by governance rather than improvised in store implementations.
- A storage-agnostic contract keeps future provider packages aligned.

## Consequences

### Positive

- Governance can support real review and operational workflows.
- Future persistence providers have a clear contract to implement.
- Tags, metadata, and blast radius fields get a practical consumer path.

### Negative

- Query surface design can grow quickly if not kept disciplined.
- Different storage providers may support different performance characteristics for the same filters.

## Related ADRs

- ADR-020: Governance MutationRequest Model
- ADR-022: Governance Request Decisions and Storage
- ADR-023: Governance Versioned Request Resolution
