namespace MediaHub.Infrastructure.Local;

public sealed record LocalDataPaths(
    string DataDirectory,
    string DatabasePath,
    string LogsDirectory,
    string CacheDirectory)
{
    public static LocalDataPaths CreateDefault()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var root = Path.Combine(localAppData, "MediaHub");

        return new LocalDataPaths(
            Path.Combine(root, "Data"),
            Path.Combine(root, "Data", "mediahub.db"),
            Path.Combine(root, "Logs"),
            Path.Combine(root, "Cache"));
    }
}
