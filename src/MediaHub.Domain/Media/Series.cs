namespace MediaHub.Domain.Media;

public sealed class Series : MediaTitle
{
    public Series(Guid id, string displayTitle, DateTimeOffset createdAtUtc)
        : base(id, displayTitle, createdAtUtc)
    {
    }

    public override MediaType MediaType => MediaType.Series;
}
