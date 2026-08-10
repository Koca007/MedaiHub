namespace MediaHub.Domain.Library;

public sealed class LibraryRoot
{
    private LibraryRoot(
        Guid id,
        string path,
        string normalizedPath,
        LibraryRootKind kind,
        LibraryRootAvailability availability,
        bool isEnabled,
        DateTimeOffset createdAtUtc,
        DateTimeOffset lastCheckedAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A library root requires a non-empty identifier.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("A library root requires a path.", nameof(path));
        }

        if (string.IsNullOrWhiteSpace(normalizedPath))
        {
            throw new ArgumentException("A library root requires a normalized path.", nameof(normalizedPath));
        }

        if (lastCheckedAtUtc < createdAtUtc)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lastCheckedAtUtc),
                "The last check cannot predate creation.");
        }

        Id = id;
        Path = path.Trim();
        NormalizedPath = normalizedPath.Trim();
        Kind = kind;
        Availability = availability;
        IsEnabled = isEnabled;
        CreatedAtUtc = createdAtUtc;
        LastCheckedAtUtc = lastCheckedAtUtc;
    }

    public Guid Id { get; }

    public string Path { get; }

    public string NormalizedPath { get; }

    public LibraryRootKind Kind { get; private set; }

    public LibraryRootAvailability Availability { get; private set; }

    public bool IsEnabled { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; }

    public DateTimeOffset LastCheckedAtUtc { get; private set; }

    public static LibraryRoot Create(
        Guid id,
        string path,
        string normalizedPath,
        LibraryRootKind kind,
        DateTimeOffset createdAtUtc) =>
        new(
            id,
            path,
            normalizedPath,
            kind,
            LibraryRootAvailability.Available,
            true,
            createdAtUtc,
            createdAtUtc);

    public static LibraryRoot Restore(
        Guid id,
        string path,
        string normalizedPath,
        LibraryRootKind kind,
        LibraryRootAvailability availability,
        bool isEnabled,
        DateTimeOffset createdAtUtc,
        DateTimeOffset lastCheckedAtUtc) =>
        new(
            id,
            path,
            normalizedPath,
            kind,
            availability,
            isEnabled,
            createdAtUtc,
            lastCheckedAtUtc);

    public void ChangeKind(LibraryRootKind kind) => Kind = kind;

    public void SetEnabled(bool isEnabled) => IsEnabled = isEnabled;

    public void SetAvailability(
        LibraryRootAvailability availability,
        DateTimeOffset checkedAtUtc)
    {
        if (checkedAtUtc < LastCheckedAtUtc)
        {
            throw new ArgumentOutOfRangeException(
                nameof(checkedAtUtc),
                "Checks cannot move the timestamp backwards.");
        }

        Availability = availability;
        LastCheckedAtUtc = checkedAtUtc;
    }
}
