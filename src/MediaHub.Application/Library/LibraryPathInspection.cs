namespace MediaHub.Application.Library;

public enum LibraryPathFailure
{
    None = 0,
    InvalidPath = 1,
    NotFound = 2,
    AccessDenied = 3,
    InputOutputError = 4,
}

public sealed record LibraryPathInspection(
    bool IsAccessible,
    string? FullPath,
    string? NormalizedPath,
    LibraryPathFailure Failure)
{
    public static LibraryPathInspection Accessible(string fullPath, string normalizedPath) =>
        new(true, fullPath, normalizedPath, LibraryPathFailure.None);

    public static LibraryPathInspection Failed(LibraryPathFailure failure) =>
        new(false, null, null, failure);
}
