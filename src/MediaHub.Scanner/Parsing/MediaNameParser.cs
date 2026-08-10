using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using MediaHub.Application.Scanning;
using MediaHub.Domain.Library;

namespace MediaHub.Scanner.Parsing;

public sealed partial class MediaNameParser
{
    public static ParsedMediaName Parse(string filePath, LibraryRootKind rootKind)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("A media path is required.", nameof(filePath));
        }

        var baseName = Path.GetFileNameWithoutExtension(filePath)
            .Normalize(NormalizationForm.FormKC);
        var normalized = WhitespaceRegex()
            .Replace(SeparatorRegex().Replace(baseName, " "), " ")
            .Trim();
        var evidence = new List<string>();

        var episodeMatch = SeasonEpisodeRegex().Match(normalized);
        var alternateEpisodeMatch = episodeMatch.Success
            ? Match.Empty
            : AlternateEpisodeRegex().Match(normalized);
        var textualEpisodeMatch = episodeMatch.Success || alternateEpisodeMatch.Success
            ? Match.Empty
            : TextualEpisodeRegex().Match(normalized);

        int? seasonNumber = null;
        int[] episodeNumbers = [];
        var episodeMarkerIndex = int.MaxValue;

        if (episodeMatch.Success)
        {
            seasonNumber = ParseNumber(episodeMatch.Groups["season"].Value);
            episodeNumbers = ParseEpisodeSequence(
                episodeMatch.Groups["first"].Value,
                episodeMatch.Groups["tail"].Value);
            episodeMarkerIndex = episodeMatch.Index;
            evidence.Add("explicit season and episode token");
        }
        else if (alternateEpisodeMatch.Success)
        {
            seasonNumber = ParseNumber(alternateEpisodeMatch.Groups["season"].Value);
            episodeNumbers = ParseAlternateEpisodeSequence(alternateEpisodeMatch);
            episodeMarkerIndex = alternateEpisodeMatch.Index;
            evidence.Add("explicit alternate episode token");
        }
        else if (textualEpisodeMatch.Success)
        {
            seasonNumber = ParseNumber(textualEpisodeMatch.Groups["season"].Value);
            episodeNumbers = [ParseNumber(textualEpisodeMatch.Groups["episode"].Value)];
            episodeMarkerIndex = textualEpisodeMatch.Index;
            evidence.Add("explicit textual episode token");
        }
        else if (rootKind == LibraryRootKind.Series)
        {
            var absoluteMatch = AbsoluteEpisodeRegex().Match(normalized);
            if (absoluteMatch.Success)
            {
                episodeNumbers = [ParseNumber(absoluteMatch.Groups["episode"].Value)];
                episodeMarkerIndex = absoluteMatch.Index;
                evidence.Add("absolute episode token in series root");
            }
        }

        var yearMatches = YearRegex().Matches(normalized);
        var yearMatch = SelectYearMatch(normalized, yearMatches);
        int? releaseYear = yearMatch is null
            ? null
            : ParseNumber(yearMatch.Value);
        if (releaseYear is not null)
        {
            evidence.Add("release year token");
        }

        var technicalMatch = TechnicalTokenRegex().Match(normalized);
        var titleEnd = new[]
        {
            episodeMarkerIndex,
            yearMatch?.Index ?? int.MaxValue,
            technicalMatch.Success ? technicalMatch.Index : int.MaxValue,
        }.Min();
        if (titleEnd == int.MaxValue)
        {
            titleEnd = normalized.Length;
        }

        var title = normalized[..titleEnd];
        title = LeadingReleaseGroupRegex().Replace(title, string.Empty);
        title = TitleDecorationRegex().Replace(title, " ");
        title = WhitespaceRegex().Replace(title, " ").Trim(' ', '-', '[', ']', '(', ')');
        if (string.IsNullOrWhiteSpace(title))
        {
            title = normalized;
        }

        var candidateKind = episodeNumbers.Length > 0
            ? MediaCandidateKind.Episode
            : rootKind == LibraryRootKind.Series
                ? MediaCandidateKind.Unknown
                : MediaCandidateKind.Movie;

        var resolution = MatchValue(ResolutionRegex(), normalized);
        var source = NormalizeSource(MatchValue(SourceRegex(), normalized));
        var codec = NormalizeCodec(MatchValue(CodecRegex(), normalized));
        if (resolution is not null)
        {
            evidence.Add("resolution token");
        }

        var confidence = candidateKind switch
        {
            MediaCandidateKind.Episode when seasonNumber is not null => 0.94,
            MediaCandidateKind.Episode => 0.76,
            MediaCandidateKind.Movie when releaseYear is not null => 0.82,
            MediaCandidateKind.Movie => 0.58,
            _ => 0.45,
        };

        return new ParsedMediaName(
            candidateKind,
            title,
            releaseYear,
            seasonNumber,
            episodeNumbers,
            resolution,
            source,
            codec,
            confidence,
            evidence);
    }

    private static Match? SelectYearMatch(string normalized, MatchCollection matches)
    {
        if (matches.Count == 0)
        {
            return null;
        }

        if (matches.Count > 1)
        {
            return matches[^1];
        }

        var onlyMatch = matches[0];
        return string.IsNullOrWhiteSpace(normalized[..onlyMatch.Index])
            ? null
            : onlyMatch;
    }

    private static int[] ParseEpisodeSequence(string firstValue, string tail)
    {
        var first = ParseNumber(firstValue);
        var additional = NumberRegex()
            .Matches(tail)
            .Select(match => ParseNumber(match.Value))
            .ToArray();

        if (tail.Contains('-', StringComparison.Ordinal)
            && additional.Length == 1
            && additional[0] >= first)
        {
            return Enumerable.Range(first, additional[0] - first + 1).ToArray();
        }

        return new[] { first }.Concat(additional).Distinct().ToArray();
    }

    private static int[] ParseAlternateEpisodeSequence(Match match)
    {
        var first = ParseNumber(match.Groups["first"].Value);
        if (!match.Groups["last"].Success)
        {
            return [first];
        }

        var last = ParseNumber(match.Groups["last"].Value);
        return last >= first
            ? Enumerable.Range(first, last - first + 1).ToArray()
            : [first, last];
    }

    private static int ParseNumber(string value) =>
        int.Parse(value, NumberStyles.None, CultureInfo.InvariantCulture);

    private static string? MatchValue(Regex regex, string value)
    {
        var match = regex.Match(value);
        return match.Success ? match.Value : null;
    }

    private static string? NormalizeSource(string? source)
    {
        if (source is null)
        {
            return null;
        }

        return source.Replace(" ", "-", StringComparison.Ordinal).ToUpperInvariant();
    }

    private static string? NormalizeCodec(string? codec)
    {
        if (codec is null)
        {
            return null;
        }

        return codec.ToUpperInvariant() switch
        {
            "X265" or "HEVC" => "H.265",
            "X264" => "H.264",
            "AV1" => "AV1",
            _ => codec.ToUpperInvariant(),
        };
    }

    [GeneratedRegex(@"[._]+", RegexOptions.CultureInvariant)]
    private static partial Regex SeparatorRegex();

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant)]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(
        @"\bS(?<season>\d{1,2})E(?<first>\d{1,3})(?<tail>(?:E\d{1,3}|-E?\d{1,3}|-\d{1,3})*)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex SeasonEpisodeRegex();

    [GeneratedRegex(
        @"\b(?<season>\d{1,2})x(?<first>\d{1,3})(?:-(?:(?:\d{1,2})x)?(?<last>\d{1,3}))?\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex AlternateEpisodeRegex();

    [GeneratedRegex(
        @"\bSeason\s+(?<season>\d{1,2})\s+Episode\s+(?<episode>\d{1,3})\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex TextualEpisodeRegex();

    [GeneratedRegex(
        @"(?:^|\s-\s)(?<episode>\d{1,3})(?=\s*(?:\[|$))",
        RegexOptions.CultureInvariant)]
    private static partial Regex AbsoluteEpisodeRegex();

    [GeneratedRegex(@"(?<!\d)(?:19\d{2}|20\d{2})(?!\d)", RegexOptions.CultureInvariant)]
    private static partial Regex YearRegex();

    [GeneratedRegex(
        @"\b(?:720p|1080p|2160p|4K|BluRay|WEB[ -]?DL|WEBRip|HDTV|x264|x265|HEVC|AV1|HDR10\+?|DV)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex TechnicalTokenRegex();

    [GeneratedRegex(@"\b(?:720p|1080p|2160p|4K)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex ResolutionRegex();

    [GeneratedRegex(@"\b(?:BluRay|WEB[ -]?DL|WEBRip|HDTV)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex SourceRegex();

    [GeneratedRegex(@"\b(?:x264|x265|HEVC|AV1)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex CodecRegex();

    [GeneratedRegex(@"^\s*\[[^\]]+\]\s*", RegexOptions.CultureInvariant)]
    private static partial Regex LeadingReleaseGroupRegex();

    [GeneratedRegex(@"[\[\]()]+", RegexOptions.CultureInvariant)]
    private static partial Regex TitleDecorationRegex();

    [GeneratedRegex(@"\d+", RegexOptions.CultureInvariant)]
    private static partial Regex NumberRegex();
}
