# Volume 02 — Product Requirements and User Flows

**Project:** MediaHub  
**Document set:** MediaHub Project Bible  
**Status:** Revised baseline specification v1.2  
**Language:** English  
**Target platform:** Windows desktop first  
**Last updated:** 2026-08-08  

---

## 1. Functional requirement catalogue

### Library onboarding

- **FR-LIB-001:** The application shall allow one or more library roots to be added.
- **FR-LIB-002:** A library root shall be classified as movie, series, mixed, or auto-detect.
- **FR-LIB-003:** The application shall validate read access before saving a root.
- **FR-LIB-004:** Network credentials shall not be stored in plain text.
- **FR-LIB-005:** Initial scanning shall run in the background and expose progress.
- **FR-LIB-006:** The user shall be able to pause, resume, or cancel an initial scan.
- **FR-LIB-007:** Cancelling a scan shall leave already committed records consistent.
- **FR-LIB-008:** The application shall support excluding folders, filename patterns, and extensions.

### Browsing and search

- **FR-BROWSE-001:** Home shall display Continue Watching, Recently Added, Movies, Series, Favorites, and configurable rows.
- **FR-BROWSE-002:** Search shall operate locally without internet access.
- **FR-BROWSE-003:** Search shall match titles, original titles, aliases, people, genres, tags, notes where enabled, file quality, language, and year.
- **FR-BROWSE-004:** Filters shall be composable.
- **FR-BROWSE-005:** Sort options shall include title, date added, release date, rating, last watched, runtime, and file quality.
- **FR-BROWSE-006:** Missing titles shall be optionally visible, hidden, or shown in a dedicated view.

### Playback

- **FR-PLAY-001:** Double-click shall toggle fullscreen.
- **FR-PLAY-002:** Space shall toggle play and pause.
- **FR-PLAY-003:** Left and Right shall seek backward or forward by ten seconds by default.
- **FR-PLAY-004:** Mouse wheel over the player shall change volume.
- **FR-PLAY-005:** Playback controls shall hide after inactivity and return on input.
- **FR-PLAY-006:** Playback position shall be persisted periodically and on pause, stop, navigation, and shutdown.
- **FR-PLAY-007:** The player shall offer “Resume” and “Start from beginning” when meaningful progress exists.
- **FR-PLAY-008:** The player shall respect the default audio track declared by the media container.
- **FR-PLAY-009:** Downloaded subtitles shall remain disabled until the user enables one.
- **FR-PLAY-010:** The next episode action shall select the next canonical episode, not merely the next filename.
- **FR-PLAY-011:** Playback failure shall not mark an item watched.
- **FR-PLAY-012:** The user shall be able to mark an item watched or unwatched manually.
- **FR-PLAY-013:** Automatic next-episode playback shall be disabled by default and explicitly configurable.
- **FR-PLAY-014:** When playback originates from a playlist, the next action shall target the next playlist entry rather than a canonical sequel or series episode.
- **FR-PLAY-015:** Picture-in-Picture shall reuse the active playback session and shall not open a second decoder/session.
- **FR-PLAY-016:** Manual screenshots shall be stored in a title gallery with source fingerprint and playback timestamp, and shall support “Jump to scene” when the same compatible edition is available.
- **FR-PLAY-017:** Provider-downloaded subtitles are a post-1.0 capability. Version 1.0 shall support embedded, sidecar, and manual subtitle import.

### Progress and history

- **FR-PROG-001:** Movie progress shall be calculated from the latest playback state.
- **FR-PROG-002:** Series progress shall show watched episodes divided by known episodes and, where duration is available, optional time-based progress.
- **FR-PROG-003:** Season progress shall be shown independently.
- **FR-PROG-004:** Completion threshold shall default to 90% and be configurable.
- **FR-PROG-005:** A resume thumbnail shall be captured near the saved position when technically feasible.
- **FR-PROG-006:** Watch events shall be retained for statistics even when the media file becomes unavailable.

### Ratings and notes

- **FR-RATE-001:** A title shall support a one-to-five-star rating in half-star increments only if later enabled; baseline uses whole stars.
- **FR-RATE-002:** Rating may be cleared.
- **FR-RATE-003:** A title shall support one optional private note with autosave and revision timestamp.
- **FR-RATE-004:** Notes shall remain when files are deleted or unavailable.
- **FR-RATE-005:** Notes shall be excluded from logs and telemetry.
- **FR-RATE-006:** A Favorite state shall be independently set and cleared without changing the star rating.
- **FR-RATE-007:** Notes shall be compact plain text without HTML or Markdown and shall use variable-length storage rather than fixed-width allocation.
- **FR-RATE-008:** A note shall contain at most 10,000 characters; the UI shall show the remaining count and reject overflow before persistence.

### Collections and playlists

- **FR-COL-001:** Manual collections shall support name, description, artwork, and ordered items.
- **FR-COL-002:** Automatic collections shall be rule-based.
- **FR-COL-003:** A title may belong to multiple collections.
- **FR-COL-004:** Deleting a collection shall not delete media.
- **FR-PL-001:** Playlists shall support movies and episodes.
- **FR-PL-002:** A playlist shall maintain manual order.
- **FR-PL-003:** A playlist item may reference an unavailable title and display it as missing.
- **FR-PL-004:** Playlist playback shall define behavior at episode boundaries and unavailable items.
- **FR-PL-005:** The default unavailable-entry behavior shall skip the entry with a visible notification; a playlist may be configured to stop instead.

### Deletion and restoration

- **FR-DEL-001:** Delete from disk shall require confirmation that names every affected file.
- **FR-DEL-002:** The default delete operation shall move files to the Windows Recycle Bin.
- **FR-DEL-003:** Logical title, progress, rating, notes, and memberships shall remain.
- **FR-DEL-004:** If a matching file reappears, historical data shall reconnect automatically when confidence is high.
- **FR-DEL-005:** Permanent deletion shall not be exposed in the first release.
- **FR-DEL-006:** Removing a title from the library without deleting files shall be a separate action.

### New episodes

- **FR-EP-001:** The application shall combine official episode air dates with the approved metadata provider's released/available state.
- **FR-EP-002:** When an episode reaches its official air date and the provider reports it released or available, the application shall create one deduplicated in-app notification even if no local file exists.
- **FR-EP-003:** Notifications shall be deduplicated.
- **FR-EP-004:** Future acquisition providers may support opt-in automatic acquisition.
- **FR-EP-005:** Automatic acquisition shall be disabled by default and configurable per series.
- **FR-EP-006:** Provider failure shall not affect library or player availability.
- **FR-EP-007:** Episode metadata refresh shall run after the shell becomes interactive at startup and at most once per 24-hour interval during normal background operation.
- **FR-EP-008:** Remote release state and local-file availability shall be represented separately.
- **FR-EP-009:** Windows toast notification for new episodes shall be optional; the in-app notification is always retained.

### Metadata and artwork

- **FR-META-001:** Version 1.0 shall include at least one approved built-in remote metadata implementation behind provider-neutral contracts.
- **FR-META-002:** Metadata language order shall be Hungarian, original language, then English.
- **FR-META-003:** Remote metadata failure shall fall back to accepted cache, local sidecars, filename-derived metadata, and generated artwork.
- **FR-META-004:** Remote refresh shall never overwrite a locked user edit.

### Updates

- **FR-UPD-001:** The application shall check for updates at a configurable frequency.
- **FR-UPD-002:** Update checks shall be optional.
- **FR-UPD-003:** Available updates shall show version, release date, release notes, download size, and compatibility notes.
- **FR-UPD-004:** Updates shall not be mandatory for local operation.
- **FR-UPD-005:** Features requiring a newer version shall be marked unavailable with an explanation.
- **FR-UPD-006:** Release notes shall identify new features, fixes, migrations, and known limitations.

## 2. Primary user flows

### 2.1 First launch

1. Application displays a concise welcome screen.
2. Version 1.0 starts in Hungarian and allows theme selection; later language packs use the existing localization architecture.
3. User adds one or more library roots.
4. Application validates access and estimates the initial scan scope.
5. User starts the scan.
6. Shell becomes usable immediately while indexing continues.
7. Imported items appear incrementally.
8. Ambiguous items are collected into a review queue rather than interrupting every file.
9. The dashboard shows scan progress and actionable problems.

Acceptance criteria:

- The user can play a directly opened file before indexing finishes.
- Closing the application during indexing does not corrupt the database.
- On restart, scanning resumes or reconciles safely.

### 2.2 Play and resume a film

1. User opens a movie details page.
2. MediaHub shows artwork, local technical quality, rating, note, and availability.
3. If no progress exists, primary action is Play.
4. If meaningful progress exists, primary action is Resume and secondary action is Start Over.
5. Player opens in cinematic mode.
6. Progress is checkpointed.
7. On exit, the title returns to Continue Watching unless completed.
8. At threshold, the title is marked watched and may leave Continue Watching.

### 2.3 Continue a series

1. User selects a series.
2. Series page shows total and per-season progress.
3. Primary action targets the earliest partially watched episode or next unwatched episode.
4. After completion, a Next Episode action is offered. Automatic continuation occurs only when the user enabled autoplay.
5. Missing next episodes are clearly marked.
6. If a later episode exists but an earlier episode is missing, the application warns without blocking manual playback.

If the session originated from a playlist, the playlist's next entry replaces the canonical Next Episode target.

### 2.4 Play a multi-episode file

1. Scanner detects an episode range such as `S01E01E02`.
2. MediaHub stores one physical media-file record and ordered links to both canonical episodes.
3. Chapter markers or validated episode runtimes define segment boundaries when reliable.
4. If no reliable boundary exists, the UI presents one combined `S01E01–E02` playable item with shared resume state.
5. Completing the combined item marks every linked episode complete; partial viewing is not guessed across unknown boundaries.

### 2.5 Resolve ambiguous matching

1. Scanner assigns a low-confidence match.
2. Item enters Review Needed.
3. User sees filename, folder, runtime, detected season/episode markers, and candidate titles.
4. User selects a candidate or creates a local-only title.
5. The decision is stored as a durable matching rule for equivalent future files.
6. The user can undo the decision.

### 2.6 Delete a title while preserving history

1. User selects Delete Files.
2. Dialog lists exact paths and explains that history and notes remain.
3. User confirms.
4. Files move to Recycle Bin.
5. Media file records become unavailable.
6. Logical title remains visible according to the missing-item filter.
7. If restored through Windows or re-added later, MediaHub reconnects the file after verification.

### 2.7 Create a playlist

1. User creates a playlist and chooses a name.
2. User adds movies or episodes from details pages or multi-select mode.
3. Playlist preserves order.
4. Playback skips unavailable items with a visible notification.
5. User can reorder using drag-and-drop and keyboard controls.

## 3. Error experience

Errors are categorized as:

- **Actionable:** user can fix now, such as disconnected share or missing permission.
- **Recoverable:** application retries or offers retry, such as temporary metadata failure.
- **Data integrity:** operation stops and protects existing data.
- **Unsupported:** file or plugin cannot be used.
- **Unexpected:** captured in diagnostics with a user-safe reference ID.

Every error message must include:

- what failed;
- whether data was changed;
- what the user can do;
- where diagnostics can be found, when relevant.

## 4. Notification rules

Notifications must be grouped and rate-limited. A scan discovering 300 files should create one summary, not 300 popups. Notifications are stored in an in-app center and may optionally use Windows notifications.

Priority levels:

- Information
- Success
- Warning
- Action required
- Critical

Critical notifications are reserved for database recovery, migration failure, or storage operations that may require immediate attention.


## 5. Cloud synchronization requirements

These requirements are implemented and exercised in Development/Test builds but are not exposed as a public user capability in Windows 1.0. When public synchronization is released, enabling it synchronizes every supported category rather than presenting per-category switches.

- **FR-SYNC-001:** Every user-owned synchronized aggregate shall be written to SQLite before a remote request is attempted.
- **FR-SYNC-002:** A successful local write shall create or update an outbox operation in the same local transaction.
- **FR-SYNC-003:** The synchronization worker shall batch, retry, deduplicate, and acknowledge outbox operations.
- **FR-SYNC-004:** Playback, local search, scanning, and local editing shall not wait for Supabase.
- **FR-SYNC-005:** Ratings, compact plain-text notes, favorites, manual collections, playlists, selected preferences, and summarized watch state shall support synchronization.
- **FR-SYNC-006:** Full local paths, network credentials, original video files, raw subtitle archives, and provider secrets shall not be synchronized.
- **FR-SYNC-007:** Conflicts shall follow field-specific policies and shall never silently discard notes.
- **FR-SYNC-008:** The UI shall expose last successful synchronization, pending operation count, authentication state, and recoverable errors.
- **FR-SYNC-009:** Realtime events shall trigger refresh/reconciliation, not direct trust of unvalidated payloads.
- **FR-SYNC-010:** Signing out shall not delete local data unless the user explicitly selects a documented local-data removal workflow.

## 6. Development and test backend flow

1. Developer starts the desktop application using the Development environment.
2. The app loads local SQLite and remains immediately usable.
3. An automatically provisioned synthetic development identity authenticates against local or hosted Supabase without a public sign-in screen.
4. Local changes are recorded in SQLite and queued in `SyncOutbox`.
5. The background synchronization worker sends idempotent mutations.
6. PostgreSQL Row Level Security verifies ownership.
7. Realtime or polling indicates remote changes.
8. The client downloads a delta, validates it, applies conflict rules, and updates local read models.
9. Failure leaves operations pending and displays non-blocking diagnostics.

---

## Document control

This document is part of the MediaHub revised baseline specification v1.2. Changes that affect architecture, security, data compatibility, plugin contracts, or user-visible behavior must be recorded in the Architecture Decision Record volume and reflected in the traceability matrix.
