namespace ModularityKit.Mutator.Governance.Abstractions.Requests.Decisions;

/// <summary>
/// Groups governance request decisions by the runtime concern that produced them.
/// </summary>
public enum MutationRequestDecisionCategory
{
    Lifecycle = 0,
    Approval = 1,
    VersionResolution = 2
}
