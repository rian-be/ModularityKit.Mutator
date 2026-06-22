using ModularityKit.Mutator.Abstractions.Context;
using ModularityKit.Mutator.Governance.Abstractions.Exceptions;
using ModularityKit.Mutator.Governance.Abstractions.Resolution;
using ModularityKit.Mutator.Governance.Abstractions.Storage;

namespace ModularityKit.Mutator.Governance.Runtime.Resolution;

/// <summary>
/// Resolves governed requests against the current state version and persists the resulting resolution outcome.
/// </summary>
public sealed class MutationRequestVersionResolutionManager(
    IMutationRequestStore requestStore,
    IMutationRequestVersionResolver versionResolver) : IMutationRequestVersionResolutionManager
{
    private readonly IMutationRequestStore _requestStore = requestStore ?? throw new ArgumentNullException(nameof(requestStore));
    private readonly IMutationRequestVersionResolver _versionResolver = versionResolver ?? throw new ArgumentNullException(nameof(versionResolver));

    /// <summary>
    /// Loads a persisted request, resolves it using version-aware governance semantics, and stores the resulting request revision.
    /// </summary>
    public async Task<MutationRequestVersionResolution> ResolveAndStore(
        string requestId,
        string currentStateVersion,
        MutationContext resolutionContext,
        VersionedRequestResolutionStrategy strategy,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(requestId))
            throw new ArgumentException("Request ID is required.", nameof(requestId));

        var request = await _requestStore.Get(requestId, cancellationToken).ConfigureAwait(false);

        if (request is null)
            throw new MutationRequestNotFoundException(requestId);

        var resolution = _versionResolver.Resolve(
            request,
            currentStateVersion,
            resolutionContext,
            strategy);

        var persistedRequest = await _requestStore
            .TryStore(resolution.Request, request.Revision, cancellationToken)
            .ConfigureAwait(false);

        if (persistedRequest is null)
            throw new MutationRequestConcurrencyException(request.RequestId, request.Revision);

        return resolution with
        {
            Request = persistedRequest
        };
    }
}
