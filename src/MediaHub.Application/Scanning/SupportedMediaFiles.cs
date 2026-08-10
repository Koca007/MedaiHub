namespace MediaHub.Application.Scanning;

public static class SupportedMediaFiles
{
    private static readonly HashSet<string> VideoExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".mkv",
            ".mp4",
            ".m4v",
            ".avi",
            ".mov",
            ".webm",
            ".ts",
            ".m2ts",
        };

    public static bool IsVideo(string path) =>
        VideoExtensions.Contains(Path.GetExtension(path));
}
