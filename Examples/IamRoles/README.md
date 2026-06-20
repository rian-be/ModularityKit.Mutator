# IamRoles

This example shows how to guard privileged role changes with policy checks and audit friendly context.

It is the most governance focused sample in the repository. Use it when you want to see how to protect sensitive operations such as granting admin rights, revoking permissions, or migrating many roles at once.

## Domain

The domain state maps user IDs to a set of roles.

The example covers three workflows:

- granting role to user
- revoking role from user
- batch migration of role assignments

## What this example demonstrates

- privilege changing mutations
- last admin protection
- approval metadata in `MutationContext`
- two man approval for critical mutations
- batch role operations across multiple users

## Project structure

- [`Program.cs`](Program.cs)
- [`IamRoles.csproj`](IamRoles.csproj)
- [`State/UserPermissionsState.cs`](State/UserPermissionsState.cs)
- [`Mutations/GrantUserRoleMutation.cs`](Mutations/GrantUserRoleMutation.cs)
- [`Mutations/RevokeUserRoleMutation.cs`](Mutations/RevokeUserRoleMutation.cs)
- [`Policies/PreventLastAdminRemovalPolicy.cs`](Policies/PreventLastAdminRemovalPolicy.cs)
- [`Policies/RequireTwoManApprovalPolicy.cs`](Policies/RequireTwoManApprovalPolicy.cs)
- [`Scenarios/GrantAdminScenario.cs`](Scenarios/GrantAdminScenario.cs)
- [`Scenarios/RevokeAdminScenario.cs`](Scenarios/RevokeAdminScenario.cs)
- [`Scenarios/BatchRoleMigrationScenario.cs`](Scenarios/BatchRoleMigrationScenario.cs)

## How it works

`Program.cs`:

1. registers the engine with strict options
2. resolves `IMutationEngine`
3. registers the two governance policies
4. runs the three role management scenarios
5. prints summary statistics

The sample keeps the domain model simple so the policies stay visible.

## Mutation flow

### Grant role

[`GrantUserRoleMutation`](Mutations/GrantUserRoleMutation.cs) grants role to user.

- validates the user ID and role name
- rejects duplicate assignments
- writes the role into copied dictionary
- emits state change for the affected user

### Revoke role

[`RevokeUserRoleMutation`](Mutations/RevokeUserRoleMutation.cs) removes role from user.

- checks that the user actually has the role
- removes the role from copied state
- emits removal change

## Policies

### Prevent last admin removal

[`PreventLastAdminRemovalPolicy`](Policies/PreventLastAdminRemovalPolicy.cs) blocks the removal of the last remaining `Admin` role.

This shows classic safety rule for sensitive systems.

### Require two man approval

[`RequireTwoManApprovalPolicy`](Policies/RequireTwoManApprovalPolicy.cs) requires additional approval metadata for critical changes.

The policy demonstrates:

- inspecting mutation risk level
- reading the approval list from metadata
- rejecting self approval

## Scenarios

### Grant admin

[`GrantAdminScenario`](Scenarios/GrantAdminScenario.cs) promotes user to admin.

### Revoke admin

[`RevokeAdminScenario`](Scenarios/RevokeAdminScenario.cs) removes admin from user.

### Batch role migration

[`BatchRoleMigrationScenario`](Scenarios/BatchRoleMigrationScenario.cs) applies multiple role grants in single batch.

It demonstrates:

- batch execution
- state driven mutation generation
- policy evaluation on each item
- reporting per mutation failures

## What to read first

1. [`State/UserPermissionsState.cs`](State/UserPermissionsState.cs)
2. [`Mutations/GrantUserRoleMutation.cs`](Mutations/GrantUserRoleMutation.cs)
3. [`Mutations/RevokeUserRoleMutation.cs`](Mutations/RevokeUserRoleMutation.cs)
4. [`Policies/PreventLastAdminRemovalPolicy.cs`](Policies/PreventLastAdminRemovalPolicy.cs)
5. [`Policies/RequireTwoManApprovalPolicy.cs`](Policies/RequireTwoManApprovalPolicy.cs)
6. [`Scenarios/BatchRoleMigrationScenario.cs`](Scenarios/BatchRoleMigrationScenario.cs)

## Run

```bash
dotnet run --project Examples/IamRoles/IamRoles.csproj
```

## Expected output

When you run the sample, you should see:

- a single grant flow
- a single revoke flow
- a batch migration flow
- blocked or approved mutations depending on policy input
- final statistics from the engine

This example is the clearest one to study if you care about policy controlled privilege management.
