namespace ModularityKit.Mutator.Governance;

/// <summary>
/// Describes why a mutation request cannot execute immediately.
/// </summary>
public enum PendingMutationReason
{
    Approval = 0,
    ExternalCheck = 1,
    Schedule = 2,
    Dependency = 3,
    Quota = 4,
    ManualReview = 5
}
