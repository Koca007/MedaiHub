namespace MediaHub.Application.Library;

public interface ILibraryPathInspector
{
    Task<LibraryPathInspection> InspectAsync(
        string path,
        CancellationToken cancellationToken = default);
}
