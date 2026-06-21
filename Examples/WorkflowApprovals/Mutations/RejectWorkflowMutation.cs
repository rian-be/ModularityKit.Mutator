using ModularityKit.Mutator.Abstractions.Changes;
using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Abstractions.Engine;
using ModularityKit.Mutator.Abstractions.Effects;
using ModularityKit.Mutator.Abstractions.Intent;
using ModularityKit.Mutator.Abstractions.Results;
using WorkflowApprovals.State;

namespace WorkflowApprovals.Mutations;

/// <summary>
/// Mutation that rejects the entire workflow in an <see cref="ApprovalWorkflowState"/>.
/// </summary>
internal sealed record RejectWorkflowMutation(
    string Rejector,
    MutationContext Context
) : IMutation<ApprovalWorkflowState>
{
    public MutationIntent Intent { get; } = new()
    {
        OperationName = "RejectWorkflow",
        Category = "Workflow",
        RiskLevel = MutationRiskLevel.Critical,
        Description = "Rejects the entire workflow"
    };

    public ValidationResult Validate(ApprovalWorkflowState state)
    {
        var result = new ValidationResult();
        if (string.IsNullOrEmpty(Rejector))
            result.AddError("Reject", "Reject cannot be empty");
        return result;
    }

    public MutationResult<ApprovalWorkflowState> Apply(ApprovalWorkflowState state)
    {
        var steps = state.Steps.Select(s => s with
        {
            Status = StepStatus.Rejected,
            RejectedBy = Rejector
        }).ToList();

        var newState = state with { Steps = steps };
        var changes = ChangeSet.Single(StateChange.Modified("Workflow", null, "Rejected"));
        return MutationResult<ApprovalWorkflowState>.Success(
            newState,
            changes,
            [
                SideEffect.Critical(
                    type: "WorkflowRejected",
                    description: "Workflow rejection requires manual follow-up",
                    data: new
                    {
                        Rejector,
                        StepCount = steps.Count,
                        State = "Rejected"
                    })
            ]);
    }

    public MutationResult<ApprovalWorkflowState> Simulate(ApprovalWorkflowState state) => Apply(state);
}
