# Volume 26 — Database Physical Schema and Migration Plan

**Project:** MediaHub  
**Document set:** MediaHub Project Bible  
**Status:** Revised baseline specification v1.2  
**Language:** English  
**Target platform:** Windows desktop first  
**Last updated:** 2026-08-08  

---

## 1. Conventions

- SQLite foreign keys enabled.
- Table names plural PascalCase or consistent snake_case; one convention chosen and enforced.
- UUID stored consistently as 16-byte blob or canonical text after benchmark.
- UTC timestamps stored as ISO-8601 text or integer epoch consistently.
- Boolean stored as integer with check constraints.
- Enumerations stored as stable strings or integers with migration-safe mapping.
- User text uses Unicode.
- Foreign keys indexed.

## 2. Illustrative schema

The following SQL is conceptual and must be adjusted to the selected EF Core mapping.

```sql
CREATE TABLE MediaTitles (
    Id TEXT PRIMARY KEY NOT NULL,
    MediaType INTEGER NOT NULL,
    DisplayTitle TEXT NOT NULL,
    OriginalTitle TEXT NULL,
    SortTitle TEXT NOT NULL,
    ReleaseYear INTEGER NULL,
    Overview TEXT NULL,
    UserRating INTEGER NULL CHECK (UserRating BETWEEN 1 AND 5),
    UserNote TEXT NULL CHECK (UserNote IS NULL OR length(UserNote) <= 10000),
    IsFavorite INTEGER NOT NULL DEFAULT 0 CHECK (IsFavorite IN (0, 1)),
    Availability INTEGER NOT NULL,
    MetadataState INTEGER NOT NULL,
    AddedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NOT NULL,
    RowVersion INTEGER NOT NULL DEFAULT 0
);

CREATE INDEX IX_MediaTitles_SortTitle
ON MediaTitles(SortTitle);

CREATE INDEX IX_MediaTitles_Availability
ON MediaTitles(Availability);
```

`UserNote` uses SQLite's variable-length `TEXT` storage. It is not padded, wrapped in rich-text JSON, or compressed individually.

```sql
CREATE TABLE PlayableItems (
    Id TEXT PRIMARY KEY NOT NULL,
    MediaTitleId TEXT NOT NULL,
    Kind INTEGER NOT NULL,
    EpisodeId TEXT NULL,
    FOREIGN KEY (MediaTitleId) REFERENCES MediaTitles(Id),
    FOREIGN KEY (EpisodeId) REFERENCES Episodes(Id)
);
```

```sql
CREATE TABLE MediaFiles (
    Id TEXT PRIMARY KEY NOT NULL,
    DirectPlayableItemId TEXT NULL,
    MappingMode INTEGER NOT NULL CHECK (MappingMode IN (0, 1, 2)),
    LibraryRootId TEXT NOT NULL,
    NormalizedPath TEXT NOT NULL,
    OriginalPath TEXT NOT NULL,
    FileSizeBytes INTEGER NOT NULL,
    ModifiedAtUtc TEXT NOT NULL,
    QuickFingerprint TEXT NULL,
    FullHash TEXT NULL,
    DurationTicks INTEGER NULL,
    Container TEXT NULL,
    VideoCodec TEXT NULL,
    Width INTEGER NULL,
    Height INTEGER NULL,
    HdrMode TEXT NULL,
    Availability INTEGER NOT NULL,
    LastVerifiedAtUtc TEXT NULL,
    QualityScore REAL NOT NULL DEFAULT 0,
    CHECK (
        (MappingMode = 0 AND DirectPlayableItemId IS NOT NULL)
        OR (MappingMode = 1 AND DirectPlayableItemId IS NULL)
        OR (MappingMode = 2 AND DirectPlayableItemId IS NOT NULL)
    ),
    FOREIGN KEY (DirectPlayableItemId) REFERENCES PlayableItems(Id),
    FOREIGN KEY (LibraryRootId) REFERENCES LibraryRoots(Id)
);

CREATE UNIQUE INDEX UX_MediaFiles_NormalizedPath
ON MediaFiles(NormalizedPath);

CREATE INDEX IX_MediaFiles_QuickFingerprint
ON MediaFiles(QuickFingerprint);
```

```sql
CREATE TABLE MediaFileEpisodeSegments (
    MediaFileId TEXT NOT NULL,
    EpisodeId TEXT NOT NULL,
    RangeOrder INTEGER NOT NULL CHECK (RangeOrder >= 0),
    StartTicks INTEGER NULL CHECK (StartTicks IS NULL OR StartTicks >= 0),
    EndTicks INTEGER NULL CHECK (EndTicks IS NULL OR EndTicks >= 0),
    BoundarySource INTEGER NOT NULL,
    Confidence REAL NOT NULL CHECK (Confidence >= 0 AND Confidence <= 1),
    PRIMARY KEY (MediaFileId, EpisodeId),
    UNIQUE (MediaFileId, RangeOrder),
    FOREIGN KEY (MediaFileId) REFERENCES MediaFiles(Id) ON DELETE CASCADE,
    FOREIGN KEY (EpisodeId) REFERENCES Episodes(Id)
);
```

`MappingMode` values are Direct (0), Segmented (1), and CombinedRange (2). Direct rows must have no episode segments. Segmented rows must have at least two ordered segments and no direct playable item. CombinedRange rows must have at least two ordered Unknown segments plus one direct range playable item. These cross-table cardinality rules are enforced transactionally by the repository and rechecked by integrity maintenance.

For an Unknown boundary, both timestamps are null. The application then uses one combined range playback state and never estimates individual completion from percentage alone.

```sql
CREATE TABLE PlaybackStates (
    PlayableItemId TEXT PRIMARY KEY NOT NULL,
    PositionTicks INTEGER NOT NULL,
    DurationTicks INTEGER NOT NULL,
    CompletionState INTEGER NOT NULL,
    LastPlayedAtUtc TEXT NULL,
    LastMediaFileId TEXT NULL,
    ResumeArtworkId TEXT NULL,
    ManualOverride INTEGER NOT NULL DEFAULT 0,
    SessionSequence INTEGER NOT NULL DEFAULT 0
);
```

```sql
CREATE TABLE WatchEvents (
    Id TEXT PRIMARY KEY NOT NULL,
    SessionId TEXT NOT NULL,
    PlayableItemId TEXT NOT NULL,
    MediaFileId TEXT NULL,
    StartedAtUtc TEXT NOT NULL,
    EndedAtUtc TEXT NOT NULL,
    ActiveWallSeconds INTEGER NOT NULL,
    MediaSecondsConsumed INTEGER NOT NULL,
    StartPositionTicks INTEGER NOT NULL,
    EndPositionTicks INTEGER NOT NULL,
    CompletionTransition INTEGER NOT NULL,
    TerminationReason INTEGER NOT NULL
);

CREATE INDEX IX_WatchEvents_PlayableItem
ON WatchEvents(PlayableItemId, StartedAtUtc);

CREATE INDEX IX_WatchEvents_Time
ON WatchEvents(StartedAtUtc);
```

## 3. Search document

FTS table may include:

- media title ID;
- display title;
- original title;
- aliases;
- people;
- genres;
- tags;
- year token;
- note only when local note search is enabled.

```sql
CREATE VIRTUAL TABLE MediaSearch USING fts5(
    MediaTitleId UNINDEXED,
    Title,
    OriginalTitle,
    Aliases,
    People,
    Genres,
    Tags,
    tokenize = 'unicode61 remove_diacritics 2'
);
```

The search index is a rebuildable projection, not canonical data.

## 4. Outbox

`OutboxMessages` carries durable local integration events. `SyncOutboxMessages` in Section 16 is a separate, allow-listed remote-mutation queue. Implementations may share infrastructure but must not conflate processing/acknowledgement semantics.

```sql
CREATE TABLE OutboxMessages (
    Id TEXT PRIMARY KEY NOT NULL,
    EventType TEXT NOT NULL,
    SchemaVersion INTEGER NOT NULL,
    PayloadJson TEXT NOT NULL,
    OccurredAtUtc TEXT NOT NULL,
    ProcessedAtUtc TEXT NULL,
    AttemptCount INTEGER NOT NULL DEFAULT 0,
    LastErrorCode TEXT NULL
);

CREATE INDEX IX_Outbox_Unprocessed
ON OutboxMessages(ProcessedAtUtc, OccurredAtUtc);
```

## 5. Background jobs

Persistent jobs include only resumable or user-relevant work. Ephemeral UI refreshes remain in memory.

Fields:

- job type;
- state;
- priority;
- payload schema;
- checkpoint;
- attempt;
- next retry;
- progress;
- owner plugin;
- error code.

## 6. Concurrency

SQLite writes are serialized naturally but application design still prevents long transactions.

Guidelines:

- WAL mode after compatibility validation;
- busy timeout;
- short transactions;
- batch scanner inserts;
- no external process while a transaction is open;
- no HTTP while transaction open;
- optimistic row version for note/rating edits;
- maintenance locks through application-level coordinator.

## 7. Migration numbering

Example:

```text
202608030001_InitialCore
202608030002_AddPlaybackStates
202608030003_AddCollections
202608030004_AddPluginState
202608080001_AddFavoritesAndMultiEpisodeMappings
```

Each migration has:

- forward operation;
- validation query;
- data-transform strategy;
- estimated complexity;
- recovery notes;
- fixture tests.

## 8. Migration execution

1. Acquire single-instance migration lock.
2. Verify source schema.
3. Run integrity check.
4. Create backup and verify hash.
5. Ensure free space.
6. Begin migration.
7. Run schema changes.
8. Run bounded data transformations.
9. Rebuild projections if required.
10. Run post-migration integrity.
11. Commit and record checksum.
12. Keep backup until retention policy removes it.

## 9. Interrupted migration

SQLite transactional DDL is used where supported. For multi-stage transformations:

- migration journal table;
- stage checkpoints;
- idempotent batches;
- original columns retained until validation;
- recovery mode on ambiguous state.

## 10. Data retention

Canonical title data remains until explicit purge. Derived data can be rebuilt:

- search index;
- recommendation profile;
- statistics aggregates;
- artwork derivatives;
- provider cache.

User-owned data is not automatically purged:

- ratings;
- notes;
- watch events;
- manual collections;
- playlists;
- screenshots;
- matching decisions, unless reset.

## 11. Referential behavior

Cascade delete is used cautiously.

Allowed cascade examples:

- deleting a playlist deletes playlist entries;
- deleting a plugin cache namespace deletes plugin cache.

Restricted:

- deleting media file must not delete title or playback state;
- deleting library root must not automatically purge logical titles;
- deleting provider cache must not delete user metadata.

## 12. Integrity queries

Examples:

```sql
SELECT NormalizedPath, COUNT(*)
FROM MediaFiles
GROUP BY NormalizedPath
HAVING COUNT(*) > 1;
```

```sql
SELECT pe.Id
FROM PlaylistEntries pe
LEFT JOIN PlayableItems pi ON pi.Id = pe.PlayableItemId
WHERE pi.Id IS NULL;
```

Maintenance tools report before repair.

## 13. Backup rotation

Default:

- last 3 migration backups;
- daily backups for 7 days;
- weekly backups for 4 weeks, if periodic backups enabled;
- size-aware cleanup;
- never delete the only known valid backup during failed migration.

## 14. Export schema

Full JSON export:

```json
{
  "schemaVersion": 1,
  "exportedAtUtc": "2026-08-03T06:00:00Z",
  "applicationVersion": "1.0.0",
  "titles": [],
  "playbackStates": [],
  "watchEvents": [],
  "collections": [],
  "playlists": [],
  "libraryPathMappings": []
}
```

Secrets and provider tokens are excluded.

## 15. Acceptance criteria

- Foreign keys are enabled and tested.
- Search index can be rebuilt.
- Migration backup is verified.
- Favorite and compact note fields round-trip without affecting rating.
- Direct, Segmented, and CombinedRange mappings preserve one physical file row, enforce valid cardinality, and keep ordered episode links.
- A file row can be deleted without deleting logical history.
- Large scanner commits do not block UI for long periods.
- Interrupted migration enters deterministic recovery.
- Export/import round trip preserves ratings, notes, and memberships.


## 16. Local synchronization schema

This schema is active in Development/Test builds in version 1.0 and reserved for the later public synchronization feature. Public 1.0 does not create remote mutations for personal data.

```sql
CREATE TABLE SyncOutboxMessages (
    Id TEXT PRIMARY KEY NOT NULL,
    AggregateType TEXT NOT NULL,
    AggregateId TEXT NOT NULL,
    Operation TEXT NOT NULL,
    PayloadJson TEXT NOT NULL,
    LocalSequence INTEGER NOT NULL,
    CreatedAtUtc TEXT NOT NULL,
    NextAttemptAtUtc TEXT NULL,
    AttemptCount INTEGER NOT NULL DEFAULT 0,
    LastErrorCode TEXT NULL,
    AcknowledgedAtUtc TEXT NULL
);

CREATE INDEX IX_SyncOutboxMessages_Pending
ON SyncOutboxMessages(AcknowledgedAtUtc, NextAttemptAtUtc, LocalSequence);

CREATE TABLE SyncCursors (
    Stream TEXT PRIMARY KEY NOT NULL,
    LastRemoteSequence INTEGER NOT NULL DEFAULT 0,
    LastSynchronizedAtUtc TEXT NULL
);

CREATE TABLE RemoteRecordShadows (
    AggregateType TEXT NOT NULL,
    AggregateId TEXT NOT NULL,
    RemoteRevision INTEGER NOT NULL,
    RemoteUpdatedAtUtc TEXT NOT NULL,
    PayloadHash TEXT NULL,
    PRIMARY KEY (AggregateType, AggregateId)
);

CREATE TABLE SyncConflicts (
    Id TEXT PRIMARY KEY NOT NULL,
    AggregateType TEXT NOT NULL,
    AggregateId TEXT NOT NULL,
    FieldName TEXT NOT NULL,
    LocalPayloadJson TEXT NOT NULL,
    RemotePayloadJson TEXT NOT NULL,
    DetectedAtUtc TEXT NOT NULL,
    ResolutionState TEXT NOT NULL
);
```

Outbox insertion occurs in the same transaction as the user-owned local change. Acknowledgement and remote-shadow update also occur transactionally.

## 17. Dual migration policy

Local SQLite migrations and Supabase PostgreSQL migrations have independent version sequences. A desktop release declares:

- minimum and maximum supported remote protocol versions;
- required local schema version;
- optional feature migrations;
- rollback/recovery instructions.

The client must not assume that the local and remote schema versions are numerically equal.

---

## Document control

This document is part of the MediaHub revised baseline specification v1.2. Changes that affect architecture, security, data compatibility, plugin contracts, or user-visible behavior must be recorded in the Architecture Decision Record volume and reflected in the traceability matrix.
