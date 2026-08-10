using MediaHub.Application.Library;
using MediaHub.Application.Scanning;
using MediaHub.Domain.Library;

namespace MediaHub.Infrastructure.Local.Library;

public sealed class WindowsMediaFolderDiscovery : IAutomaticLibraryRootDiscovery
{
    private const int DirectoryProbeBudget = 500;
    private static readonly string[] CommonMediaFolderNames =
    [
        "Videos",
        "Video",
        "Movies",
        "Movie",
        "Films",
        "Filmek",
        "Series",
        "TV Shows",
        "TV",
        "Sorozatok",
        "Media",
    ];

    private static readonly HashSet<string> IgnoredTopLevelNames =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "$Recycle.Bin",
            "Windows",
            "Program Files",
            "Program Files (x86)",
            "ProgramData",
            "Recovery",
            "System Volume Information",
            "Users",
        };

    private readonly IReadOnlyList<string>? _explicitSearchLocations;

    public WindowsMediaFolderDiscovery()
    {
    }

    public WindowsMediaFolderDiscovery(IEnumerable<string> searchLocations)
    {
        ArgumentNullException.ThrowIfNull(searchLocations);
        _explicitSearchLocations = searchLocations
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(Path.GetFullPath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public Task<IReadOnlyList<AutomaticLibraryRootCandidate>> DiscoverAsync(
        CancellationToken cancellationToken = default) =>
        Task.Run<IReadOnlyList<AutomaticLibraryRootCandidate>>(
            () => Discover(cancellationToken),
            cancellationToken);

    private List<AutomaticLibraryRootCandidate> Discover(
        CancellationToken cancellationToken)
    {
        var probes = _explicitSearchLocations is not null
            ? _explicitSearchLocations.Select(path => new FolderProbe(path, 4)).ToArray()
            : BuildSystemProbes(cancellationToken);
        var discovered = new List<AutomaticLibraryRootCandidate>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var probe in probes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var normalizedPath = NormalizeExistingDirectory(probe.Path);
            if (normalizedPath is null
                || !seen.Add(normalizedPath)
                || !ContainsVideo(normalizedPath, probe.MaximumDepth, cancellationToken))
            {
                continue;
            }

            discovered.Add(
                new AutomaticLibraryRootCandidate(
                    normalizedPath,
                    InferKind(normalizedPath)));
        }

        return RemoveNestedCandidates(discovered);
    }

    private static FolderProbe[] BuildSystemProbes(CancellationToken cancellationToken)
    {
        var probes = new List<FolderProbe>();
        AddStandardLocations(probes);

        foreach (var drive in DriveInfo.GetDrives())
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                if (!drive.IsReady
                    || drive.DriveType is not (DriveType.Fixed or DriveType.Removable))
                {
                    continue;
                }

                foreach (var folderName in CommonMediaFolderNames)
                {
                    probes.Add(new FolderProbe(Path.Combine(drive.RootDirectory.FullName, folderName), 4));
                }

                foreach (var directory in drive.RootDirectory.EnumerateDirectories())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!IgnoredTopLevelNames.Contains(directory.Name)
                        && !IsHiddenSystemOrLink(directory))
                    {
                        probes.Add(new FolderProbe(directory.FullName, 0));
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (IOException)
            {
            }
        }

        return probes.ToArray();
    }

    private static void AddStandardLocations(List<FolderProbe> probes)
    {
        AddIfPresent(
            probes,
            Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
            4);
        AddIfPresent(
            probes,
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
            1);

        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrWhiteSpace(userProfile))
        {
            AddIfPresent(probes, Path.Combine(userProfile, "Downloads"), 1);
            foreach (var folderName in CommonMediaFolderNames)
            {
                AddIfPresent(probes, Path.Combine(userProfile, folderName), 4);
            }
        }

        var oneDrive = Environment.GetEnvironmentVariable("OneDrive");
        if (!string.IsNullOrWhiteSpace(oneDrive))
        {
            foreach (var folderName in CommonMediaFolderNames)
            {
                AddIfPresent(probes, Path.Combine(oneDrive, folderName), 4);
            }
        }
    }

    private static void AddIfPresent(List<FolderProbe> probes, string path, int maximumDepth)
    {
        if (!string.IsNullOrWhiteSpace(path))
        {
            probes.Add(new FolderProbe(path, maximumDepth));
        }
    }

    private static bool ContainsVideo(
        string rootPath,
        int maximumDepth,
        CancellationToken cancellationToken)
    {
        var pending = new Stack<(DirectoryInfo Directory, int Depth)>();
        pending.Push((new DirectoryInfo(rootPath), 0));
        var visitedDirectories = 0;

        while (pending.Count > 0 && visitedDirectories < DirectoryProbeBudget)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var (directory, depth) = pending.Pop();
            visitedDirectories++;

            try
            {
                if (directory.EnumerateFiles().Any(file => SupportedMediaFiles.IsVideo(file.Name)))
                {
                    return true;
                }

                if (depth >= maximumDepth)
                {
                    continue;
                }

                foreach (var child in directory.EnumerateDirectories())
                {
                    if (!IsHiddenSystemOrLink(child))
                    {
                        pending.Push((child, depth + 1));
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (IOException)
            {
            }
        }

        return false;
    }

    private static bool IsHiddenSystemOrLink(DirectoryInfo directory)
    {
        var attributes = directory.Attributes;
        return attributes.HasFlag(FileAttributes.Hidden)
            || attributes.HasFlag(FileAttributes.System)
            || attributes.HasFlag(FileAttributes.ReparsePoint);
    }

    private static string? NormalizeExistingDirectory(string path)
    {
        try
        {
            var fullPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
            return Directory.Exists(fullPath) ? fullPath : null;
        }
        catch (ArgumentException)
        {
            return null;
        }
        catch (NotSupportedException)
        {
            return null;
        }
        catch (PathTooLongException)
        {
            return null;
        }
    }

    private static LibraryRootKind InferKind(string path)
    {
        var name = new DirectoryInfo(path).Name;
        if (name.Contains("series", StringComparison.OrdinalIgnoreCase)
            || name.Contains("tv", StringComparison.OrdinalIgnoreCase)
            || name.Contains("show", StringComparison.OrdinalIgnoreCase)
            || name.Contains("sorozat", StringComparison.OrdinalIgnoreCase))
        {
            return LibraryRootKind.Series;
        }

        if (name.Contains("movie", StringComparison.OrdinalIgnoreCase)
            || name.Contains("film", StringComparison.OrdinalIgnoreCase))
        {
            return LibraryRootKind.Movies;
        }

        return LibraryRootKind.AutoDetect;
    }

    private static List<AutomaticLibraryRootCandidate> RemoveNestedCandidates(
        IReadOnlyList<AutomaticLibraryRootCandidate> candidates)
    {
        var selected = new List<AutomaticLibraryRootCandidate>();
        foreach (var candidate in candidates.OrderBy(item => item.Path.Length))
        {
            if (selected.Any(parent => IsWithin(candidate.Path, parent.Path)))
            {
                continue;
            }

            selected.Add(candidate);
        }

        return selected;
    }

    private static bool IsWithin(string path, string potentialParent)
    {
        var relative = Path.GetRelativePath(potentialParent, path);
        return relative == "."
            || (!relative.StartsWith("..", StringComparison.Ordinal)
                && !Path.IsPathRooted(relative));
    }

    private sealed record FolderProbe(string Path, int MaximumDepth);
}
