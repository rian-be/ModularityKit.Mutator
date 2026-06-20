using ModularityKit.Mutator.Abstractions.Changes;
using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Abstractions.Engine;
using ModularityKit.Mutator.Abstractions.Intent;
using ModularityKit.Mutator.Abstractions.Results;
using WorkflowApprovals.State;

namespace WorkflowApprovals.Mutations;

/// <summary>
/// Mutation that starts new approval workflow in an <see cref="ApprovalWorkflowState"/>.
/// </summary>
internal sealed record StartApprovalMutation(
    string Initiator,
    string[] StepNames,
    MutationContext Context
) : IMutation<ApprovalWorkflowState>
{
    public MutationIntent Intent { get; } = new()
    {
        OperationName = "StartWorkflow",
        Category = "Workflow",
        RiskLevel = MutationRiskLevel.Medium,
        Description = "Starts a new approval workflow"
    };

    public ValidationResult Validate(ApprovalWorkflowState state)
    {
        var result = new ValidationResult();
        if (string.IsNullOrEmpty(Initiator))
            result.AddError("Initiator", "Initiator cannot be empty");
        if (StepNames.Length == 0)
            result.AddError("Steps", "Workflow must have at least one step");
        return result;
    }

    public MutationResult<ApprovalWorkflowState> Apply(ApprovalWorkflowState state)
    {
        var steps = StepNames.Select(name => new WorkflowStep(name)).ToList();
        var newState = state with
        {
            WorkflowId = Guid.NewGuid().ToString(),
            Steps = steps,
            Initiator = Initiator
        };

        var changes = ChangeSet.Single(StateChange.Added("Steps", steps));
        return MutationResult<ApprovalWorkflowState>.Success(newState, changes);
    }

    public MutationResult<ApprovalWorkflowState> Simulate(ApprovalWorkflowState state) => Apply(state);
}