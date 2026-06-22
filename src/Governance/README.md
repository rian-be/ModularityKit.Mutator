# ModularityKit.Mutator.Governance

`ModularityKit.Mutator.Governance` is the governance focused extension layer for `ModularityKit.Mutator`.

The core package stays responsible for direct mutation execution. Governance builds on top of that runtime with request based lifecycle concepts such as deferred execution, approvals, and request storage.

## Features

- **Mutation Requests** - model governed mutation submission as a durable request
- **Pending Lifecycle** - represent requests that cannot execute immediately
- **Decision History** - record approvals, rejections, cancellations, and other lifecycle transitions
- **Request Storage Contracts** - define a persistence seam for governance-oriented stores
- **Runtime Lifecycle Management** - move requests through pending, approval, expiration, and execution transitions
- **In-Memory Runtime Support** - provide lightweight request runtime services for development and tests

## Current Structure

### Abstractions

The package defines governance-first abstractions under:

- `Abstractions/Requests`
- `Abstractions/Lifecycle`
- `Abstractions/Storage`
- `Abstractions/Resolution`
- `Abstractions/Exceptions`

Key types:

- `MutationRequest`
- `MutationRequestDecision`
- `MutationRequestDecisionType`
- `MutationRequestStatus`
- `PendingMutationReason`
- `IMutationRequestStore`
- `IMutationRequestLifecycleManager`
- `IMutationRequestVersionResolver`
- `IMutationRequestVersionResolutionManager`
- `MutationRequestVersionResolution`
- `MutationRequestVersionResolutionOutcome`
- `VersionedRequestResolutionStrategy`
- `MutationRequestAlreadyExistsException`
- `MutationRequestConcurrencyException`
- `MutationRequestNotFoundException`
- `InvalidMutationRequestTransitionException`

### Runtime

The initial runtime layer currently provides:

- `Runtime/Storage/InMemoryMutationRequestStore`
- `Runtime/Lifecycle/MutationRequestLifecycleManager`
- `Runtime/Resolution/MutationRequestVersionResolver`
- `Runtime/Resolution/MutationRequestVersionResolutionManager`

This keeps the first version small while leaving room for later persistence providers such as Entity Framework Core or PostgreSQL-backed governance stores.

## Relationship to Core

### `ModularityKit.Mutator`

Responsible for:

- mutation execution
- policy evaluation
- audit and history basics
- side effects
- metrics and interception

### `ModularityKit.Mutator.Governance`

Responsible for:

- mutation request lifecycle
- pending execution modeling
- approval oriented governance contracts
- request decision history
- governance specific storage and future query seams

## Direction

This package is intentionally the place where broader governance behavior should grow.

That includes future work such as:

- pending mutation resolution
- approval workflow execution
- version aware deferred execution
- governance persistence providers
- governance query APIs

The goal is to keep the core runtime small and execution focused while letting governance evolve as an opt-in extension.
