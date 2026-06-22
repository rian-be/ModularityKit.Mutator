using ModularityKit.Mutator.Governance.Abstractions.Requests.Model;

namespace ModularityKit.Mutator.Governance.Abstractions.Resolution.Model;

/// <summary>
/// Represents the result of resolving a mutation request against the current state version.
/// </summary>
public sealed record MutationRequestVersionResolution
{
    /// <summary>
    /// Updated mutation request after version-aware resolution.
    /// </summary>
    public MutationRequest Request { get; init; } = null!;

    /// <summary>
    /// Outcome selected by the governance runtime after comparing expected and current versions.
    /// </summary>
    public MutationRequestVersionResolutionOutcome Outcome { get; init; }

    /// <summary>
    /// Expected version captured on the request before resolution.
    /// </summary>
    public string? ExpectedStateVersion { get; init; }

    /// <summary>
    /// Current version observed at resolution time.
    /// </summary>
    public string CurrentStateVersion { get; init; } = string.Empty;

    /// <summary>
    /// Indicates whether the current version differs from the original expected version.
    /// </summary>
    public bool IsStale { get; init; }
}
