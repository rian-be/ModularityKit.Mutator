namespace ModularityKit.Mutator.Governance.Abstractions.Requests;

/// <summary>
/// Represents a governance decision taken against a mutation request.
/// </summary>
public enum MutationRequestDecisionType
{
    Submitted = 0,
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Canceled = 4,
    Expired = 5,
    Superseded = 6,
    Executed = 7,
    VersionValidated = 8,
    RevalidationRequired = 9,
    RenewedApprovalRequired = 10,
    RejectedAsStale = 11
}
