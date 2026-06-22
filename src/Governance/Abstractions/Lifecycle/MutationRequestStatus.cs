namespace ModularityKit.Mutator.Governance.Abstractions.Lifecycle;

/// <summary>
/// Represents the lifecycle status of governed mutation request.
/// </summary>
public enum MutationRequestStatus
{
    Created = 0,
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Canceled = 4,
    Expired = 5,
    Superseded = 6,
    Executed = 7
}
