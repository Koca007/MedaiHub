using MediaHub.Application.Library;
using MediaHub.Application.Scanning;
using MediaHub.Scanner.Parsing;

namespace MediaHub.Scanner.Pipeline;

public sealed class LibraryScanner
{
    private const int BatchSize = 100;
    private readonly IMediaFileRepository _repository;
    private readonly FileStabilityChecker _stabilityChecker;
    private readonly TimeProvider _timeProvider;

    public LibraryScanner(
        IMediaFileRepository repository,
        FileStabilityChecker stabilityChecker,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(stabilityChecker);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _repository = repository;
        _stabilityChecker = stabilityChecker;
        _timeProvider = timeProvider;
    }

    public async Task<ScanOutcome> ScanAsync(
        LibraryRootSummary root,
        ScanControl control,
        IProgress<ScanProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(control);

        var counters = new ScanCounters();
        var batch = new List<DiscoveredMediaFile>(BatchSize);
        Report(progress, ScanPhase.Enumerating, counters, root.Path);

        try
        {
            var rootDirectory = new DirectoryInfo(root.Path);
            rootDirectory.Refresh();
            if (!rootDirectory.Exists)
            {
                return Failed(progress, counters, "root_unavailable");
            }

            var directories = new Stack<DirectoryInfo>();
            directories.Push(rootDirectory);

            while (directories.Count > 0)
            {
                await WaitForResumeAsync(control, progress, counters, cancellationToken)
                    .ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();

                var directory = directories.Pop();
                FileSystemInfo[] entries;
                try
                {
                    entries = directory.GetFileSystemInfos();
                }
                catch (UnauthorizedAccessException)
                {
                    if (PathsEqual(directory.FullName, rootDirectory.FullName))
                    {
                        return Failed(progress, counters, "root_access_denied");
                    }

                    counters.SkippedFiles++;
                    continue;
                }
                catch (IOException)
                {
                    if (PathsEqual(directory.FullName, rootDirectory.FullName))
                    {
                        return Failed(progress, counters, "root_unavailable");
                    }

                    counters.SkippedFiles++;
                    continue;
                }

                foreach (var entry in entries)
                {
                    await WaitForResumeAsync(control, progress, counters, cancellationToken)
                        .ConfigureAwait(false);
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        if (entry is DirectoryInfo childDirectory)
                        {
                            if (!MediaPathRules.ShouldSkipDirectory(childDirectory))
                            {
                                directories.Push(childDirectory);
                            }

                            continue;
                        }

                        if (entry is FileInfo file)
                        {
                            await ProcessFileAsync(
                                    file,
                                    root,
                                    batch,
                                    counters,
                                    progress,
                                    cancellationToken)
                                .ConfigureAwait(false);
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        counters.SkippedFiles++;
                        Report(progress, ScanPhase.Processing, counters, entry.FullName);
                    }
                    catch (IOException)
                    {
                        counters.SkippedFiles++;
                        Report(progress, ScanPhase.Processing, counters, entry.FullName);
                    }
                }
            }

            await CommitBatchAsync(batch, counters, cancellationToken).ConfigureAwait(false);
            Report(progress, ScanPhase.Completed, counters, null);
            return counters.ToOutcome(ScanPhase.Completed, null);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            Report(progress, ScanPhase.Cancelled, counters, null);
            return counters.ToOutcome(ScanPhase.Cancelled, null);
        }
        catch (UnauthorizedAccessException)
        {
            return Failed(progress, counters, "file_access_denied");
        }
        catch (IOException)
        {
            return Failed(progress, counters, "file_system_error");
        }
    }

    private async Task ProcessFileAsync(
        FileInfo file,
        LibraryRootSummary root,
        List<DiscoveredMediaFile> batch,
        ScanCounters counters,
        IProgress<ScanProgress>? progress,
        CancellationToken cancellationToken)
    {
        counters.DiscoveredFiles++;
        if (!MediaPathRules.IsSupportedVideo(file.Name)
            || MediaPathRules.ShouldSkipFile(file))
        {
            counters.SkippedFiles++;
            Report(progress, ScanPhase.Processing, counters, file.FullName);
            return;
        }

        if (!await _stabilityChecker
            .IsStableAsync(file, cancellationToken)
            .ConfigureAwait(false))
        {
            counters.SkippedFiles++;
            Report(progress, ScanPhase.Processing, counters, file.FullName);
            return;
        }

        var parsed = MediaNameParser.Parse(file.FullName, root.Kind);
        batch.Add(
            new DiscoveredMediaFile(
                Guid.NewGuid(),
                root.Id,
                file.FullName,
                NormalizePath(file.FullName),
                file.Length,
                new DateTimeOffset(file.LastWriteTimeUtc),
                parsed.CandidateKind,
                parsed.CandidateTitle,
                parsed.ReleaseYear,
                parsed.SeasonNumber,
                parsed.EpisodeNumbers,
                parsed.Resolution,
                parsed.Source,
                parsed.Codec,
                parsed.Confidence,
                _timeProvider.GetUtcNow()));
        counters.ProcessedFiles++;

        if (batch.Count >= BatchSize)
        {
            await CommitBatchAsync(batch, counters, cancellationToken).ConfigureAwait(false);
        }

        Report(progress, ScanPhase.Processing, counters, file.FullName);
    }

    private static async ValueTask WaitForResumeAsync(
        ScanControl control,
        IProgress<ScanProgress>? progress,
        ScanCounters counters,
        CancellationToken cancellationToken)
    {
        if (control.IsPaused)
        {
            Report(progress, ScanPhase.Paused, counters, null);
        }

        await control.WaitIfPausedAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task CommitBatchAsync(
        List<DiscoveredMediaFile> batch,
        ScanCounters counters,
        CancellationToken cancellationToken)
    {
        if (batch.Count == 0)
        {
            return;
        }

        await _repository.UpsertBatchAsync(batch, cancellationToken).ConfigureAwait(false);
        counters.IndexedFiles += batch.Count;
        batch.Clear();
    }

    private static ScanOutcome Failed(
        IProgress<ScanProgress>? progress,
        ScanCounters counters,
        string errorCode)
    {
        Report(progress, ScanPhase.Failed, counters, null, errorCode);
        return counters.ToOutcome(ScanPhase.Failed, errorCode);
    }

    private static void Report(
        IProgress<ScanProgress>? progress,
        ScanPhase phase,
        ScanCounters counters,
        string? path,
        string? errorCode = null) =>
        progress?.Report(
            new ScanProgress(
                phase,
                counters.DiscoveredFiles,
                counters.ProcessedFiles,
                counters.IndexedFiles,
                counters.SkippedFiles,
                path,
                errorCode));

    private static bool PathsEqual(string left, string right) =>
        string.Equals(
            NormalizePath(left),
            NormalizePath(right),
            StringComparison.OrdinalIgnoreCase);

    private static string NormalizePath(string path) =>
        Path.TrimEndingDirectorySeparator(Path.GetFullPath(path)).ToUpperInvariant();

    private sealed class ScanCounters
    {
        public int DiscoveredFiles { get; set; }

        public int ProcessedFiles { get; set; }

        public int IndexedFiles { get; set; }

        public int SkippedFiles { get; set; }

        public ScanOutcome ToOutcome(ScanPhase phase, string? errorCode) =>
            new(
                phase,
                DiscoveredFiles,
                ProcessedFiles,
                IndexedFiles,
                SkippedFiles,
                errorCode);
    }
}
