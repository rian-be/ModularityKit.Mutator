using ModularityKit.Mutator.Governance.Abstractions.Requests;

namespace ModularityKit.Mutator.Governance.Runtime.Resolution;

/// <summary>
/// Evaluates whether a governed request still matches the currently observed state version.
/// </summary>
internal static class MutationRequestVersionEvaluator
{
    /// <summary>
    /// Compares the request expected version with the current state version and returns a normalized evaluation model.
    /// </summary>
    public static MutationRequestVersionEvaluation Evaluate(
        MutationRequest request,
        string currentStateVersion)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(currentStateVersion))
            throw new ArgumentException("Current state version is required.", nameof(currentStateVersion));

        var expectedStateVersion = request.ExpectedStateVersion;
        var isStale = !string.IsNullOrWhiteSpace(expectedStateVersion)
                      && !string.Equals(expectedStateVersion, currentStateVersion, StringComparison.Ordinal);

        return new MutationRequestVersionEvaluation
        {
            ExpectedStateVersion = expectedStateVersion,
            CurrentStateVersion = currentStateVersion,
            IsStale = isStale
        };
    }
}
