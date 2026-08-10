using MediaHub.Domain.Library;
using MediaHub.Infrastructure.Local;
using MediaHub.Infrastructure.Local.Library;
using MediaHub.Infrastructure.Local.Persistence;
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
}
