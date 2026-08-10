namespace MediaHub.Domain.Media;

public abstract class MediaTitle
{
    public const int MaximumNoteCharacters = 10_000;

    protected MediaTitle(Guid id, string displayTitle, DateTimeOffset createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A media title requires a non-empty identifier.", nameof(id));
        }

        Id = id;
        DisplayTitle = ValidateDisplayTitle(displayTitle);
        SortTitle = DisplayTitle;
        Availability = MediaAvailability.Missing;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; }

    public abstract MediaType MediaType { get; }

    public string DisplayTitle { get; private set; }

    public string SortTitle { get; private set; }

    public int? UserRating { get; private set; }

    public string? UserNote { get; private set; }

    public bool IsFavorite { get; private set; }

    public MediaAvailability Availability { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public long Revision { get; private set; }

    public void Rename(string displayTitle, string? sortTitle, DateTimeOffset changedAtUtc)
    {
        DisplayTitle = ValidateDisplayTitle(displayTitle);
        SortTitle = string.IsNullOrWhiteSpace(sortTitle)
            ? DisplayTitle
            : sortTitle.Trim();
        Touch(changedAtUtc);
    }

    public void SetRating(int stars, DateTimeOffset changedAtUtc)
    {
        if (stars is < 1 or > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(stars), stars, "Rating must be between one and five stars.");
        }

        UserRating = stars;
        Touch(changedAtUtc);
    }

    public void ClearRating(DateTimeOffset changedAtUtc)
    {
        UserRating = null;
        Touch(changedAtUtc);
    }

    public void UpdateNote(string? note, DateTimeOffset changedAtUtc)
    {
        if (note?.Length > MaximumNoteCharacters)
        {
            throw new ArgumentException($"A note cannot exceed {MaximumNoteCharacters} characters.", nameof(note));
        }

        if (note?.Contains('\0') is true)
        {
            throw new ArgumentException("A note cannot contain null characters.", nameof(note));
        }

        UserNote = string.IsNullOrEmpty(note) ? null : note;
        Touch(changedAtUtc);
    }

    public void SetFavorite(bool isFavorite, DateTimeOffset changedAtUtc)
    {
        IsFavorite = isFavorite;
        Touch(changedAtUtc);
    }

    public void SetAvailability(MediaAvailability availability, DateTimeOffset changedAtUtc)
    {
        Availability = availability;
        Touch(changedAtUtc);
    }

    private static string ValidateDisplayTitle(string displayTitle)
    {
        if (string.IsNullOrWhiteSpace(displayTitle))
        {
            throw new ArgumentException("A media title requires a display title.", nameof(displayTitle));
        }

        return displayTitle.Trim();
    }

    private void Touch(DateTimeOffset changedAtUtc)
    {
        if (changedAtUtc < UpdatedAtUtc)
        {
            throw new ArgumentOutOfRangeException(nameof(changedAtUtc), "Changes cannot move the title timestamp backwards.");
        }

        UpdatedAtUtc = changedAtUtc;
        Revision++;
    }
}
