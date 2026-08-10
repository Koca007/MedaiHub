using Microsoft.Data.Sqlite;

namespace MediaHub.Infrastructure.Local.Persistence;

public sealed class LocalDatabase : IDisposable
{
    private const int CurrentSchemaVersion = 1;
    private readonly SemaphoreSlim _initializationGate = new(1, 1);
    private bool _isInitialized;

    public LocalDatabase(LocalDataPaths paths)
    {
        ArgumentNullException.ThrowIfNull(paths);
        Paths = paths;

        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = paths.DatabasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared,
            Pooling = true,
            ForeignKeys = true,
        };

        ConnectionString = connectionString.ToString();
    }

    public LocalDataPaths Paths { get; }

    public string ConnectionString { get; }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_isInitialized)
        {
            return;
        }

        await _initializationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_isInitialized)
            {
                return;
            }

            Directory.CreateDirectory(Paths.DataDirectory);
            Directory.CreateDirectory(Paths.LogsDirectory);
            Directory.CreateDirectory(Paths.CacheDirectory);

            await using var connection = new SqliteConnection(ConnectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            await using var command = connection.CreateCommand();
            command.CommandText =
                """
                PRAGMA journal_mode = WAL;
                PRAGMA foreign_keys = ON;
                PRAGMA busy_timeout = 5000;

                CREATE TABLE IF NOT EXISTS schema_migrations (
                    version INTEGER PRIMARY KEY NOT NULL,
                    applied_at_utc TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS library_roots (
                    id TEXT PRIMARY KEY NOT NULL,
                    path TEXT NOT NULL,
                    normalized_path TEXT NOT NULL COLLATE NOCASE,
                    kind INTEGER NOT NULL CHECK (kind BETWEEN 0 AND 3),
                    availability INTEGER NOT NULL CHECK (availability BETWEEN 0 AND 1),
                    is_enabled INTEGER NOT NULL CHECK (is_enabled IN (0, 1)),
                    created_at_utc TEXT NOT NULL,
                    last_checked_at_utc TEXT NOT NULL
                );

                CREATE UNIQUE INDEX IF NOT EXISTS ux_library_roots_normalized_path
                    ON library_roots(normalized_path);
                """;
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            await using var versionCommand = connection.CreateCommand();
            versionCommand.CommandText =
                """
                INSERT OR IGNORE INTO schema_migrations(version, applied_at_utc)
                VALUES ($version, $appliedAtUtc);
                """;
            versionCommand.Parameters.AddWithValue("$version", CurrentSchemaVersion);
            versionCommand.Parameters.AddWithValue(
                "$appliedAtUtc",
                DateTimeOffset.UtcNow.ToString("O", System.Globalization.CultureInfo.InvariantCulture));
            await versionCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            _isInitialized = true;
        }
        finally
        {
            _initializationGate.Release();
        }
    }

    internal async Task<SqliteConnection> OpenConnectionAsync(
        CancellationToken cancellationToken)
    {
        await InitializeAsync(cancellationToken).ConfigureAwait(false);
        var connection = new SqliteConnection(ConnectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        return connection;
    }

    public void Dispose() => _initializationGate.Dispose();
}
