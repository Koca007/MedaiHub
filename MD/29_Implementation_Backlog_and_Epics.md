# Volume 29 — Implementation Backlog and Epics

**Project:** MediaHub  
**Document set:** MediaHub Project Bible  
**Status:** Revised baseline specification v1.2  
**Language:** English  
**Target platform:** Windows desktop first  
**Last updated:** 2026-08-08  

---

## 1. Backlog conventions

Each backlog item should include:

- stable ID;
- requirement links;
- user value;
- technical scope;
- dependencies;
- security impact;
- migration impact;
- acceptance criteria;
- test strategy;
- documentation impact.

Priority labels:

- P0: blocks usable product or protects data/security;
- P1: required for target release;
- P2: important enhancement;
- P3: future.

## 2. Epic E01 — Repository and build foundation

### E01-S01 Create solution boundaries

**Priority:** P0

Tasks:

- create projects described in Volume 04;
- configure central package management;
- enable nullable;
- warnings as errors;
- analyzers;
- architecture tests;
- baseline CI.

Acceptance:

- clean clone builds;
- dependency boundary tests pass;
- no UI dependency from Domain/Application.

### E01-S02 Establish application bootstrap

- composition root;
- settings load;
- structured logging;
- unhandled exception boundary;
- single-instance policy;
- safe-mode command line.

## 3. Epic E02 — Domain and persistence

### E02-S01 Core media domain

Implement title, movie, series, season, episode, media file, availability, and external identifiers.

### E02-S02 User state

Implement playback state, watch events, ratings, notes, favorites, collections, and playlists.

Favorites are independent from ratings. Notes use bounded variable-length plain text. Multi-episode files use ordered segment mappings rather than duplicate physical rows.

### E02-S03 SQLite persistence

- mappings;
- migrations;
- indexes;
- transactions;
- backups;
- integrity checks;
- export/import baseline.

Exit criteria:

- deletion of file record does not remove title state;
- migration fixture tests pass.

## 4. Epic E03 — Library roots and scanner

Stories:

- add local root;
- add UNC root;
- exclusions;
- enumerate;
- parse filenames;
- probe media;
- batch import;
- watcher events;
- reconciliation;
- missing state;
- rename/move identity;
- review queue;
- user matching rules.

P0 scenarios:

- copy in progress;
- root disconnect;
- watcher overflow;
- same-title ambiguity;
- reattachment after restore.

## 5. Epic E04 — Search and read models

- FTS index;
- title/people/genre search;
- filter model;
- home rows;
- details projections;
- season episode projection;
- missing-media view;
- cache invalidation.

Performance gate: 10,000-title search target.

## 6. Epic E05 — Cinematic shell

- app navigation;
- theme tokens;
- localization;
- MediaCard;
- ContentRow;
- Home;
- Movies;
- Series;
- Search;
- details pages;
- loading/error/empty states;
- keyboard navigation.

Accessibility gate: full keyboard browse.

The 1.0 UI locale is Hungarian. Personalized recommendation rows remain absent until Epic E13.

## 7. Epic E06 — Player

- engine spike;
- ADR selection;
- adapter;
- rendering;
- controls;
- fullscreen;
- track selection;
- checkpoints;
- resume;
- completion;
- next episode;
- screenshot;
- mini player;
- error fallback.

P0: progress survives forced termination.

Version 1.0 acceptance includes Picture-in-Picture session reuse, screenshot-gallery scene jump, playlist-origin continuation precedence, and multi-episode combined playback.

## 8. Epic E07 — Metadata and artwork

- local sidecars;
- provenance;
- artwork cache;
- generated frame;
- metadata-provider SDK;
- refresh;
- review;
- locale fallback.

Version 1.0 includes one approved built-in remote metadata implementation, Hungarian → original → English fallback, remote artwork, and daily provider-confirmed episode schedule refresh. Additional providers and deeper enrichment may follow.

## 9. Epic E08 — Subtitle system

- embedded/sidecar discovery;
- manual import;
- track registration;
- delay controls.

Post-1.0 slice:

- subtitle-provider SDK activation;
- download validation and cache;
- downloaded-track default disabled behavior.

Security gate: archive/path tests.

## 10. Epic E09 — Organization and statistics

- star rating;
- note editor;
- manual collections;
- automatic rule engine;
- playlists;
- favorites;
- watch history;
- weekly/monthly/yearly statistics;
- favorite genre/actor/director.

## 11. Epic E10 — Network storage

- root reachability;
- credential references;
- reconnect;
- network watcher/polling;
- playback interruption;
- mapped drive identity;
- safe delete capability detection.

P0: no false missing/deletion on outage.

## 12. Epic E11 — Plugin foundation

- SDK;
- manifest schema;
- package validator;
- developer/internal package test host;
- built-in capability enforcement;
- host services;
- health/circuit breaker;
- declarative settings;
- sample plugin;
- compatibility CI.

Public 1.0 ships approved built-in extensions only and exposes no external package installer. User-facing `.mhpkg` install, permission, and uninstall workflows are a later hardened slice.

## 13. Epic E12 — Download manager

- trusted update/artwork queue for 1.0;
- HTTP adapter;
- pause/resume;
- temporary files;
- checksum;
- quarantine;
- scanner handoff;
- bandwidth;
- provider resolver after 1.0;
- external-client integration contract after 1.0.

## 14. Epic E13 — Recommendations

**Release:** Post-1.0

- profile derivation;
- genre weights;
- quality preference;
- similar-title ranker;
- deterministic same-director suggestions without learned director affinity;
- explanations;
- feedback;
- reset;
- recommendation row.

Privacy gate: no note/title telemetry.

## 15. Epic E14 — Updates and deployment

- installer spike;
- ADR;
- signing;
- update manifest;
- download/staging;
- optional notification;
- release notes;
- migration coordination;
- rollback/recovery;
- channels.

## 16. Epic E15 — Diagnostics and support

- diagnostics page;
- event catalog;
- support bundle;
- redaction;
- crash marker;
- recovery mode;
- backup restore;
- database tool.

## 17. Release slicing

### Prototype release

Only internal:

- shell;
- scanner proof;
- player proof;
- DB scale proof;
- network proof.

### Alpha

- local root;
- local scan;
- basic browse;
- playback/resume;
- no destructive operations beyond test environment.

### Beta

- network roots;
- deletion to Recycle Bin;
- migration/recovery;
- ratings/notes;
- collections/playlists;
- localization/theme;
- signed installer.

### 1.0

- release gates from Volume 19;
- signed downloadable installer for Windows 10/11 x64;
- automatic collections and Favorites;
- approved remote metadata/artwork and daily new-episode monitoring;
- Hungarian UI and metadata fallback;
- Picture-in-Picture and screenshot gallery with scene jump;
- plugin foundation with built-in extensions only;
- update notification;
- diagnostics;
- accessibility;
- backup/export;
- documented limitations.

Documented post-1.0 limitations include online subtitle providers, AI recommendations, public user synchronization/account UI, authorized acquisition, and external `.mhpkg` installation.

## 18. Dependency graph

```text
Foundation
├── Domain/Persistence
│   ├── Scanner
│   ├── Search/Read Models
│   ├── Progress/Statistics
│   └── Collections/Playlists
├── UI Shell
│   ├── Browse
│   ├── Details
│   └── Settings
├── Player
├── Plugin SDK
│   ├── Metadata
│   ├── Subtitles
│   └── Acquisition
└── Deployment/Diagnostics
```

## 19. Scope control rules

A new feature cannot enter v1 unless it:

- supports core local playback/library trust;
- does not delay P0 reliability work;
- has owner and acceptance;
- fits data/security model;
- preserves local operation without a mandatory account or reachable cloud backend.

Online subtitle providers, AI, public synchronization/account UI, acquisition, external plugin installation, mobile, web, and marketplace remain post-1.0. The Supabase Development/Test backend and local-first synchronization foundation are part of the implementation baseline defined by Epic E18 but remain hidden from public personal-data workflows.

## 20. Backlog exit evidence

Every epic closes with:

- demo;
- automated tests;
- performance evidence where relevant;
- security review;
- updated docs;
- migration/recovery evidence;
- accessibility evidence;
- known limitations.


## 21. Epic E18 — Supabase development backend and synchronization

### E18-S01 Supabase project foundation

- local CLI configuration;
- development and staging projects;
- migrations and seed data;
- CI migration checks.

### E18-S02 Cloud identity adapter

- automatically provisioned synthetic Development/Test account;
- protected session storage;
- internal identity lifecycle and future sign-in/sign-out contracts;
- token refresh diagnostics.

### E18-S03 Local sync infrastructure

- outbox table;
- cursor table;
- remote shadows;
- dead-letter and conflict records;
- transactional write integration.

### E18-S04 Supabase adapter

- database mapping;
- Realtime hints;
- Storage abstraction;
- Edge Function invocation;
- strict DTO validation.

### E18-S05 Synchronized aggregates

- rating and notes;
- favorites;
- collections and memberships;
- playlists and ordering;
- selected preferences;
- playback/watch summaries.

### E18-S06 Security and RLS

- per-table RLS;
- ownership tests;
- least-privilege functions;
- no privileged client secrets.

### E18-S07 Operations and diagnostics

- sync status screen;
- pending/error counters;
- remote protocol compatibility;
- release catalog and feature flags.

Acceptance gate: all local capabilities remain functional with the backend offline for at least 24 hours of simulated edits and playback.

Public Windows 1.0 acceptance additionally verifies that personal synchronization workers and sign-in UI are disabled while approved non-personal remote metadata, episode schedule, compatibility, and release services remain available.

---

## Document control

This document is part of the MediaHub revised baseline specification v1.2. Changes that affect architecture, security, data compatibility, plugin contracts, or user-visible behavior must be recorded in the Architecture Decision Record volume and reflected in the traceability matrix.
