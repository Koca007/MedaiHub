namespace MediaHub.Application.Scanning;

public interface IMediaFileRepository
{
    Task UpsertBatchAsync(
        IReadOnlyCollection<DiscoveredMediaFile> mediaFiles,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MediaFileSummary>> GetRecentAsync(
        int maximumCount,
        CancellationToken cancellationToken = default);

    Task<int> CountByRootAsync(
        Guid libraryRootId,
        CancellationToken cancellationToken = default);
}
