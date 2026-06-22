using ModularityKit.Mutator.Abstractions.Context;

namespace ModularityKit.Mutator.Governance.Abstractions.Requests;

/// <summary>
/// Captures a single decision or lifecycle transition applied to a mutation request.
/// </summary>
public sealed record MutationRequestDecision
{
    /// <summary>
    /// Type of the decision that was taken.
    /// </summary>
    public MutationRequestDecisionType Type { get; init; }

    /// <summary>
    /// Context of the actor or system that recorded the decision.
    /// </summary>
    public MutationContext Context { get; init; } = null!;

    /// <summary>
    /// Optional human-readable reason for the decision.
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    /// Timestamp at which the decision was recorded.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Optional metadata for governance integrations or diagnostics.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();

    /// <summary>
    /// Creates a new request decision entry.
    /// </summary>
    public static MutationRequestDecision Create(
        MutationRequestDecisionType type,
        MutationContext context,
        string? reason = null,
        IReadOnlyDictionary<string, object>? metadata = null)
        => new()
        {
            Type = type,
            Context = context,
            Reason = reason,
            Metadata = metadata ?? new Dictionary<string, object>()
        };
}
