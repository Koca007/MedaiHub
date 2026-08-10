using System.Globalization;
using MediaHub.Application.Library;
using MediaHub.Domain.Library;
using MediaHub.Infrastructure.Local.Persistence;

namespace MediaHub.Infrastructure.Local.Library;

public sealed class SqliteLibraryRootRepository : ILibraryRootRepository
{
    private readonly LocalDatabase _database;

    public SqliteLibraryRootRepository(LocalDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);
        _database = database;
    }

    public async Task<IReadOnlyList<LibraryRoot>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _database
            .OpenConnectionAsync(cancellationToken)
            .ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT
                id,
                path,
                normalized_path,
                kind,
                availability,
                is_enabled,
                created_at_utc,
                last_checked_at_utc
            FROM library_roots
            ORDER BY created_at_utc, path COLLATE NOCASE;
            """;

        var roots = new List<LibraryRoot>();
        await using var reader = await command
            .ExecuteReaderAsync(cancellationToken)
            .ConfigureAwait(false);

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            roots.Add(ReadRoot(reader));
        }

        return roots;
    }

    public async Task<LibraryRoot?> FindByNormalizedPathAsync(
        string normalizedPath,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _database
            .OpenConnectionAsync(cancellationToken)
            .ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT
                id,
                path,
                normalized_path,
                kind,
                availability,
                is_enabled,
                created_at_utc,
                last_checked_at_utc
            FROM library_roots
            WHERE normalized_path = $normalizedPath COLLATE NOCASE
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("$normalizedPath", normalizedPath);

        await using var reader = await command
            .ExecuteReaderAsync(cancellationToken)
            .ConfigureAwait(false);
        return await reader.ReadAsync(cancellationToken).ConfigureAwait(false)
            ? ReadRoot(reader)
            : null;
    }

    public async Task AddAsync(
        LibraryRoot libraryRoot,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(libraryRoot);

        await using var connection = await _database
            .OpenConnectionAsync(cancellationToken)
            .ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO library_roots(
                id,
                path,
                normalized_path,
                kind,
                availability,
                is_enabled,
                created_at_utc,
                last_checked_at_utc)
            VALUES (
                $id,
                $path,
                $normalizedPath,
                $kind,
                $availability,
                $isEnabled,
                $createdAtUtc,
                $lastCheckedAtUtc);
            """;

        command.Parameters.AddWithValue("$id", libraryRoot.Id.ToString("D"));
        command.Parameters.AddWithValue("$path", libraryRoot.Path);
        command.Parameters.AddWithValue("$normalizedPath", libraryRoot.NormalizedPath);
        command.Parameters.AddWithValue("$kind", (int)libraryRoot.Kind);
        command.Parameters.AddWithValue("$availability", (int)libraryRoot.Availability);
        command.Parameters.AddWithValue("$isEnabled", libraryRoot.IsEnabled);
        command.Parameters.AddWithValue(
            "$createdAtUtc",
            libraryRoot.CreatedAtUtc.ToString("O", CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue(
            "$lastCheckedAtUtc",
            libraryRoot.LastCheckedAtUtc.ToString("O", CultureInfo.InvariantCulture));

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static LibraryRoot ReadRoot(Microsoft.Data.Sqlite.SqliteDataReader reader) =>
        LibraryRoot.Restore(
            Guid.Parse(reader.GetString(0)),
            reader.GetString(1),
            reader.GetString(2),
            (LibraryRootKind)reader.GetInt32(3),
            (LibraryRootAvailability)reader.GetInt32(4),
            reader.GetBoolean(5),
            DateTimeOffset.Parse(
                reader.GetString(6),
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind),
            DateTimeOffset.Parse(
                reader.GetString(7),
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind));
}
