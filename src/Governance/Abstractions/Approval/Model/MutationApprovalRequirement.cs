using ModularityKit.Mutator.Abstractions.Context;

namespace ModularityKit.Mutator.Governance.Abstractions.Approval.Model;

/// <summary>
/// Represents one concrete approval action that must be completed before a governed request can proceed.
/// </summary>
public sealed record MutationApprovalRequirement
{
    /// <summary>
    /// Stable identifier of the approval requirement inside the request.
    /// </summary>
    public string ApprovalId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Requirement type copied from the originating policy requirement.
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Human-readable description of the approval requirement.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Identifier of the approver who is allowed to resolve this requirement.
    /// </summary>
    public string ApproverId { get; init; } = string.Empty;

    /// <summary>
    /// Optional human-readable name of the approver.
    /// </summary>
    public string? ApproverName { get; init; }

    /// <summary>
    /// Step number used for ordered approval workflows.
    /// Requirements in the same step may be resolved in any order.
    /// </summary>
    public int StepOrder { get; init; } = 1;

    /// <summary>
    /// Current state of the approval requirement.
    /// </summary>
    public MutationApprovalRequirementStatus Status { get; init; } = MutationApprovalRequirementStatus.Pending;

    /// <summary>
    /// Timestamp when the requirement was resolved.
    /// </summary>
    public DateTimeOffset? DecidedAt { get; init; }

    /// <summary>
    /// Context of the actor who resolved this requirement.
    /// </summary>
    public MutationContext? DecisionContext { get; init; }

    /// <summary>
    /// Optional human-readable reason attached to the approval decision.
    /// </summary>
    public string? DecisionReason { get; init; }

    /// <summary>
    /// Additional approval metadata for integrations and audit trails.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();
}
