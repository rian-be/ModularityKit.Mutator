namespace ModularityKit.Mutator.Governance.Runtime.Resolution.Evaluation;

/// <summary>
/// Describes how a governed request version compares to the currently observed state version.
/// </summary>
internal sealed record MutationRequestVersionEvaluation
{
    /// <summary>
    /// Expected version captured on the request before resolution.
    /// </summary>
    public string? ExpectedStateVersion { get; init; }

    /// <summary>
    /// Current version observed by the runtime during resolution.
    /// </summary>
    public string CurrentStateVersion { get; init; } = string.Empty;

    /// <summary>
    /// Indicates whether the expected and current versions differ.
    /// </summary>
    public bool IsStale { get; init; }
}
