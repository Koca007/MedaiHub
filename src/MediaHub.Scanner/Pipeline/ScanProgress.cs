namespace MediaHub.Scanner.Pipeline;

public enum ScanPhase
{
    Enumerating = 0,
    Processing = 1,
    Paused = 2,
    Completed = 3,
    Cancelled = 4,
    Failed = 5,
}

public sealed record ScanProgress(
    ScanPhase Phase,
    int DiscoveredFiles,
    int ProcessedFiles,
    int IndexedFiles,
    int SkippedFiles,
    string? CurrentPath,
    string? ErrorCode);

public sealed record ScanOutcome(
    ScanPhase Phase,
    int DiscoveredFiles,
    int ProcessedFiles,
    int IndexedFiles,
    int SkippedFiles,
    string? ErrorCode);
