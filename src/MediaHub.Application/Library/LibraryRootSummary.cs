using MediaHub.Domain.Library;

namespace MediaHub.Application.Library;

public sealed record LibraryRootSummary(
    Guid Id,
    string Path,
    LibraryRootKind Kind,
    LibraryRootAvailability Availability,
    bool IsEnabled,
    DateTimeOffset LastCheckedAtUtc);

public enum AddLibraryRootFailure
{
    None = 0,
    EmptyPath = 1,
    InvalidPath = 2,
    NotFound = 3,
    AccessDenied = 4,
    InputOutputError = 5,
    Duplicate = 6,
}

public sealed record AddLibraryRootResult(
    bool IsSuccess,
    LibraryRootSummary? Root,
    AddLibraryRootFailure Failure)
{
    public static AddLibraryRootResult Added(LibraryRootSummary root) =>
        new(true, root, AddLibraryRootFailure.None);

    public static AddLibraryRootResult Rejected(AddLibraryRootFailure failure) =>
        new(false, null, failure);
}
