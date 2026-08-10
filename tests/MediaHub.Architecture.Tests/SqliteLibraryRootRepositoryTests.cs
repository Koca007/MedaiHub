using MediaHub.Application.Scanning;
using MediaHub.Domain.Library;
using MediaHub.Infrastructure.Local;
using MediaHub.Infrastructure.Local.Library;
using MediaHub.Infrastructure.Local.Persistence;
using MediaHub.Infrastructure.Local.Scanning;
using Microsoft.Data.Sqlite;

namespace MediaHub.Architecture.Tests;

public sealed class SqliteLibraryRootRepositoryTests
{
    [Fact]
    public async Task RepositoryPersistsAndReloadsLibraryRoot()
    {
        var testDirectory = Path.Combine(
            Path.GetTempPath(),
            "MediaHub.Tests",
            Guid.NewGuid().ToString("N"));
        var dataDirectory = Path.Combine(testDirectory, "Data");
        var paths = new LocalDataPaths(
            dataDirectory,
            Path.Combine(dataDirectory, "mediahub.db"),
            Path.Combine(testDirectory, "Logs"),
            Path.Combine(testDirectory, "Cache"));

        try
        {
            using var database = new LocalDatabase(paths);
            await database.InitializeAsync();
            var repository = new SqliteLibraryRootRepository(database);
            var createdAt = new DateTimeOffset(2026, 8, 10, 12, 0, 0, TimeSpan.Zero);
            var expected = LibraryRoot.Create(
                Guid.NewGuid(),
                @"D:\Media",
                @"D:\MEDIA",
                LibraryRootKind.Mixed,
                createdAt);

            await repository.AddAsync(expected);

            var reloaded = await repository.FindByNormalizedPathAsync(@"d:\media");
            Assert.NotNull(reloaded);
            Assert.Equal(expected.Id, reloaded.Id);
            Assert.Equal(expected.Path, reloaded.Path);
            Assert.Equal(expected.Kind, reloaded.Kind);
            Assert.Equal(expected.CreatedAtUtc, reloaded.CreatedAtUtc);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, recursive: true);
            }
        }
    }

    [Fact]
    public async Task PathInspectorAcceptsReadableDirectory()
    {
        var testDirectory = Path.Combine(
            Path.GetTempPath(),
            "MediaHub.Tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(testDirectory);

        try
        {
            var inspector = new FileSystemLibraryPathInspector();

            var result = await inspector.InspectAsync(testDirectory);

            Assert.True(result.IsAccessible);
            Assert.Equal(
                Path.TrimEndingDirectorySeparator(Path.GetFullPath(testDirectory)),
                result.FullPath);
        }
        finally
        {
            Directory.Delete(testDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task MediaRepositoryUpsertIsIdempotentAndPreservesIdentity()
    {
        var testDirectory = Path.Combine(
            Path.GetTempPath(),
            "MediaHub.Tests",
            Guid.NewGuid().ToString("N"));
        var dataDirectory = Path.Combine(testDirectory, "Data");
        var paths = new LocalDataPaths(
            dataDirectory,
            Path.Combine(dataDirectory, "mediahub.db"),
            Path.Combine(testDirectory, "Logs"),
            Path.Combine(testDirectory, "Cache"));

        try
        {
            using var database = new LocalDatabase(paths);
            await database.InitializeAsync();
            var rootRepository = new SqliteLibraryRootRepository(database);
            var mediaRepository = new SqliteMediaFileRepository(database);
            var now = new DateTimeOffset(2026, 8, 10, 12, 0, 0, TimeSpan.Zero);
            var root = LibraryRoot.Create(
                Guid.NewGuid(),
                @"D:\Media",
                @"D:\MEDIA",
                LibraryRootKind.Movies,
                now);
            await rootRepository.AddAsync(root);

            var originalId = Guid.NewGuid();
            var normalizedPath = @"D:\MEDIA\MOVIE.2026.MKV";
            await mediaRepository.UpsertBatchAsync(
                [
                    CreateMediaFile(
                        originalId,
                        root.Id,
                        normalizedPath,
                        "Movie",
                        now),
                ]);
            await mediaRepository.UpsertBatchAsync(
                [
                    CreateMediaFile(
                        Guid.NewGuid(),
                        root.Id,
                        normalizedPath.ToLowerInvariant(),
                        "Movie Remastered",
                        now.AddMinutes(1)),
                ]);

            Assert.Equal(1, await mediaRepository.CountByRootAsync(root.Id));
            var reloaded = Assert.Single(await mediaRepository.GetRecentAsync(10));
            Assert.Equal(originalId, reloaded.Id);
            Assert.Equal("Movie Remastered", reloaded.CandidateTitle);
            Assert.Equal(now.AddMinutes(1), reloaded.LastSeenAtUtc);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, recursive: true);
            }
        }
    }

    private static DiscoveredMediaFile CreateMediaFile(
        Guid id,
        Guid rootId,
        string normalizedPath,
        string title,
        DateTimeOffset seenAtUtc) =>
        new(
            id,
            rootId,
            @"D:\Media\Movie.2026.mkv",
            normalizedPath,
            1024,
            seenAtUtc,
            MediaCandidateKind.Movie,
            title,
            2026,
            null,
            [],
            "1080p",
            "BLURAY",
            "H.265",
            0.82,
            seenAtUtc);
}
