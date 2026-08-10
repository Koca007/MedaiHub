using System.Globalization;
using MediaHub.Application.Scanning;

namespace MediaHub.App.ViewModels;

public sealed record MediaFileItemViewModel(
    Guid Id,
    MediaCandidateKind CandidateKind,
    string Title,
    string ContextLabel,
    string TechnicalLabel,
    string Path)
{
    public static MediaFileItemViewModel FromSummary(MediaFileSummary mediaFile)
    {
        var context = mediaFile.CandidateKind switch
        {
            MediaCandidateKind.Episode => EpisodeContext(mediaFile),
            MediaCandidateKind.Movie when mediaFile.ReleaseYear is not null =>
                mediaFile.ReleaseYear.Value.ToString(CultureInfo.InvariantCulture),
            MediaCandidateKind.Movie => "Filmjel\u00f6lt",
            _ => "Ellen\u0151rz\u00e9st ig\u00e9nyel",
        };

        var technicalTokens = new[]
        {
            mediaFile.Resolution,
            mediaFile.Source,
            mediaFile.Codec,
            $"{mediaFile.Confidence:P0}",
        }.Where(value => !string.IsNullOrWhiteSpace(value));

        return new MediaFileItemViewModel(
            mediaFile.Id,
            mediaFile.CandidateKind,
            mediaFile.CandidateTitle,
            context,
            string.Join("  /  ", technicalTokens),
            mediaFile.OriginalPath);
    }

    private static string EpisodeContext(MediaFileSummary mediaFile)
    {
        var season = mediaFile.SeasonNumber is null
            ? string.Empty
            : $"S{mediaFile.SeasonNumber:00}";
        var episodes = mediaFile.EpisodeNumbers.Count switch
        {
            0 => string.Empty,
            1 => $"E{mediaFile.EpisodeNumbers[0]:00}",
            _ => $"E{mediaFile.EpisodeNumbers[0]:00}-E{mediaFile.EpisodeNumbers[^1]:00}",
        };
        var firstEpisode = mediaFile.EpisodeNumbers.Count > 0
            ? mediaFile.EpisodeNumbers[0]
            : 0;
        return string.IsNullOrEmpty(season)
            ? $"Abszol\u00fat epiz\u00f3d {firstEpisode}"
            : $"{season}{episodes}";
    }
}
