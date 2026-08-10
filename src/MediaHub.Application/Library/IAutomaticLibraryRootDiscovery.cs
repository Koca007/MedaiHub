using MediaHub.Domain.Library;

namespace MediaHub.Application.Library;

public interface IAutomaticLibraryRootDiscovery
{
    Task<IReadOnlyList<AutomaticLibraryRootCandidate>> DiscoverAsync(
        CancellationToken cancellationToken = default);
}

public sealed record AutomaticLibraryRootCandidate(
    string Path,
    LibraryRootKind SuggestedKind);
