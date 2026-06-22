using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Abstractions.Intent;
using ModularityKit.Mutator.Abstractions.Policies;
using ModularityKit.Mutator.Governance.Abstractions.Lifecycle;

namespace ModularityKit.Mutator.Governance.Abstractions.Requests;

/// <summary>
/// Represents a governed mutation request that may execute immediately or enter a pending lifecycle.
/// </summary>
public sealed record MutationRequest
{
    /// <summary>
    /// Stable identifier for the mutation request.
    /// </summary>
    public string RequestId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Identifier of the state targeted by this request.
    /// </summary>
    public string StateId { get; init; } = string.Empty;

    /// <summary>
    /// Logical state type targeted by the request.
    /// </summary>
    public string StateType { get; init; } = string.Empty;

    /// <summary>
    /// CLR type name of the underlying mutation.
    /// </summary>
    public string MutationType { get; init; } = string.Empty;

    /// <summary>
    /// Intent associated with the requested mutation.
    /// </summary>
    public MutationIntent Intent { get; init; } = null!;

    /// <summary>
    /// Request context describing who requested the mutation and why.
    /// </summary>
    public MutationContext Context { get; init; } = null!;

    /// <summary>
    /// Current lifecycle status of the request.
    /// </summary>
    public MutationRequestStatus Status { get; init; } = MutationRequestStatus.Created;

    /// <summary>
    /// Reason why the request is pending, if it has not executed yet.
    /// </summary>
    public PendingMutationReason? PendingReason { get; init; }

    /// <summary>
    /// Requirements that must be fulfilled before execution may proceed.
    /// </summary>
    public IReadOnlyList<PolicyRequirement> Requirements { get; init; } = [];

    /// <summary>
    /// Governance decisions recorded against this request over time.
    /// </summary>
    public IReadOnlyList<MutationRequestDecision> Decisions { get; init; } = [];

    /// <summary>
    /// Optimistic concurrency revision for the governed request.
    /// </summary>
    public long Revision { get; init; }

    /// <summary>
    /// Expected version or concurrency token for the target state.
    /// </summary>
    public string? ExpectedStateVersion { get; init; }

    /// <summary>
    /// Optional expiration time for pending requests.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; init; }

    /// <summary>
    /// Timestamp when the request was first created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Timestamp of the last lifecycle update applied to the request.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Additional governance metadata carried by the request.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();

    /// <summary>
    /// Creates a request that should enter the pending lifecycle.
    /// </summary>
    public static MutationRequest Pending(
        string stateId,
        string stateType,
        string mutationType,
        MutationIntent intent,
        MutationContext context,
        PendingMutationReason pendingReason,
        IReadOnlyList<PolicyRequirement>? requirements = null,
        string? expectedStateVersion = null,
        DateTimeOffset? expiresAt = null,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        return new MutationRequest
        {
            StateId = stateId,
            StateType = stateType,
            MutationType = mutationType,
            Intent = intent,
            Context = context,
            Status = MutationRequestStatus.Pending,
            PendingReason = pendingReason,
            Requirements = requirements ?? [],
            ExpectedStateVersion = expectedStateVersion,
            ExpiresAt = expiresAt,
            Metadata = metadata ?? new Dictionary<string, object>(),
            Decisions =
            [
                MutationRequestDecision.Create(
                    MutationRequestDecisionType.Submitted,
                    context,
                    reason: context.Reason),
                MutationRequestDecision.Create(
                    MutationRequestDecisionType.Pending,
                    context,
                    reason: $"Request entered pending lifecycle for reason '{pendingReason}'.")
            ]
        };
    }

    /// <summary>
    /// Creates a request that is immediately approved for execution.
    /// </summary>
    public static MutationRequest Approved(
        string stateId,
        string stateType,
        string mutationType,
        MutationIntent intent,
        MutationContext context,
        string? expectedStateVersion = null,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        return new MutationRequest
        {
            StateId = stateId,
            StateType = stateType,
            MutationType = mutationType,
            Intent = intent,
            Context = context,
            Status = MutationRequestStatus.Approved,
            ExpectedStateVersion = expectedStateVersion,
            Metadata = metadata ?? new Dictionary<string, object>(),
            Decisions =
            [
                MutationRequestDecision.Create(
                    MutationRequestDecisionType.Submitted,
                    context,
                    reason: context.Reason),
                MutationRequestDecision.Create(
                    MutationRequestDecisionType.Approved,
                    context,
                    reason: "Approved at submission time")
            ]
        };
    }
}
