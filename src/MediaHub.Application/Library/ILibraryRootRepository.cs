using MediaHub.Domain.Library;

namespace MediaHub.Application.Library;

public interface ILibraryRootRepository
{
    Task<IReadOnlyList<LibraryRoot>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<LibraryRoot?> FindByNormalizedPathAsync(
        string normalizedPath,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        LibraryRoot libraryRoot,
        CancellationToken cancellationToken = default);
}
