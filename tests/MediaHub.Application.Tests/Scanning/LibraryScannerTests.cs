using MediaHub.Application.Library;
using MediaHub.Application.Scanning;
using MediaHub.Domain.Library;
using MediaHub.Scanner.Pipeline;

namespace MediaHub.Application.Tests.Scanning;

public sealed class LibraryScannerTests
{
    [Fact]
    public async Task ScanIndexesSupportedStableFilesAndSkipsNoise()
    {
        var testDirectory = CreateTestDirectory();
        try
        {
            CreateOldFile(testDirectory, "Breaking.Bad.S01E02.1080p.mkv");
            CreateOldFile(testDirectory, "notes.txt");
            CreateOldFile(testDirectory, "unfinished.mkv.part");
            var sampleDirectory = Directory.CreateDirectory(Path.Combine(testDirectory, "sample"));
            CreateOldFile(sampleDirectory.FullName, "Sample.Movie.2026.mkv");

            var repository = new InMemoryMediaFileRepository();
            var scanner = CreateScanner(repository);

            var outcome = await scanner.ScanAsync(
                CreateRoot(testDirectory, LibraryRootKind.Series),
                new ScanControl());

            Assert.Equal(ScanPhase.Completed, outcome.Phase);
            Assert.Equal(3, outcome.DiscoveredFiles);
            Assert.Equal(1, outcome.ProcessedFiles);
            Assert.Equal(1, outcome.IndexedFiles);
            Assert.Equal(2, outcome.SkippedFiles);
            var indexed = Assert.Single(repository.Files);
            Assert.Equal(MediaCandidateKind.Episode, indexed.CandidateKind);
            Assert.Equal("Breaking Bad", indexed.CandidateTitle);
        }
        finally
        {
            Directory.Delete(testDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task CancellationPreservesPreviouslyCommittedBatch()
    {
        var testDirectory = CreateTestDirectory();
        try
        {
            for (var index = 0; index < 101; index++)
            {
                CreateOldFile(testDirectory, $"Movie.{index:D3}.2026.mkv");
            }

            var repository = new InMemoryMediaFileRepository();
            var scanner = CreateScanner(repository);
            using var cancellation = new CancellationTokenSource();
            var progress = new SynchronousProgress<ScanProgress>(
                update =>
                {
                    if (update.IndexedFiles >= 100)
                    {
                        cancellation.Cancel();
                    }
                });

            var outcome = await scanner.ScanAsync(
                CreateRoot(testDirectory, LibraryRootKind.Movies),
                new ScanControl(),
                progress,
                cancellation.Token);

            Assert.Equal(ScanPhase.Cancelled, outcome.Phase);
            Assert.Equal(100, outcome.IndexedFiles);
            Assert.Equal(100, repository.Files.Count);
        }
        finally
        {
            Directory.Delete(testDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task PauseBlocksUntilResume()
    {
        var control = new ScanControl();
        control.Pause();

        var waitTask = control.WaitIfPausedAsync(CancellationToken.None).AsTask();
        Assert.False(waitTask.IsCompleted);

        control.Resume();
        await waitTask.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.True(waitTask.IsCompletedSuccessfully);
    }

    [Fact]
    public async Task MissingRootFailsWithoutDeletingPersistedMedia()
    {
        var repository = new InMemoryMediaFileRepository();
        repository.Files.Add(
            new DiscoveredMediaFile(
                Guid.NewGuid(),
                Guid.NewGuid(),
                @"D:\Media\Existing.mkv",
                @"D:\MEDIA\EXISTING.MKV",
                0,
                DateTimeOffset.UtcNow,
                MediaCandidateKind.Movie,
                "Existing",
                null,
                null,
                [],
                null,
                null,
                null,
                0.5,
                DateTimeOffset.UtcNow));
        var missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        var outcome = await CreateScanner(repository).ScanAsync(
            CreateRoot(missingPath, LibraryRootKind.Mixed),
            new ScanControl());

        Assert.Equal(ScanPhase.Failed, outcome.Phase);
        Assert.Equal("root_unavailable", outcome.ErrorCode);
        Assert.Single(repository.Files);
    }

    private static LibraryScanner CreateScanner(IMediaFileRepository repository) =>
        new(repository, new FileStabilityChecker(TimeProvider.System), TimeProvider.System);

    private static LibraryRootSummary CreateRoot(string path, LibraryRootKind kind) =>
        new(
            Guid.NewGuid(),
            path,
            kind,
            LibraryRootAvailability.Available,
            true,
            DateTimeOffset.UtcNow);

    private static string CreateTestDirectory()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            "MediaHub.Tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static void CreateOldFile(string directory, string fileName)
    {
        var path = Path.Combine(directory, fileName);
        File.WriteAllBytes(path, []);
        File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddMinutes(-1));
    }

    private sealed class SynchronousProgress<T>(Action<T> report) : IProgress<T>
    {
        public void Report(T value) => report(value);
    }

    private sealed class InMemoryMediaFileRepository : IMediaFileRepository
    {
        public List<DiscoveredMediaFile> Files { get; } = [];

        public Task UpsertBatchAsync(
            IReadOnlyCollection<DiscoveredMediaFile> mediaFiles,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var mediaFile in mediaFiles)
            {
                var existing = Files.FindIndex(
                    item => item.NormalizedPath == mediaFile.NormalizedPath);
                if (existing >= 0)
                {
                    Files[existing] = mediaFile;
                }
                else
                {
                    Files.Add(mediaFile);
                }
            }

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<MediaFileSummary>> GetRecentAsync(
            int maximumCount,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<MediaFileSummary>>([]);

        public Task<int> CountByRootAsync(
            Guid libraryRootId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Files.Count(file => file.LibraryRootId == libraryRootId));
    }
}
