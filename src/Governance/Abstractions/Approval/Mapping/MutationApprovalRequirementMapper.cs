using System.Collections;
using ModularityKit.Mutator.Abstractions.Policies;
using ModularityKit.Mutator.Governance.Abstractions.Approval.Model;

namespace ModularityKit.Mutator.Governance.Abstractions.Approval.Mapping;

internal static class MutationApprovalRequirementMapper
{
    public static IReadOnlyList<MutationApprovalRequirement> Map(
        IReadOnlyList<PolicyRequirement>? requirements)
    {
        if (requirements is null || requirements.Count == 0)
            return [];

        var mapped = new List<MutationApprovalRequirement>();
        var approvalIndex = 0;

        foreach (var requirement in requirements)
        {
            if (!string.Equals(requirement.Type, "Approval", StringComparison.Ordinal))
                continue;

            var approvalDefinitions = ExtractApprovalDefinitions(requirement, approvalIndex);
            mapped.AddRange(approvalDefinitions);
            approvalIndex++;
        }

        return mapped;
    }

    private static IReadOnlyList<MutationApprovalRequirement> ExtractApprovalDefinitions(
        PolicyRequirement requirement,
        int defaultStepOrder)
    {
        var stepOrder = ReadIntProperty(requirement.Data, "StepOrder") ?? defaultStepOrder + 1;
        var approverName = ReadStringProperty(requirement.Data, "ApproverName");
        var reason = ReadStringProperty(requirement.Data, "Reason");

        var approvers = ReadStringSequenceProperty(requirement.Data, "Approvers");
        if (approvers.Count == 0)
        {
            var approver = ReadStringProperty(requirement.Data, "Approver");
            if (!string.IsNullOrWhiteSpace(approver))
                approvers = [approver];
        }

        if (approvers.Count == 0)
            throw new InvalidOperationException(
                $"Approval requirement '{requirement.Description}' does not define an approver.");

        return approvers
            .Select(approverId => new MutationApprovalRequirement
            {
                Type = requirement.Type,
                Description = requirement.Description,
                ApproverId = approverId,
                ApproverName = approverName,
                StepOrder = stepOrder,
                Metadata = new Dictionary<string, object>
                {
                    ["RequirementDescription"] = requirement.Description,
                    ["RequirementReason"] = reason ?? string.Empty
                }
            })
            .ToList();
    }

    private static string? ReadStringProperty(object? source, string propertyName)
    {
        if (source is null)
            return null;

        var property = source.GetType().GetProperty(propertyName);
        var value = property?.GetValue(source);
        return value as string;
    }

    private static int? ReadIntProperty(object? source, string propertyName)
    {
        if (source is null)
            return null;

        var property = source.GetType().GetProperty(propertyName);
        var value = property?.GetValue(source);
        return value switch
        {
            int intValue => intValue,
            long longValue => checked((int)longValue),
            short shortValue => shortValue,
            _ => null
        };
    }

    private static List<string> ReadStringSequenceProperty(object? source, string propertyName)
    {
        if (source is null)
            return [];

        var property = source.GetType().GetProperty(propertyName);
        var value = property?.GetValue(source);

        return value switch
        {
            IEnumerable<string> typedStrings => typedStrings.Where(static x => !string.IsNullOrWhiteSpace(x)).ToList(),
            IEnumerable sequence => sequence.Cast<object?>()
                .OfType<string>()
                .Where(static x => !string.IsNullOrWhiteSpace(x))
                .ToList(),
            _ => []
        };
    }
}
