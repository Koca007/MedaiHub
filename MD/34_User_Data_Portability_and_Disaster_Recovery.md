# Volume 34 — User Data Portability and Disaster Recovery

**Project:** MediaHub  
**Document set:** MediaHub Project Bible  
**Status:** Revised baseline specification v1.2  
**Language:** English  
**Target platform:** Windows desktop first  
**Last updated:** 2026-08-08  

---

## 1. Data ownership classes

### User-owned canonical data

- ratings;
- notes;
- playback states;
- watch events;
- collections;
- playlists;
- favorites;
- screenshots;
- manual metadata edits;
- matching decisions;
- library configuration.

Notes are exported as compact plain text. Favorite is exported independently from rating.

### Rebuildable data

- search index;
- statistics aggregates;
- recommendation profile;
- artwork derivatives;
- provider cache;
- probe cache;
- home read models.

### External or physical data

- media files;
- sidecars;
- downloaded subtitles;
- plugin packages;
- provider accounts.

Recovery prioritizes canonical user-owned data.

## 2. Backup types

### Migration backup

Automatic before schema change.

### Scheduled application backup

Optional periodic database and settings backup.

### User export

Portable JSON/CSV with documented schema.

### Full support copy

Only through explicit advanced workflow; may contain sensitive library data.

## 3. Backup consistency

Before backup:

- finish/rollback active write transaction;
- checkpoint WAL if appropriate;
- use SQLite backup API or safe snapshot;
- include schema version;
- hash result;
- validate open and integrity;
- write manifest.

## 4. Backup manifest

```json
{
  "schemaVersion": 1,
  "createdAtUtc": "2026-08-03T06:00:00Z",
  "applicationVersion": "1.0.0",
  "databaseSchemaVersion": 12,
  "files": [
    {
      "name": "mediahub.db",
      "sha256": "...",
      "bytes": 123456
    }
  ],
  "containsSecrets": false
}
```

## 5. Export scopes

- ratings and notes;
- watch history;
- collections/playlists;
- full logical library;
- settings without secrets;
- matching rules;
- screenshots manifest;
- all selected.

Path mappings may be included as templates to migrate from one machine to another.

## 6. Import modes

### Merge

- match by stable IDs/external IDs;
- preserve newer local edits according to conflict policy;
- append watch events with deduplication;
- merge collection memberships;
- report conflicts.

### Replace

Advanced destructive operation with backup and preview.

### New installation migration

Map old roots to new roots before scanning.

## 7. Conflict resolution

Fields:

- rating: choose local/imported/newest;
- note: never silently concatenate; preview;
- playback: latest timestamp, with manual override priority;
- collection: union by default;
- playlist order: user choice;
- title edits: user-locked fields require explicit decision.

## 8. Machine migration flow

1. Export from old machine.
2. Install MediaHub on new machine.
3. Import logical data.
4. Map library roots.
5. Scan.
6. Reattach files by fingerprint and metadata.
7. Review unresolved items.
8. Verify counts.
9. Keep old backup until accepted.

## 9. Disaster scenarios

### Database deleted

- restore newest verified backup;
- rescan media;
- restore export if backup unavailable;
- provider metadata can be reacquired;
- notes/history only recoverable from backup/export.

### Database corrupt

- preserve original;
- restore verified backup;
- optionally recover recent watch checkpoints from journal/crash state;
- reconcile.

### Media drive lost

- logical records remain Missing;
- export inventory;
- attach replacement drive;
- reattach matching files;
- retain history.

### Application settings corrupt

- load last-known-good;
- default settings;
- preserve database;
- safe mode.

### Plugin corrupts own state

- disable plugin;
- reset isolated plugin state;
- host database remains protected.

## 10. Recovery point objectives

Suggested targets:

- progress checkpoint loss: under 15 seconds during active playback;
- settings loss: last atomic save;
- database after application crash: no committed transaction loss beyond SQLite guarantees;
- migration: restore pre-migration state;
- scheduled backup: configurable, default daily if enabled.

## 11. Recovery time objectives

Targets depend on library size:

- settings recovery: minutes;
- database restore: minutes;
- search/recommendation rebuild: background;
- full media rescan: potentially long, but browsing restored data should begin before completion;
- network root reconnection: targeted.

## 12. Restore validation

After restore:

- database integrity;
- schema compatibility;
- title counts;
- ratings/notes counts;
- watch event date range;
- collection/playlist relationships;
- root mapping;
- search rebuild;
- plugin state compatibility.

## 13. Privacy in exports

- no credentials;
- no tokens;
- paths optional/redactable;
- notes included only when selected;
- screenshots excluded by default;
- encryption option recommended for full exports in future;
- warning before sharing support data.

## 14. Uninstall behavior

Uninstaller options:

- remove application only;
- remove cache/logs;
- remove settings;
- remove database/user data, separate explicit confirmation.

Default uninstall retains user database or clearly asks. Media files are never removed.

## 15. Development/Test and future account synchronization

Cloud synchronization is implemented and tested as an internal local-first capability in Development/Test builds. Public Windows 1.0 exposes no sign-in and uploads no personal state. The later public synchronization feature follows these rules:

- the local database remains usable and authoritative for offline-critical state;
- stable synchronization IDs, outbox records, cursors, and remote shadows are part of the baseline;
- compact plain-text notes, favorites, history summaries, and every other supported category synchronize together when the single synchronization feature is enabled;
- the conflict model is shared with import/merge behavior;
- remote deletion uses explicit tombstones and retention;
- cloud account deletion does not remove local data without separate consent;
- cloud export and remote-data deletion are available through trusted server-side workflows.

## 16. Future cloud-assisted recovery and portability

Cloud synchronization is not a replacement for local backup. It can restore synchronized logical user data but cannot reconstruct:

- media files;
- local folder paths;
- complete scan/probe state;
- locally generated screenshots unless explicitly synchronized;
- local-only watch detail when upload is disabled;
- network credentials.

### New-machine recovery

This flow is available only after public synchronization ships.

1. Install MediaHub and initialize SQLite.
2. Optionally sign in to the single cloud identity.
3. Pull synchronized ratings, notes, collections, playlists, preferences, and watch summaries.
4. Map local or network media roots.
5. Scan and reattach files to logical identities.
6. Review ambiguous matches.
7. Restore any additional local backup for non-synchronized data.

### Backend migration/export

The project shall provide an administrative export of synchronized user data in provider-neutral JSON. Supabase row IDs, SDK-specific metadata, and internal auth secrets are excluded or mapped to stable MediaHub IDs. This enables future migration to a custom API or another PostgreSQL deployment.

### Supabase disaster handling

- restore remote database using provider backup capabilities where available;
- keep migration SQL and seed/reference data in Git;
- redeploy RLS policies and Edge Functions from source;
- compare remote revisions with local shadows;
- run reconciliation without deleting unmatched local user data;
- treat every desktop SQLite database as a recoverable local copy, not as expendable cache.


## 17. Acceptance criteria

- Verified backup can be restored.
- Import/export round trip preserves user-owned fields.
- Root path migration works.
- Corrupt settings do not delete database.
- Uninstall does not remove media.
- Support export excludes secrets.
- Missing drive retains logical inventory and history.
- Cloud restoration never overwrites local-only media paths or credentials.
- Provider-neutral cloud export can be imported by a replacement backend adapter.
- Public Windows 1.0 restore never expects a cloud identity; local backup/export remains authoritative.

---

## Document control

This document is part of the MediaHub revised baseline specification v1.2. Changes that affect architecture, security, data compatibility, plugin contracts, or user-visible behavior must be recorded in the Architecture Decision Record volume and reflected in the traceability matrix.
