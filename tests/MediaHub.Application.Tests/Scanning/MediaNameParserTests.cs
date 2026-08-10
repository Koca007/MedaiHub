using MediaHub.Application.Scanning;
using MediaHub.Domain.Library;
using MediaHub.Scanner.Parsing;

namespace MediaHub.Application.Tests.Scanning;

public sealed class MediaNameParserTests
{
    [Fact]
    public void ParsesSeasonEpisodeAndTechnicalTokens()
    {
        var result = MediaNameParser.Parse(
            "Breaking.Bad.S03E07.1080p.BluRay.x265.mkv",
            LibraryRootKind.Series);

        Assert.Equal(MediaCandidateKind.Episode, result.CandidateKind);
        Assert.Equal("Breaking Bad", result.CandidateTitle);
        Assert.Equal(3, result.SeasonNumber);
        Assert.Equal([7], result.EpisodeNumbers);
        Assert.Equal("1080p", result.Resolution);
        Assert.Equal("BLURAY", result.Source);
        Assert.Equal("H.265", result.Codec);
        Assert.True(result.Confidence >= 0.9);
    }

    [Theory]
    [InlineData("Show.S01E01E02.mkv", 1, 2)]
    [InlineData("Show.S01E01-E03.mkv", 1, 2, 3)]
    public void ParsesMultiEpisodeFiles(string fileName, params int[] expectedEpisodes)
    {
        var result = MediaNameParser.Parse(fileName, LibraryRootKind.Series);

        Assert.Equal(expectedEpisodes, result.EpisodeNumbers);
    }

    [Fact]
    public void ParsesAlternateEpisodeNotation()
    {
        var result = MediaNameParser.Parse(
            "Series Name - 2x04 - Episode Title.mp4",
            LibraryRootKind.Series);

        Assert.Equal(MediaCandidateKind.Episode, result.CandidateKind);
        Assert.Equal("Series Name", result.CandidateTitle);
        Assert.Equal(2, result.SeasonNumber);
        Assert.Equal([4], result.EpisodeNumbers);
    }

    [Theory]
    [InlineData("1917.1080p.BluRay.mkv", "1917", null)]
    [InlineData("1917.2019.1080p.BluRay.mkv", "1917", 2019)]
    [InlineData("Movie.Title.2025.Directors.Cut.2160p.HDR.mkv", "Movie Title", 2025)]
    public void DistinguishesNumericTitlesFromReleaseYears(
        string fileName,
        string expectedTitle,
        int? expectedYear)
    {
        var result = MediaNameParser.Parse(fileName, LibraryRootKind.Movies);

        Assert.Equal(MediaCandidateKind.Movie, result.CandidateKind);
        Assert.Equal(expectedTitle, result.CandidateTitle);
        Assert.Equal(expectedYear, result.ReleaseYear);
    }

    [Fact]
    public void ParsesAbsoluteEpisodeInSeriesRoot()
    {
        var result = MediaNameParser.Parse(
            "[Group] Anime.Title.-.012.[1080p].mkv",
            LibraryRootKind.Series);

        Assert.Equal(MediaCandidateKind.Episode, result.CandidateKind);
        Assert.Equal("Anime Title", result.CandidateTitle);
        Assert.Null(result.SeasonNumber);
        Assert.Equal([12], result.EpisodeNumbers);
    }
}
