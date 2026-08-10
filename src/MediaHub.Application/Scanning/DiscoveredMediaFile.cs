namespace MediaHub.Application.Scanning;

public sealed record DiscoveredMediaFile(
    Guid Id,
    Guid LibraryRootId,
    string OriginalPath,
    string NormalizedPath,
    long FileSizeBytes,
    DateTimeOffset ModifiedAtUtc,
    MediaCandidateKind CandidateKind,
    string CandidateTitle,
    int? ReleaseYear,
    int? SeasonNumber,
    IReadOnlyList<int> EpisodeNumbers,
    string? Resolution,
    string? Source,
    string? Codec,
    double Confidence,
    DateTimeOffset LastSeenAtUtc);

public sealed record MediaFileSummary(
    Guid Id,
    Guid LibraryRootId,
    string OriginalPath,
    MediaCandidateKind CandidateKind,
    string CandidateTitle,
    int? ReleaseYear,
    int? SeasonNumber,
    IReadOnlyList<int> EpisodeNumbers,
    string? Resolution,
    string? Source,
    string? Codec,
    double Confidence,
    DateTimeOffset LastSeenAtUtc);
