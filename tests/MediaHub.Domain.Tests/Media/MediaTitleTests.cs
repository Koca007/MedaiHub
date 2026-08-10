using MediaHub.Domain.Media;

namespace MediaHub.Domain.Tests.Media;

public sealed class MediaTitleTests
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 8, 10, 6, 0, 0, TimeSpan.Zero);

    [Fact]
    public void FavoriteStateIsIndependentFromRating()
    {
        var movie = new Movie(Guid.NewGuid(), "Arrival", CreatedAt);

        movie.SetRating(5, CreatedAt.AddMinutes(1));
        movie.SetFavorite(true, CreatedAt.AddMinutes(2));
        movie.ClearRating(CreatedAt.AddMinutes(3));

        Assert.Null(movie.UserRating);
        Assert.True(movie.IsFavorite);
    }

    [Fact]
    public void NoteLongerThanLimitIsRejectedWithoutChangingExistingNote()
    {
        var movie = new Movie(Guid.NewGuid(), "Arrival", CreatedAt);
        movie.UpdateNote("Megjegyzés", CreatedAt.AddMinutes(1));

        var tooLong = new string('x', MediaTitle.MaximumNoteCharacters + 1);

        Assert.Throws<ArgumentException>(() => movie.UpdateNote(tooLong, CreatedAt.AddMinutes(2)));
        Assert.Equal("Megjegyzés", movie.UserNote);
    }

    [Fact]
    public void MissingAvailabilityDoesNotRemoveUserState()
    {
        var movie = new Movie(Guid.NewGuid(), "Arrival", CreatedAt);
        movie.SetRating(4, CreatedAt.AddMinutes(1));
        movie.UpdateNote("Keep this", CreatedAt.AddMinutes(2));

        movie.SetAvailability(MediaAvailability.Missing, CreatedAt.AddMinutes(3));

        Assert.Equal(4, movie.UserRating);
        Assert.Equal("Keep this", movie.UserNote);
        Assert.Equal(MediaAvailability.Missing, movie.Availability);
    }

    [Fact]
    public void ChangeTimestampCannotMoveBackwards()
    {
        var movie = new Movie(Guid.NewGuid(), "Arrival", CreatedAt);
        movie.SetFavorite(true, CreatedAt.AddMinutes(2));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => movie.SetRating(5, CreatedAt.AddMinutes(1)));
    }
}
