using System.Security;
using MediaHub.Application.Library;

namespace MediaHub.Infrastructure.Local.Library;

public sealed class FileSystemLibraryPathInspector : ILibraryPathInspector
{
    public Task<LibraryPathInspection> InspectAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var fullPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path.Trim()));
            if (!Directory.Exists(fullPath))
            {
                return Task.FromResult(
                    LibraryPathInspection.Failed(LibraryPathFailure.NotFound));
            }

            using var entries = Directory.EnumerateFileSystemEntries(fullPath).GetEnumerator();
            _ = entries.MoveNext();

            var normalizedPath = fullPath.ToUpperInvariant();
            return Task.FromResult(
                LibraryPathInspection.Accessible(fullPath, normalizedPath));
        }
        catch (UnauthorizedAccessException)
        {
            return Task.FromResult(
                LibraryPathInspection.Failed(LibraryPathFailure.AccessDenied));
        }
        catch (SecurityException)
        {
            return Task.FromResult(
                LibraryPathInspection.Failed(LibraryPathFailure.AccessDenied));
        }
        catch (ArgumentException)
        {
            return Task.FromResult(
                LibraryPathInspection.Failed(LibraryPathFailure.InvalidPath));
        }
        catch (NotSupportedException)
        {
            return Task.FromResult(
                LibraryPathInspection.Failed(LibraryPathFailure.InvalidPath));
        }
        catch (PathTooLongException)
        {
            return Task.FromResult(
                LibraryPathInspection.Failed(LibraryPathFailure.InvalidPath));
        }
        catch (IOException)
        {
            return Task.FromResult(
                LibraryPathInspection.Failed(LibraryPathFailure.InputOutputError));
        }
    }
}
