using System.Globalization;
using MediaHub.Application.Scanning;
using MediaHub.Infrastructure.Local.Persistence;

namespace MediaHub.Infrastructure.Local.Scanning;

public sealed class SqliteMediaFileRepository : IMediaFileRepository
{
    private readonly LocalDatabase _database;

    public SqliteMediaFileRepository(LocalDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);
        _database = database;
    }

    public async Task UpsertBatchAsync(
        IReadOnlyCollection<DiscoveredMediaFile> mediaFiles,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mediaFiles);
        if (mediaFiles.Count == 0)
        {
            return;
        }

        await using var connection = await _database
            .OpenConnectionAsync(cancellationToken)
            .ConfigureAwait(false);
        await using var transaction = await connection
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var mediaFile in mediaFiles)
        {
            await using var command = connection.CreateCommand();
            command.Transaction = (Microsoft.Data.Sqlite.SqliteTransaction)transaction;
            command.CommandText =
                """
                INSERT INTO media_files(
                    id,
                    library_root_id,
                    original_path,
                    normalized_path,
                    file_size_bytes,
                    modified_at_utc,
                    candidate_kind,
                    candidate_title,
                    release_year,
                    season_number,
                    episode_numbers,
                    resolution,
                    source,
                    codec,
                    confidence,
                    last_seen_at_utc)
                VALUES (
                    $id,
                    $libraryRootId,
                    $originalPath,
                    $normalizedPath,
                    $fileSizeBytes,
                    $modifiedAtUtc,
                    $candidateKind,
                    $candidateTitle,
                    $releaseYear,
                    $seasonNumber,
                    $episodeNumbers,
                    $resolution,
                    $source,
                    $codec,
                    $confidence,
                    $lastSeenAtUtc)
                ON CONFLICT(normalized_path) DO UPDATE SET
                    library_root_id = excluded.library_root_id,
                    original_path = excluded.original_path,
                    file_size_bytes = excluded.file_size_bytes,
                    modified_at_utc = excluded.modified_at_utc,
                    candidate_kind = excluded.candidate_kind,
                    candidate_title = excluded.candidate_title,
                    release_year = excluded.release_year,
                    season_number = excluded.season_number,
                    episode_numbers = excluded.episode_numbers,
                    resolution = excluded.resolution,
                    source = excluded.source,
                    codec = excluded.codec,
                    confidence = excluded.confidence,
                    last_seen_at_utc = excluded.last_seen_at_utc;
                """;
            AddParameters(command, mediaFile);
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<MediaFileSummary>> GetRecentAsync(
        int maximumCount,
        CancellationToken cancellationToken = default)
    {
        if (maximumCount is < 1 or > 500)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumCount),
                "The result limit must be between one and 500.");
        }

        await using var connection = await _database
            .OpenConnectionAsync(cancellationToken)
            .ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT
                id,
                library_root_id,
                original_path,
                candidate_kind,
                candidate_title,
                release_year,
                season_number,
                episode_numbers,
                resolution,
                source,
                codec,
                confidence,
                last_seen_at_utc
            FROM media_files
            ORDER BY last_seen_at_utc DESC, candidate_title COLLATE NOCASE
            LIMIT $maximumCount;
            """;
        command.Parameters.AddWithValue("$maximumCount", maximumCount);

        var results = new List<MediaFileSummary>();
        await using var reader = await command
            .ExecuteReaderAsync(cancellationToken)
            .ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(ReadSummary(reader));
        }

        return results;
    }

    public async Task<int> CountByRootAsync(
        Guid libraryRootId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _database
            .OpenConnectionAsync(cancellationToken)
            .ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT COUNT(*)
            FROM media_files
            WHERE library_root_id = $libraryRootId;
            """;
        command.Parameters.AddWithValue("$libraryRootId", libraryRootId.ToString("D"));
        var count = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return Convert.ToInt32(count, CultureInfo.InvariantCulture);
    }

    private static void AddParameters(
        Microsoft.Data.Sqlite.SqliteCommand command,
        DiscoveredMediaFile mediaFile)
    {
        command.Parameters.AddWithValue("$id", mediaFile.Id.ToString("D"));
        command.Parameters.AddWithValue("$libraryRootId", mediaFile.LibraryRootId.ToString("D"));
        command.Parameters.AddWithValue("$originalPath", mediaFile.OriginalPath);
        command.Parameters.AddWithValue("$normalizedPath", mediaFile.NormalizedPath);
        command.Parameters.AddWithValue("$fileSizeBytes", mediaFile.FileSizeBytes);
        command.Parameters.AddWithValue(
            "$modifiedAtUtc",
            mediaFile.ModifiedAtUtc.ToString("O", CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("$candidateKind", (int)mediaFile.CandidateKind);
        command.Parameters.AddWithValue("$candidateTitle", mediaFile.CandidateTitle);
        command.Parameters.AddWithValue(
            "$releaseYear",
            (object?)mediaFile.ReleaseYear ?? DBNull.Value);
        command.Parameters.AddWithValue(
            "$seasonNumber",
            (object?)mediaFile.SeasonNumber ?? DBNull.Value);
        command.Parameters.AddWithValue(
            "$episodeNumbers",
            string.Join(',', mediaFile.EpisodeNumbers));
        command.Parameters.AddWithValue(
            "$resolution",
            (object?)mediaFile.Resolution ?? DBNull.Value);
        command.Parameters.AddWithValue("$source", (object?)mediaFile.Source ?? DBNull.Value);
        command.Parameters.AddWithValue("$codec", (object?)mediaFile.Codec ?? DBNull.Value);
        command.Parameters.AddWithValue("$confidence", mediaFile.Confidence);
        command.Parameters.AddWithValue(
            "$lastSeenAtUtc",
            mediaFile.LastSeenAtUtc.ToString("O", CultureInfo.InvariantCulture));
    }

    private static MediaFileSummary ReadSummary(Microsoft.Data.Sqlite.SqliteDataReader reader) =>
        new(
            Guid.Parse(reader.GetString(0)),
            Guid.Parse(reader.GetString(1)),
            reader.GetString(2),
            (MediaCandidateKind)reader.GetInt32(3),
            reader.GetString(4),
            reader.IsDBNull(5) ? null : reader.GetInt32(5),
            reader.IsDBNull(6) ? null : reader.GetInt32(6),
            ParseEpisodeNumbers(reader.GetString(7)),
            reader.IsDBNull(8) ? null : reader.GetString(8),
            reader.IsDBNull(9) ? null : reader.GetString(9),
            reader.IsDBNull(10) ? null : reader.GetString(10),
            reader.GetDouble(11),
            DateTimeOffset.Parse(
                reader.GetString(12),
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind));

    private static int[] ParseEpisodeNumbers(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(
                    item => int.Parse(
                        item,
                        NumberStyles.None,
                        CultureInfo.InvariantCulture))
                .ToArray();
}
