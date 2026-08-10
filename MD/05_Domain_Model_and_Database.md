# Volume 05 — Domain Model and Database

**Project:** MediaHub  
**Document set:** MediaHub Project Bible  
**Status:** Revised baseline specification v1.2  
**Language:** English  
**Target platform:** Windows desktop first  
**Last updated:** 2026-08-08  

---

## 1. Persistence strategy

The Windows client uses SQLite with Entity Framework Core or an equivalent mature ORM. The domain model remains persistence-ignorant. Writes are transactional; read-heavy screens may use projection queries.

Database location:

```text
%LOCALAPPDATA%\MediaHub\Data\mediahub.db
```

User-configurable portable mode is not part of the initial release, but paths are abstracted to allow it later.

## 2. Core aggregates

### MediaTitle

Abstract logical identity shared by `Movie` and `Series`.

Fields:

| Field | Type | Notes |
|---|---|---|
| Id | UUID | Stable identity |
| MediaType | enum | Movie or Series |
| DisplayTitle | string | Required |
| OriginalTitle | string? | Optional |
| SortTitle | string | Normalized |
| ReleaseYear | int? | Validation range |
| Overview | string? | Metadata |
| CommunityRating | decimal? | Provider value |
| UserRating | int? | 1–5 |
| UserNote | string? | Compact plain text, no HTML/Markdown |
| IsFavorite | bool | Independent from rating |
| AddedAtUtc | timestamp | First import |
| UpdatedAtUtc | timestamp | Logical update |
| Availability | enum | Available, Partial, Missing |
| MetadataState | enum | LocalOnly, Matched, Enriched, ReviewNeeded |
| PreferredArtworkId | UUID? | Optional |
| ExternalIds | value collection | Provider-specific |
| RowVersion | integer | Optimistic concurrency |

Invariants:

- display title cannot be empty;
- user rating is null or 1–5;
- favorite state does not imply or change a rating;
- note content is plain text and stored in a variable-length field;
- removing all files changes availability but does not delete the title;
- external IDs are unique per provider and media type.

### Movie

Additional fields:

- runtime;
- edition group;
- release date;
- production countries;
- certification;
- collection/franchise metadata.

### Series

Additional fields:

- first air date;
- end date;
- continuing status;
- known season count;
- known episode count;
- next known air date;
- auto-acquisition preference for future authorized providers.

### Season

Fields:

- series ID;
- season number or special identifier;
- title;
- overview;
- artwork;
- known episode count;
- sort order.

Unique key: `(SeriesId, SeasonNumber)` for numbered seasons.

### Episode

Fields:

- series ID;
- season ID;
- episode number;
- absolute episode number;
- title;
- overview;
- air date;
- runtime;
- special flag.

Canonical uniqueness is based on series, season, and episode number, with provider-specific handling for specials and double episodes.

### MediaFile

Represents one physical file.

Fields:

- ID;
- direct playable item ID, nullable for a reliably segmented multi-episode file;
- mapping mode: Direct, Segmented, or CombinedRange;
- library root ID;
- normalized path;
- original path;
- file identity where available;
- size;
- modified timestamp;
- quick fingerprint;
- optional full hash;
- container;
- video codec;
- width and height;
- frame rate;
- HDR mode;
- duration;
- audio stream summary;
- subtitle stream summary;
- default audio stream;
- reachable state;
- last verified timestamp;
- edition label;
- quality score.

A media file is never the owner of rating, note, favorite, or watched state.

### MediaFileEpisodeSegment

Represents an ordered mapping between one physical multi-episode file and one canonical episode.

Fields:

- media file ID;
- episode ID;
- range order;
- optional start and end timestamp;
- boundary source: Chapter, ProviderRuntime, Manual, or Unknown;
- confidence;
- validation timestamp.

One `MediaFile` remains one physical record even when several segment mappings exist. In Direct mode it points to one playable item and has no episode segments. In Segmented mode it has no direct playable item and its ordered segment records resolve the canonical episode playable items. In CombinedRange mode it points to one range playable item while retaining ordered links to all canonical episodes. When reliable boundaries are unavailable, MediaHub uses CombinedRange with shared progress and does not infer individual completion from an arbitrary percentage. Completing the full combined item completes every linked episode.

### PlaybackState

One current state per playable item:

- item ID;
- position;
- duration at last playback;
- completion state;
- last played timestamp;
- last media file ID;
- resume thumbnail ID;
- manual watched override;
- update version.

### WatchEvent

Append-only statistical event:

- session ID;
- playable item ID;
- media file ID;
- start timestamp;
- end timestamp;
- watched seconds;
- start position;
- end position;
- completion transition;
- termination reason.

Short accidental opens below a configurable threshold can be ignored for aggregate statistics.

### Collection

Fields:

- ID;
- name;
- description;
- type: Manual or Automatic;
- artwork;
- rule definition for automatic collections;
- sort behavior;
- creation and modification timestamps.

Many-to-many relation with media titles. Manual membership and automatic computed membership are distinguishable.

### Playlist

Fields:

- ID;
- name;
- description;
- artwork;
- behavior options;
- timestamps.

`PlaylistEntry` fields:

- playlist ID;
- entry ID;
- playable item ID;
- sort position;
- added timestamp;
- optional label.

### LibraryRoot

Fields:

- ID;
- path;
- root type: Local, UNC, MappedDrive;
- content classification;
- enabled;
- scan options;
- exclusion rules;
- credential reference, never raw credential;
- reachability state;
- last scan and last reconciliation;
- watcher support state.

### MatchingDecision

Stores durable user corrections:

- normalized input signature;
- selected media title or episode;
- decision scope;
- source filename pattern;
- confidence at decision;
- created timestamp;
- optional expiration.

### ProviderRecord

Tracks provider data without making provider responses the canonical domain:

- provider ID;
- entity ID;
- external ID;
- payload version;
- fetched timestamp;
- expiry;
- content hash;
- attribution requirements.

## 3. Relationship summary

```text
MediaTitle
├── Movie
└── Series ──< Season ──< Episode

Movie/Episode ──< MediaFile
MediaFile ──< MediaFileEpisodeSegment >── Episode
Movie/Episode ──1 PlaybackState
Movie/Episode ──< WatchEvent

MediaTitle >──< Collection
Movie/Episode >──< Playlist
LibraryRoot ──< MediaFile
MediaTitle ──< Artwork
MediaTitle ──< ExternalIdentifier
```

## 4. Tables

Suggested physical tables:

```text
MediaTitles
Movies
Series
Seasons
Episodes
MediaFiles
MediaFileEpisodeSegments
VideoStreams
AudioStreams
SubtitleStreams
PlaybackStates
WatchEvents
Collections
CollectionMembers
Playlists
PlaylistEntries
LibraryRoots
LibraryExclusions
MatchingDecisions
ArtworkAssets
ExternalIdentifiers
ProviderCacheEntries
Notifications
BackgroundJobs
PluginStates
ApplicationSettings
SchemaMigrations
OutboxMessages
AuditEntries
```

## 5. Index strategy

Required indexes include:

- normalized title and sort title;
- release year;
- media type;
- availability;
- last played;
- date added;
- series/season/episode composite key;
- normalized media path unique index;
- quick fingerprint;
- collection membership;
- playlist order;
- watch event timestamp;
- provider/external ID unique key;
- outbox processed state.

Full-text search may use SQLite FTS5 over a denormalized search document containing titles, aliases, people, genres, tags, and selected technical fields.

## 6. Soft retention versus deletion

Logical records are retained when:

- a disk is disconnected;
- a network share is unavailable;
- a file is moved to Recycle Bin;
- a file is renamed and temporarily unmatched;
- a library root is disabled.

A user may explicitly purge historical records through a separate maintenance workflow. Purge must show impact on notes, ratings, progress, playlists, and statistics.

## 7. Matching and reattachment

When a new file appears, reattachment scoring considers:

1. stable file identity on the same volume;
2. full hash;
3. quick fingerprint plus size;
4. normalized title, year, runtime;
5. series, season, and episode tokens;
6. prior matching decisions;
7. folder context.

Automatic reattachment requires a high confidence threshold and no competing candidate. Otherwise the item enters review.

## 8. Migration policy

- Migrations are sequential and immutable after release.
- Pre-migration backup is mandatory.
- Migration records include app version and checksum.
- Large data transformations are resumable.
- Downgrade is not guaranteed, but recovery to the pre-migration backup is.
- Plugin-owned persistent state is separated and versioned by plugin.
- Migration tests use databases from every supported upgrade path.

## 9. Backup and export

Automatic backups:

- before application schema migration;
- before repair operations;
- configurable periodic backup;
- rotation by count and age.

Export formats:

- JSON for full logical library state;
- CSV for ratings and watch history;
- optional portable artwork manifest;
- no provider credentials.

Import validates schema, IDs, duplicate policies, and path mapping before commit.

## 10. Concurrency

The desktop application is single-user but multi-threaded. Rules:

- one write unit of work per command;
- optimistic concurrency for user-edited fields;
- serialized migration and maintenance;
- scanner writes use batches;
- progress checkpoints use upsert with monotonic session sequence;
- read operations use cancellation and no tracking where possible.

## 11. Data integrity checks

Startup performs lightweight checks. Full maintenance can validate:

- foreign keys;
- duplicate normalized paths;
- orphan artwork;
- impossible progress values;
- missing collection targets;
- invalid playlist positions;
- unresolved outbox records;
- migration checksums;
- FTS index consistency.

Repairs are logged and previewed when user data may change.


## 12. Hybrid persistence model

The local and remote databases serve different purposes.

### Local-only entities

- `LibraryRoot`
- `MediaFile`
- `MediaTrack`
- `ProbeResult`
- `ScanJob`
- `PathIdentity`
- `LocalArtworkAsset`
- `DownloadJob`
- `SyncOutboxMessage`
- `SyncCursor`
- `RemoteRecordShadow`

### Synchronized aggregates

- rating on `MediaTitle`;
- user note on `MediaTitle`;
- favorite state on `MediaTitle`;
- `PlaybackStateSummary`;
- `WatchHistorySummary`;
- manual `Collection` and memberships;
- `Playlist` and ordered entries;
- selected application preferences;
- post-1.0 recommendation genre/quality preference profile;
- notification acknowledgement;
- optional matching decisions that are safe across devices.

### Remote-only or remotely authoritative records

- cloud identity;
- device registration;
- feature flags;
- application releases and changelog;
- server-generated recommendations;
- remote notification envelopes;
- server-side function audit records.

## 13. Synchronization entities

```csharp
public sealed class SyncOutboxMessage
{
    public Guid Id { get; init; }
    public string AggregateType { get; init; } = string.Empty;
    public Guid AggregateId { get; init; }
    public string Operation { get; init; } = string.Empty;
    public string PayloadJson { get; init; } = string.Empty;
    public long LocalSequence { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? NextAttemptAtUtc { get; set; }
    public int AttemptCount { get; set; }
    public string? LastErrorCode { get; set; }
    public DateTimeOffset? AcknowledgedAtUtc { get; set; }
}

public sealed class SyncCursor
{
    public string Stream { get; init; } = string.Empty;
    public long LastRemoteSequence { get; set; }
    public DateTimeOffset? LastSynchronizedAtUtc { get; set; }
}
```

Every remote mutation carries an idempotency key derived from the local outbox ID. Remote records contain `user_id`, stable aggregate IDs, `updated_at`, and a server-controlled revision or sequence.

## 14. Conflict policy

- Rating: newest explicit edit wins; manual conflict review is available in diagnostics.
- Note: preserve both versions when concurrent edits occur and require user selection or merge.
- Favorite: latest accepted explicit toggle wins; the replaced state remains diagnosable for a bounded period.
- Playback state: furthest credible position wins unless a later explicit restart/reset exists.
- Collection membership: set union for independent additions; explicit removals use tombstones.
- Playlist order: operation-based ordering with stable entry IDs; concurrent reorder conflicts require deterministic server ordering and may surface review.
- Preferences: field-level latest accepted update.
- Device-specific fields: never merged remotely.

## 15. Tombstones and deletion

Synchronized deletions use tombstones rather than immediate physical deletion. Tombstones contain aggregate ID, deletion timestamp, origin device, and revision. They are retained long enough for offline devices to observe them. Local physical file deletion is never represented as remote deletion of the logical media title.

## 16. Public 1.0 synchronization scope

The synchronization entities remain part of the Development/Test architecture and are exercised with automatically provisioned synthetic identities. Public Windows 1.0 does not expose sign-in or upload personal state. When public synchronization is released, enabling it includes every supported user-owned aggregate, including compact plain-text notes and favorites.

---

## Document control

This document is part of the MediaHub revised baseline specification v1.2. Changes that affect architecture, security, data compatibility, plugin contracts, or user-visible behavior must be recorded in the Architecture Decision Record volume and reflected in the traceability matrix.
