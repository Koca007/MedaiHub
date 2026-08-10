namespace MediaHub.Scanner.Pipeline;

public sealed class FileStabilityChecker
{
    private static readonly TimeSpan RecentWriteWindow = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan StabilityDelay = TimeSpan.FromMilliseconds(750);
    private readonly TimeProvider _timeProvider;

    public FileStabilityChecker(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);
        _timeProvider = timeProvider;
    }

    public async Task<bool> IsStableAsync(
        FileInfo file,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        file.Refresh();
        if (!file.Exists)
        {
            return false;
        }

        var initialLength = file.Length;
        var initialWrite = file.LastWriteTimeUtc;
        if (_timeProvider.GetUtcNow() - initialWrite >= RecentWriteWindow)
        {
            return CanOpenForReading(file);
        }

        await Task.Delay(StabilityDelay, _timeProvider, cancellationToken).ConfigureAwait(false);
        file.Refresh();
        return file.Exists
            && file.Length == initialLength
            && file.LastWriteTimeUtc == initialWrite
            && CanOpenForReading(file);
    }

    private static bool CanOpenForReading(FileInfo file)
    {
        try
        {
            using var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            return stream.CanRead;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
    }
}
