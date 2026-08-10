using MediaHub.Shared.Contracts.Cloud;

namespace MediaHub.Application.Abstractions.Cloud;

public interface IReleaseCatalogService
{
    Task<IReadOnlyList<ReleaseCatalogItem>> GetActiveReleasesAsync(
        string channel,
        CancellationToken cancellationToken);
}
