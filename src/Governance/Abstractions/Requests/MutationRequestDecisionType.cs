namespace ModularityKit.Mutator.Governance;

/// <summary>
/// Represents a governance decision taken against a mutation request.
/// </summary>
public enum MutationRequestDecisionType
{
    Submitted = 0,
    Approved = 1,
    Rejected = 2,
    Canceled = 3,
    Expired = 4,
    Superseded = 5,
    Executed = 6
}
