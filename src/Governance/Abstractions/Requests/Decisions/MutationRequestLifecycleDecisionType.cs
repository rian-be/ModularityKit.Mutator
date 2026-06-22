namespace ModularityKit.Mutator.Governance.Abstractions.Requests.Decisions;

/// <summary>
/// Represents high-level lifecycle decisions taken against a mutation request.
/// </summary>
public enum MutationRequestLifecycleDecisionType
{
    Submitted = 0,
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Canceled = 4,
    Expired = 5,
    Superseded = 6,
    Executed = 7
}
