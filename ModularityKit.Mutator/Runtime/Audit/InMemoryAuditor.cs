using ModularityKit.Mutator.Abstractions.Audit;

namespace ModularityKit.Mutator.Runtime.Audit;

/// <summary>
/// An in-memory implementation of <see cref="IMutationAuditor"/> suitable for testing and development.
/// </summary>
/// <remarks>
/// <para>
/// All audit entries are stored in memory. This implementation is <b>not suitable for production</b>
/// as it does not persist entries beyond the lifetime of the process.
/// </para>
/// <para>
/// Thread safe: all public methods use locking to prevent concurrent access issues.
/// </para>
/// </remarks>
internal sealed class InMemoryAuditor : IMutationAuditor
{
    private readonly List<MutationAuditEntry> _entries = [];
    private readonly Lock _lock = new();

    /// <inheritdoc />
    public Task AuditAsync(MutationAuditEntry entry)
        => AuditAsync(entry, CancellationToken.None);

    /// <inheritdoc />
    public Task AuditAsync(
        MutationAuditEntry entry,
        CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            _entries.Add(entry);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<MutationAuditEntry>> GetAuditLogAsync(string stateId)
        => GetAuditLogAsync(stateId, null, null, CancellationToken.None);

    /// <inheritdoc />
    public Task<IReadOnlyList<MutationAuditEntry>> GetAuditLogAsync(
        string stateId,
        DateTimeOffset? from,
        DateTimeOffset? to)
        => GetAuditLogAsync(stateId, from, to, CancellationToken.None);

    /// <inheritdoc />
    public Task<IReadOnlyList<MutationAuditEntry>> GetAuditLogAsync(
        string stateId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            var query =
                _entries.Where(e => e.StateId == stateId);

            if (from.HasValue)
                query = query.Where(e => e.Timestamp >= from.Value);

            if (to.HasValue)
                query = query.Where(e => e.Timestamp <= to.Value);

            return Task.FromResult<IReadOnlyList<MutationAuditEntry>>(
                [.. query]);
        }
    }

    /// <summary>
    /// Returns all stored audit entries in memory.
    /// </summary>
    public IReadOnlyList<MutationAuditEntry> GetAllEntries()
    {
        lock (_lock)
        {
            return [.. _entries];
        }
    }

    /// <summary>
    /// Clears all in-memory audit entries.
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _entries.Clear();
        }
    }
}
