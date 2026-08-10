using MediaHub.Application.Scanning;

namespace MediaHub.Scanner.Pipeline;

public static class MediaPathRules
{
    private static readonly HashSet<string> IgnoredDirectoryNames =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "$RECYCLE.BIN",
            "System Volume Information",
            "sample",
            "samples",
            "extras",
            "featurettes",
            "deleted scenes",
            "behind the scenes",
        };

    private static readonly string[] IncompleteSuffixes =
    [
        ".part",
        ".partial",
        ".tmp",
        ".crdownload",
        ".download",
    ];

    public static bool IsSupportedVideo(string path) =>
        SupportedMediaFiles.IsVideo(path);

    public static bool ShouldSkipDirectory(DirectoryInfo directory)
    {
        ArgumentNullException.ThrowIfNull(directory);
        if (IgnoredDirectoryNames.Contains(directory.Name))
        {
            return true;
        }

        var attributes = directory.Attributes;
        return attributes.HasFlag(FileAttributes.ReparsePoint)
            || attributes.HasFlag(FileAttributes.System)
            || attributes.HasFlag(FileAttributes.Hidden);
    }

    public static bool ShouldSkipFile(FileInfo file)
    {
        ArgumentNullException.ThrowIfNull(file);
        var attributes = file.Attributes;
        if (attributes.HasFlag(FileAttributes.System)
            || attributes.HasFlag(FileAttributes.Hidden))
        {
            return true;
        }

        return IncompleteSuffixes.Any(
            suffix => file.Name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
    }
}
