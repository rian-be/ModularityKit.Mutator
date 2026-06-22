namespace ModularityKit.Mutator.Governance.Abstractions.Requests.Decisions;

/// <summary>
/// Identifies one governance request decision together with the concern category that produced it.
/// </summary>
public readonly record struct MutationRequestDecisionType
{
    /// <summary>
    /// Decision category.
    /// </summary>
    public MutationRequestDecisionCategory Category { get; init; }

    /// <summary>
    /// Stable textual code of the decision inside the category.
    /// </summary>
    public string Code { get; init; }

    /// <summary>
    /// Creates a lifecycle decision type wrapper.
    /// </summary>
    public static MutationRequestDecisionType Lifecycle(MutationRequestLifecycleDecisionType type)
        => new()
        {
            Category = MutationRequestDecisionCategory.Lifecycle,
            Code = type.ToString()
        };

    /// <summary>
    /// Creates an approval decision type wrapper.
    /// </summary>
    public static MutationRequestDecisionType Approval(MutationRequestApprovalDecisionType type)
        => new()
        {
            Category = MutationRequestDecisionCategory.Approval,
            Code = type.ToString()
        };

    /// <summary>
    /// Creates a version-resolution decision type wrapper.
    /// </summary>
    public static MutationRequestDecisionType VersionResolution(MutationRequestVersionResolutionDecisionType type)
        => new()
        {
            Category = MutationRequestDecisionCategory.VersionResolution,
            Code = type.ToString()
        };

    /// <summary>
    /// Returns the stable code of the wrapped decision.
    /// </summary>
    public override string ToString() => Code;
}
