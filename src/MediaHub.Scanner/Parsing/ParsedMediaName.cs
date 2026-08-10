using MediaHub.Application.Scanning;

namespace MediaHub.Scanner.Parsing;

public sealed record ParsedMediaName(
    MediaCandidateKind CandidateKind,
    string CandidateTitle,
    int? ReleaseYear,
    int? SeasonNumber,
    IReadOnlyList<int> EpisodeNumbers,
    string? Resolution,
    string? Source,
    string? Codec,
    double Confidence,
    IReadOnlyList<string> Evidence);
