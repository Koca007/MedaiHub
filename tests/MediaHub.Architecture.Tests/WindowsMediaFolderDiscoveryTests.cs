using MediaHub.Domain.Library;
using MediaHub.Infrastructure.Local.Library;

namespace MediaHub.Architecture.Tests;

public sealed class WindowsMediaFolderDiscoveryTests
{
    [Fact]
    public async Task DiscoveryFindsNestedVideoAndInfersSeriesFolder()
    {
        var testDirectory = CreateTestDirectory();
        var seriesDirectory = Path.Combine(testDirectory, "Series");
        var seasonDirectory = Path.Combine(seriesDirectory, "Season 01");
        var emptyDirectory = Path.Combine(testDirectory, "Documents");
        Directory.CreateDirectory(seasonDirectory);
        Directory.CreateDirectory(emptyDirectory);
        File.WriteAllBytes(
            Path.Combine(seasonDirectory, "Example.Show.S01E01.MKV"),
            []);

        try
        {
            var discovery = new WindowsMediaFolderDiscovery(
                [seriesDirectory, emptyDirectory]);

            var candidates = await discovery.DiscoverAsync();

            var candidate = Assert.Single(candidates);
            Assert.Equal(
                Path.TrimEndingDirectorySeparator(Path.GetFullPath(seriesDirectory)),
                candidate.Path);
            Assert.Equal(LibraryRootKind.Series, candidate.SuggestedKind);
        }
        finally
        {
            Directory.Delete(testDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task DiscoveryReturnsParentInsteadOfNestedDuplicate()
    {
        var testDirectory = CreateTestDirectory();
        var mediaDirectory = Path.Combine(testDirectory, "Media");
        var moviesDirectory = Path.Combine(mediaDirectory, "Movies");
        Directory.CreateDirectory(moviesDirectory);
        File.WriteAllBytes(Path.Combine(moviesDirectory, "Example.Movie.2026.mp4"), []);

        try
        {
            var discovery = new WindowsMediaFolderDiscovery(
                [moviesDirectory, mediaDirectory]);

            var candidates = await discovery.DiscoverAsync();

            var candidate = Assert.Single(candidates);
            Assert.Equal(
                Path.TrimEndingDirectorySeparator(Path.GetFullPath(mediaDirectory)),
                candidate.Path);
            Assert.Equal(LibraryRootKind.AutoDetect, candidate.SuggestedKind);
        }
        finally
        {
            Directory.Delete(testDirectory, recursive: true);
        }
    }

    private static string CreateTestDirectory()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            "MediaHub.Tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
