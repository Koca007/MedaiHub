using MediaHub.Domain.Library;

namespace MediaHub.Domain.Tests.Library;

public sealed class LibraryRootTests
{
    private static readonly DateTimeOffset CreatedAt =
        new(2026, 8, 10, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateInitializesAvailableEnabledRoot()
    {
        var id = Guid.NewGuid();

        var root = LibraryRoot.Create(
            id,
            @"D:\Media",
            @"D:\MEDIA",
            LibraryRootKind.Mixed,
            CreatedAt);

        Assert.Equal(id, root.Id);
        Assert.Equal(LibraryRootAvailability.Available, root.Availability);
        Assert.True(root.IsEnabled);
        Assert.Equal(CreatedAt, root.LastCheckedAtUtc);
    }

    [Fact]
    public void CreateRejectsEmptyPath()
    {
        Assert.Throws<ArgumentException>(
            () => LibraryRoot.Create(
                Guid.NewGuid(),
                " ",
                @"D:\MEDIA",
                LibraryRootKind.AutoDetect,
                CreatedAt));
    }

    [Fact]
    public void AvailabilityCheckCannotMoveBackwards()
    {
        var root = LibraryRoot.Create(
            Guid.NewGuid(),
            @"D:\Media",
            @"D:\MEDIA",
            LibraryRootKind.Movies,
            CreatedAt);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => root.SetAvailability(
                LibraryRootAvailability.Unavailable,
                CreatedAt.AddSeconds(-1)));
    }
}
