namespace MediaHub.Domain.Media;

public sealed class Movie : MediaTitle
{
    public Movie(Guid id, string displayTitle, DateTimeOffset createdAtUtc)
        : base(id, displayTitle, createdAtUtc)
    {
    }

    public override MediaType MediaType => MediaType.Movie;
}
