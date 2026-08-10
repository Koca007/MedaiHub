# Volume 33 — Detailed Test Case Catalog

**Project:** MediaHub  
**Document set:** MediaHub Project Bible  
**Status:** Revised baseline specification v1.2  
**Language:** English  
**Target platform:** Windows desktop first  
**Last updated:** 2026-08-08  

---

## 1. Test case format

```text
ID
Requirement
Preconditions
Test data
Steps
Expected result
Cleanup
Automation status
```

## 2. Library tests

### TC-LIB-001 Add local movie root

**Preconditions:** Empty database.

**Steps:**

1. Add valid local folder as Movies.
2. Confirm.
3. Observe scan.

**Expected:**

- root persisted;
- scan job created;
- supported files imported;
- unsupported files ignored;
- UI remains responsive.

### TC-LIB-002 Add inaccessible root

Expected:

- root not saved without explicit disabled choice;
- permission guidance;
- no credential in log.

### TC-LIB-003 Disconnect NAS

Expected:

- root Offline;
- files TemporarilyUnavailable;
- no title purge;
- one grouped notification;
- cached browse works.

### TC-LIB-004 Reconnect NAS

Expected:

- root Online;
- targeted reconciliation;
- files reachable;
- history unchanged.

## 3. Scanner tests

### TC-SCAN-001 Copy growing file

1. Start copying a large `.mkv`.
2. Watch events fire.
3. Verify no final import until stable.
4. Complete copy.

Expected one media file record.

### TC-SCAN-002 Rename watched file

Expected same media identity or safe reattachment; progress retained.

### TC-SCAN-003 Same title ambiguity

Input `King Kong.mkv` with multiple candidates.

Expected Review Needed, no guessed year.

### TC-SCAN-004 Watcher overflow

Simulate overflow.

Expected reconciliation job and consistent final state.

### TC-SCAN-005 Malicious sidecar path

Sidecar references `../../outside`.

Expected reject with security event.

## 4. Player tests

### TC-PLAY-001 Default controls

- Space pauses/plays.
- Left seeks -10 seconds.
- Right seeks +10 seconds.
- Wheel changes volume.
- Double-click toggles fullscreen.

### TC-PLAY-002 Resume after crash

1. Play to 20 minutes.
2. Wait for checkpoint.
3. Force terminate.
4. Restart and resume.

Expected position within checkpoint tolerance.

### TC-PLAY-003 Natural completion

Expected watched state, WatchEvent, next episode prompt if applicable.

### TC-PLAY-004 Decoder fallback

Hardware decoding failure.

Expected offer/retry software mode; no watched state corruption.

### TC-PLAY-005 Downloaded subtitle

**Release:** Post-1.0 online-subtitle milestone. Expected subtitle listed, Off remains selected.

### TC-PLAY-006 Default audio

File has default track flag.

Expected selected track matches flag.

### TC-PLAY-007 Picture-in-Picture session reuse

Enter and leave Picture-in-Picture during playback.

Expected one playback session, continuous position, no duplicate WatchEvent, and preserved playlist origin.

### TC-PLAY-008 Screenshot scene jump

Capture with `Ctrl+S`, open the title gallery, and choose “Ugrás ehhez a jelenethez.”

Expected fingerprint/edition validation and seek to the stored timestamp. A changed or missing edition produces a safe unavailable action rather than an incorrect seek.

### TC-PLAY-009 Playlist continuation precedence

Play a film or episode from an ordered playlist and finish it.

Expected next target is the next playlist entry, never an inferred sequel or canonical next episode.

### TC-PLAY-010 Multi-episode file

Import `Series.S01E01E02.mkv` with and without valid chapter boundaries.

Expected one physical file record and two ordered episode links. Reliable boundaries use Segmented mode with no direct playable-item link. Unknown boundaries use CombinedRange mode with one range playable item and shared progress. Invalid mode/cardinality combinations are rejected transactionally.

## 5. Deletion tests

### TC-DEL-001 Local delete

Expected file in Windows Recycle Bin and logical title Missing.

### TC-DEL-002 Retention

Before deletion: rating, note, progress, two collections, playlist.

After deletion: all remain.

### TC-DEL-003 Restore

Restore file and reconcile.

Expected automatic reattachment and resume.

### TC-DEL-004 Stale preview token

Change file after preview.

Expected execution rejected and new preview required.

### TC-DEL-005 Network share without recycle

Expected physical delete disabled or accurately labeled; never falsely claim Recycle Bin.

## 6. Collections and playlists

### TC-COL-001 Multiple collections

One title can join multiple; removing from one leaves others.

### TC-COL-002 Automatic rule

Change rating from 3 to 5; collection rule `rating >= 4` updates.

### TC-PL-001 Missing playlist item

Expected entry retained with missing indicator; playback follows configured skip/stop.

### TC-PL-002 Reorder keyboard

Expected stable persisted order.

### TC-FAV-001 Favorite independent from rating

Toggle Favorite, change rating, clear rating, delete/restore the file.

Expected Favorite remains unchanged and survives missing-media transitions.

## 7. Ratings and notes

### TC-RATE-001 Set/clear stars

Valid 1–5; invalid rejected; clear supported.

### TC-NOTE-001 Autosave

Type, wait debounce, close/reopen.

Expected exact text.

### TC-NOTE-002 Privacy

Generate logs/support bundle.

Expected note absent.

## 8. Metadata/artwork

### TC-META-001 Offline local-only

Expected title playable and generated frame artwork.

### TC-META-002 User edit lock

Provider refresh does not overwrite locked title.

### TC-META-003 Language fallback

Request metadata with Hungarian partially unavailable.

Expected field resolution order is Hungarian, original language, then English, with provenance retained per field.

### TC-EP-001 Daily released-episode notification

Provide an episode whose official air date has arrived and whose provider state is Released/Available.

Expected one in-app notification, no duplicate within subsequent refreshes, and local availability remains a separate state. Refresh runs after startup and no more than once per 24 hours.

### TC-ART-001 Black-frame avoidance

Generated fallback should choose non-black candidate when available.

### TC-SUB-001 Archive traversal

Malicious subtitle archive.

Expected extraction blocked and quarantined.

## 9. Downloads

### TC-DL-001 Disk full

Expected pause/fail safely, partial remains according to policy, no corrupted final.

### TC-DL-002 Executable mismatch

File named `.mkv` with executable magic.

Expected quarantine, no scanner import.

### TC-DL-003 Resume with changed ETag

Expected restart, not append.

### TC-DL-004 Checksum mismatch

Expected quarantine and re-download option.

## 10. Plugins

### TC-PLUG-001 Incompatible SDK

Expected internal test-host activation rejection.

### TC-PLUG-002 Undeclared host

Plugin requests network domain not in manifest.

Expected blocked.

### TC-PLUG-003 Crash loop

Expected circuit open and host stable.

### TC-PLUG-004 Secret logging

Attempt to log secret through structured value.

Expected redaction or policy violation detection.

### TC-PLUG-005 Public external package unavailable

Attempt to open or install an arbitrary `.mhpkg` in a production 1.0 build.

Expected no install surface, no package execution/loading, and a safe unsupported response.

## 11. Updates

### TC-UPD-001 Decline update

Expected application remains usable.

### TC-UPD-002 Invalid signature

Expected reject and security event.

### TC-UPD-003 Migration interruption

Expected backup/recovery path.

### TC-UPD-004 Old plugin after host update

Expected clear compatible/incompatible status.

## 12. AI tests

Every test in this section belongs to the post-1.0 recommendation milestone.

### TC-AI-001 Cold start

Expected non-personal rows and explanation.

### TC-AI-002 Genre learning

Watch/rate sufficient genre sample.

Expected increased relevant ranking after threshold.

### TC-AI-003 No learned director affinity

**Release:** Post-1.0. Watch/rate several works by one director. Expected no persistent director-affinity feature; any same-director row must cite a concrete source title and use deterministic metadata.

### TC-AI-004 Reset

Expected derived profile cleared; watch history optionally retained.

### TC-AI-005 Notes excluded

Change note text.

Expected recommendation profile unchanged.

## 13. Localization/theme

### TC-I18N-001 Missing Hungarian key

Expected English fallback, not raw key.

### TC-I18N-003 Complete Hungarian 1.0

Run the production resource inventory and primary UI journeys.

Expected every public 1.0 string has a Hungarian resource; English remains safety fallback only.

### TC-I18N-002 Placeholder mismatch

Build validation fails.

### TC-THEME-001 Invalid contrast

Expected warning/block for critical text.

### TC-THEME-002 Reduced motion

Expected major transitions disabled.

## 14. Accessibility

### TC-A11Y-001 Keyboard onboarding

Complete without mouse.

### TC-A11Y-002 Screen-reader card

Expected title, type, progress, rating, availability.

### TC-A11Y-003 Focus return

Close details; focus returns to originating card.

### TC-A11Y-004 200% text

No clipped primary actions.

## 15. Recovery

### TC-REC-001 Corrupt copy

Expected original preserved and recovery mode.

### TC-REC-002 Safe mode

Expected external plugins disabled after the external-plugin milestone; in 1.0, approved built-ins start only when compatible and the default theme is restored.

### TC-REC-003 Support bundle

Expected category preview and sensitive exclusions.

## 16. Release regression pack

Mandatory before stable release:

- add local and NAS roots;
- initial scan and reconcile;
- ambiguous match;
- playback/resume/crash;
- next episode;
- embedded/sidecar/manual subtitles;
- default audio;
- Picture-in-Picture and screenshot scene jump;
- playlist continuation precedence;
- multi-episode combined file;
- rate/note;
- collections/playlists;
- delete/restore retention;
- update decline/signature;
- plugin crash;
- backup/migration;
- keyboard/screen reader;
- offline operation.
- proof that public personal synchronization, AI, online subtitle providers, acquisition, and external plugin installation remain disabled.


## 17. Synchronization test cases

The following cases execute only in Development/Test builds using automatically provisioned synthetic identities until public synchronization ships.

### TC-SYNC-001 — Offline rating edit

**Given:** Supabase is unreachable.  
**When:** User changes a rating.  
**Then:** Rating updates immediately in SQLite, one outbox record exists, no blocking dialog appears, and the record synchronizes once connectivity returns.

### TC-SYNC-002 — Lost success response

**Given:** Remote upsert commits but the response is lost.  
**When:** The same outbox message retries.  
**Then:** Idempotency prevents duplicate logical records and the outbox is acknowledged.

### TC-SYNC-003 — Concurrent note conflict

**Given:** Desktop and test device edit the same note offline.  
**When:** Both synchronize.  
**Then:** Neither note is discarded; a conflict record is created and UI offers both versions.

### TC-SYNC-004 — RLS isolation

**Given:** Two test identities.  
**When:** Identity A queries or mutates identity B data.  
**Then:** No row is returned or changed and the policy test passes.

### TC-SYNC-005 — Backend outage during playback

**Given:** Active playback and pending synchronization.  
**When:** Network and Supabase fail.  
**Then:** Playback continues, checkpoints remain local, retry load is bounded, and the UI shows only a non-blocking sync status.

### TC-SYNC-006 — Unsupported remote protocol

**Given:** Server advertises a protocol newer than the client maximum.  
**When:** Sync initializes.  
**Then:** Incompatible cloud features are disabled, release notes explain the update, and all local features remain usable.

### TC-SYNC-007 — Environment separation

**Given:** A production-signed client.  
**When:** Configuration attempts to use the development project.  
**Then:** Startup rejects the endpoint for cloud operations and records a security diagnostic without exposing keys.

---

## Document control

This document is part of the MediaHub revised baseline specification v1.2. Changes that affect architecture, security, data compatibility, plugin contracts, or user-visible behavior must be recorded in the Architecture Decision Record volume and reflected in the traceability matrix.
