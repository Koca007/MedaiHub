using MediaHub.Domain.Library;

namespace MediaHub.Application.Library;

public sealed class LibraryRootCatalog
{
    private readonly ILibraryRootRepository _repository;
    private readonly ILibraryPathInspector _pathInspector;
    private readonly TimeProvider _timeProvider;

    public LibraryRootCatalog(
        ILibraryRootRepository repository,
        ILibraryPathInspector pathInspector,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(pathInspector);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _repository = repository;
        _pathInspector = pathInspector;
        _timeProvider = timeProvider;
    }

    public async Task<IReadOnlyList<LibraryRootSummary>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var roots = await _repository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        return roots.Select(ToSummary).ToArray();
    }

    public async Task<AddLibraryRootResult> AddAsync(
        string path,
        LibraryRootKind kind,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return AddLibraryRootResult.Rejected(AddLibraryRootFailure.EmptyPath);
        }

        var inspection = await _pathInspector
            .InspectAsync(path, cancellationToken)
            .ConfigureAwait(false);

        if (!inspection.IsAccessible)
        {
            return AddLibraryRootResult.Rejected(MapFailure(inspection.Failure));
        }

        var normalizedPath = inspection.NormalizedPath!;
        var existing = await _repository
            .FindByNormalizedPathAsync(normalizedPath, cancellationToken)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            return AddLibraryRootResult.Rejected(AddLibraryRootFailure.Duplicate);
        }

        var root = LibraryRoot.Create(
            Guid.NewGuid(),
            inspection.FullPath!,
            normalizedPath,
            kind,
            _timeProvider.GetUtcNow());

        await _repository.AddAsync(root, cancellationToken).ConfigureAwait(false);
        return AddLibraryRootResult.Added(ToSummary(root));
    }

    private static LibraryRootSummary ToSummary(LibraryRoot root) =>
        new(
            root.Id,
            root.Path,
            root.Kind,
            root.Availability,
            root.IsEnabled,
            root.LastCheckedAtUtc);

    private static AddLibraryRootFailure MapFailure(LibraryPathFailure failure) =>
        failure switch
        {
            LibraryPathFailure.InvalidPath => AddLibraryRootFailure.InvalidPath,
            LibraryPathFailure.NotFound => AddLibraryRootFailure.NotFound,
            LibraryPathFailure.AccessDenied => AddLibraryRootFailure.AccessDenied,
            LibraryPathFailure.InputOutputError => AddLibraryRootFailure.InputOutputError,
            _ => AddLibraryRootFailure.InvalidPath,
        };
}
