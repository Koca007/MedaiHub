# Volume 19 — Testing, Quality Assurance and Acceptance

**Project:** MediaHub  
**Document set:** MediaHub Project Bible  
**Status:** Revised baseline specification v1.2  
**Language:** English  
**Target platform:** Windows desktop first  
**Last updated:** 2026-08-08  

---

## 1. Quality strategy

Testing is risk-based. Highest-risk areas are:

- loss or corruption of user history;
- wrong physical-file deletion;
- resume-position loss;
- scanner misidentification;
- network-share false deletion;
- unsafe plugin or download behavior;
- database migration;
- player regressions;
- accessibility regressions.

## 2. Test pyramid

### Unit tests

Fast tests for:

- domain invariants;
- progress calculations;
- collection rules;
- recommendation scoring;
- filename parsing;
- match scoring;
- path normalization;
- settings validation;
- update manifest verification logic.

### Integration tests

- SQLite repositories;
- migrations;
- FTS search;
- FFprobe adapter;
- file-system abstraction;
- Windows Recycle Bin adapter;
- network reachability simulation;
- plugin host;
- download commit;
- update staging.

### Contract tests

- plugin SDK;
- provider schemas;
- update manifest;
- import/export formats;
- localization placeholder compatibility.

### UI tests

- onboarding;
- browse/search;
- details;
- playback controls;
- rating/note;
- collections/playlists;
- deletion dialog;
- settings;
- update notification;
- recovery mode.
- Favorite toggle independent from rating;
- Picture-in-Picture session continuity;
- screenshot gallery and “Jump to scene.”

### Manual exploratory tests

- visual quality;
- cinematic controls;
- unusual media;
- multi-monitor;
- high DPI;
- network instability;
- accessibility technologies;
- long-running sessions.

## 3. Test data

A legally distributable test corpus includes:

- short public-domain or generated media;
- multiple containers/codecs;
- multi-audio;
- embedded subtitles;
- malformed files;
- black-frame samples;
- variable frame rate;
- long filenames;
- Unicode paths;
- duplicate titles and years;
- multi-episode filenames;
- large artificial libraries.

No copyrighted commercial media is required in CI.

## 4. Domain test examples

### Progress

- position zero;
- negative invalid input;
- duration zero;
- 89.9%;
- exactly 90%;
- natural end;
- manual override;
- changed duration.

### Series progress

- unaired episodes excluded;
- specials excluded by default;
- partially known season;
- missing local episode but known canonical episode;
- watched event retained after file deletion.

### Collections

- same title in multiple collections;
- automatic rule updates;
- deletion of collection preserves title;
- invalid cyclic rule representation rejected.

## 5. Scanner tests

Filename matrix covers:

- `S01E01`;
- `1x01`;
- absolute numbering;
- ranges;
- years in title;
- titles containing numbers;
- director's cut;
- resolution tokens;
- anime naming;
- specials;
- ambiguous same-title films;
- invalid path characters;
- Unicode.

File-system scenarios:

- copy in progress;
- rename;
- move within root;
- move across roots;
- delete;
- restore;
- watcher overflow;
- inaccessible file;
- root disconnect/reconnect;
- symlink loop;
- permission change.

## 6. Network tests

A controlled SMB environment simulates:

- latency;
- packet loss;
- authentication failure;
- share restart;
- mapped-drive change;
- read-only share;
- unsupported recycle semantics;
- playback interruption;
- stale directory cache.

## 7. Player tests

Automated where possible, with engine-level fixtures:

- open/play/pause/seek/stop;
- checkpoint timing;
- default audio selection;
- post-1.0 provider-downloaded subtitles remain disabled until explicitly enabled;
- fullscreen state;
- completion threshold;
- natural end;
- next episode;
- decoder fallback;
- network interruption;
- screen sleep behavior;
- session cleanup.
- playlist-origin continuation;
- multi-episode range playback with known and unknown boundaries;
- Picture-in-Picture without a second session;
- screenshot scene-jump fingerprint validation.

Visual/video correctness still requires manual and hardware matrix testing.

## 8. Database migration tests

For every supported source version:

1. restore fixture database;
2. run migration;
3. verify integrity;
4. verify representative user data;
5. verify indexes;
6. verify search;
7. verify backup exists;
8. simulate interruption;
9. verify recovery.

Production migrations cannot be edited after release.

## 9. Security tests

- path traversal;
- command injection;
- malformed JSON;
- huge payload;
- archive bomb;
- executable disguised as media;
- invalid update signature;
- plugin permission bypass;
- secret redaction;
- untrusted URL redirects;
- TLS failure;
- database tampering;
- symbolic link escape.

## 10. Performance tests

Benchmarks:

- cold and warm startup;
- 10k/50k/100k title search;
- initial scan throughput;
- reconciliation;
- artwork loading;
- home read-model query;
- mass progress update;
- statistics aggregation;
- plugin timeout behavior;
- download hashing.

Performance baselines are version-controlled. Significant regression requires review.

## 11. Accessibility testing

- full keyboard walkthrough;
- screen reader on Home, details, player, settings;
- focus visibility;
- high contrast;
- 200% text scaling;
- reduced motion;
- color-blind state differentiation;
- subtitle/control overlap.

## 12. Localization testing

- pseudolocalization;
- long German-like strings;
- accented Hungarian;
- CJK fonts;
- right-to-left readiness;
- plural rules;
- date/number formatting;
- missing resource fallback;
- plugin translation fallback.

Windows 1.0 release validation requires a complete Hungarian UI. English is tested as an internal resource fallback, not advertised as a complete 1.0 locale.

## 13. Resilience and soak tests

- 24-hour playback;
- repeated episode autoplay;
- week-long background scanner simulation;
- repeated NAS disconnect;
- 1000 download job history entries;
- plugin crash loop;
- log rotation;
- cache cleanup;
- database backup rotation.

## 14. Release acceptance gates

A stable release requires:

- all critical and high-priority tests pass;
- no open data-loss issue;
- no open critical/high unmitigated security issue;
- migration tested from previous stable versions;
- accessibility primary flows pass;
- signed artifacts;
- SBOM generated;
- release notes complete;
- support bundle validated;
- crash-free target met in beta evidence if telemetry exists;
- signed installer and upgrade validation on Windows 10 and Windows 11 x64;
- no public path for installing arbitrary third-party `.mhpkg` packages.

## 15. Definition of done for a feature

- requirement and acceptance criteria approved;
- implementation reviewed;
- unit/integration tests;
- UI tests for critical journey;
- logging;
- localization;
- accessibility;
- security review;
- migration if needed;
- documentation;
- changelog entry;
- no expired feature flag.

## 16. Example end-to-end acceptance scenario

**Scenario:** Delete and restore a rated film.

1. Import film.
2. Rate five stars and add note.
3. Watch 45%.
4. Add to two collections and one playlist.
5. Delete through MediaHub.
6. Verify file enters Recycle Bin.
7. Verify title is Missing.
8. Verify rating, note, progress, memberships remain.
9. Restore file.
10. Reconcile.
11. Verify automatic reattachment.
12. Resume at previous position.
13. Verify statistics remain consistent.

This scenario is a mandatory release regression test.


## 17. Supabase and synchronization test matrix

These tests run in Development/Test configurations with automatically provisioned synthetic identities. Passing them does not enable public personal-data synchronization in Windows 1.0.

Required automated suites:

- local Supabase CLI startup and migration from an empty database;
- migration from every supported remote schema version;
- RLS positive and negative ownership tests;
- anonymous/unauthenticated request rejection where required;
- token expiry and refresh;
- lost response after successful mutation;
- duplicate idempotency key;
- out-of-order delta delivery;
- concurrent rating edits;
- concurrent note edits preserving both versions;
- collection add/remove while one device is offline;
- playlist reorder conflict;
- complete backend outage during playback;
- malformed Realtime event;
- staging/production environment separation;
- Edge Function secret isolation;
- desktop downgrade within supported protocol window.

Cloud tests run against disposable projects or local containers. Tests must never target production.

---

## Document control

This document is part of the MediaHub revised baseline specification v1.2. Changes that affect architecture, security, data compatibility, plugin contracts, or user-visible behavior must be recorded in the Architecture Decision Record volume and reflected in the traceability matrix.
