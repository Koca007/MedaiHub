# Volume 23 — Architecture Decision Records

**Project:** MediaHub  
**Document set:** MediaHub Project Bible  
**Status:** Revised baseline specification v1.2  
**Language:** English  
**Target platform:** Windows desktop first  
**Last updated:** 2026-08-08  

---

## ADR format

Each decision includes:

- Status
- Context
- Decision
- Consequences
- Alternatives
- Review trigger

---

## ADR-001 — Windows-first native desktop client

**Status:** Accepted

**Context:** The primary product is a Windows media player and library with deep file-system, Recycle Bin, notification, fullscreen, and hardware integration.

**Decision:** Build the first client as a native .NET Windows desktop application supporting Windows 10 and Windows 11 x64 in version 1.0.

**Consequences:** Strong Windows integration and maintainability for the initial market. Cross-platform UI reuse is not the first priority. Domain/application layers remain platform-independent where practical.

**Alternatives:** Electron/React, Avalonia, web-first shell.

**Review trigger:** Mobile or cross-platform desktop roadmap enters funded implementation.

---

## ADR-002 — Clean Architecture with MVVM presentation

**Status:** Accepted

**Decision:** Use Clean Architecture boundaries and MVVM for desktop presentation.

**Consequences:** More project structure and contracts, but improved testability and replacement of UI/player/storage implementations.

---

## ADR-003 — Single local profile

**Status:** Accepted

**Decision:** The Windows client has one local MediaHub profile. It does not implement household profile switching in v1.

**Consequences:** Simpler data model and UI. Stable user IDs are used internally for the accepted synchronization architecture and future profile migration without household profile switching.

---

## ADR-004 — SQLite local database and local system of record

**Status:** Accepted, revised in v1.1

**Decision:** Use SQLite for the single-user Windows client with migrations, FTS, backups, repository abstraction, synchronization outbox, and remote-shadow state. SQLite remains authoritative for device-specific and offline-critical state.

**Consequences:** Easy deployment, local ownership, and uninterrupted playback. Synchronized user data is reconciled with a remote backend through a dedicated synchronization layer rather than by replacing local persistence.

---

## ADR-005 — Established playback engine behind adapter

**Status:** Accepted in principle; implementation selection pending prototype.

**Decision:** Do not implement codecs or demuxing. Embed LibVLCSharp or MPV behind `IMediaPlayer`.

**Consequences:** Broad format support and security-update dependency. The adapter prevents engine-specific types from entering application code.

**Review trigger:** Prototype benchmark and licensing review.

---

## ADR-006 — Real-time watcher plus reconciliation

**Status:** Accepted

**Decision:** Use file-system watchers for responsiveness and periodic reconciliation for correctness.

**Consequences:** Additional complexity and duplicate-event handling, but robust local and network behavior.

---

## ADR-007 — Logical title survives physical deletion

**Status:** Accepted

**Decision:** Moving media to Recycle Bin marks media files unavailable but retains title, rating, note, progress, history, collections, and playlists.

**Consequences:** Library can display missing titles and reconnect replacements. Separate purge workflow is required.

---

## ADR-008 — Local Recycle Bin default

**Status:** Accepted

**Decision:** Local deletion uses Windows Recycle Bin. Unsupported network locations do not pretend to provide recycle semantics.

**Consequences:** Safer deletion. Network deletion is restricted until capability-specific design exists.

---

## ADR-009 — Downloaded subtitles disabled by default

**Status:** Accepted

**Decision:** Version 1.0 supports embedded, sidecar, and manual subtitle import. Online subtitle-provider download is post-1.0; when introduced, newly downloaded subtitles are not activated automatically.

**Consequences:** Respects user preference and avoids unwanted overlays. User can enable a track quickly.

---

## ADR-010 — Container default audio track

**Status:** Accepted

**Decision:** Player initially respects the media container's default audio flag.

**Consequences:** Predictable file-author intent. Later explicit per-series preferences may override after user action.

---

## ADR-011 — Plugin foundation in version 1

**Status:** Accepted

**Decision:** Ship a versioned plugin manifest, capability model, SDK contracts, host foundation, and approved built-in extensions in v1. Public installation of arbitrary third-party `.mhpkg` packages is deferred.

**Consequences:** Upfront design cost prevents later invasive refactoring. Security boundaries must be real.

---

## ADR-012 — Declarative third-party UI

**Status:** Accepted

**Decision:** Future third-party plugins return declarative UI data rather than arbitrary WPF controls. Version 1.0 applies the same declarative contracts to built-in extensions where they contribute UI.

**Consequences:** Consistent themes, localization, accessibility, and lower spoofing risk. Less flexibility for plugin authors.

---

## ADR-013 — Offline-first recommendations

**Status:** Accepted

**Decision:** The recommendation module is post-1.0. It runs locally by default and learns only genre and preferred quality. Ratings and watch behavior are weighting signals. It stores no learned director, actor, cast, or note-text affinity.

**Consequences:** Strong privacy and offline availability. More advanced cloud-scale collaborative recommendations are deferred.

---

## ADR-014 — Optional updates

**Status:** Accepted

**Decision:** Updates are notified but not mandatory for local operation.

**Consequences:** Multiple supported versions may exist. Online provider compatibility must be communicated clearly.

---

## ADR-015 — Multilingual and token-based theming from v1

**Status:** Accepted

**Decision:** All UI text uses resources and appearance uses theme tokens from the beginning. The supported 1.0 UI locale is Hungarian; English remains an internal safety/source fallback for later language expansion.

**Consequences:** More discipline in early UI implementation; avoids expensive localization and theme retrofit.

---

## ADR-016 — Provider-neutral acquisition

**Status:** Accepted

**Decision:** Search and acquisition are extension contracts. Core does not hardcode a specific third-party media site.

**Consequences:** Better legal, security, and maintenance boundaries. Provider capabilities can be enabled or disabled independently.

---

## ADR-017 — User data not tied to physical file

**Status:** Accepted

**Decision:** Rating, compact plain-text note, favorite, progress, and organization attach to logical titles/playable items, not path records.

**Consequences:** Rename, move, edition upgrade, deletion, and restoration preserve user state.

---

## ADR-018 — Optional telemetry disabled by default

**Status:** Accepted

**Decision:** General telemetry is opt-in and content-free.

**Consequences:** Less default product analytics, stronger privacy promise and simpler offline use.

---


## ADR-019 — Supabase as development and testing backend

**Status:** Accepted

**Decision:** Use Supabase PostgreSQL, Auth, Realtime, Storage, and Edge Functions as the active backend platform during development and testing. Development/Test builds automatically provision a synthetic identity. Public Windows 1.0 exposes no sign-in or personal synchronization.

**Consequences:** Rapid backend iteration and realistic hosted integration are available early. Production remains an explicit later decision. Supabase-specific code is isolated.

---

## ADR-020 — Backend-neutral application contracts

**Status:** Accepted

**Decision:** Domain and Application projects depend only on MediaHub-owned cloud interfaces and DTOs. Supabase SDK types are restricted to `MediaHub.Infrastructure.Supabase`.

**Consequences:** Backend replacement is practical and test doubles are straightforward. Additional mapping code is required.

---

## ADR-021 — Outbox-based local-first synchronization

**Status:** Accepted

**Decision:** In Development/Test and the later public synchronization feature, local user changes commit to SQLite and an outbox in one transaction. A background worker performs idempotent remote synchronization.

**Consequences:** The UI never waits for the backend and offline edits are safe. Conflict handling, tombstones, and retry operations become first-class infrastructure.

---

## ADR-022 — Optional single cloud identity

**Status:** Accepted

**Decision:** A later release may expose one cloud account identity for synchronization and mobile access, but Windows 1.0 has no public sign-in and the application never implements household profile switching. When public synchronization is enabled, every supported user-data category synchronizes automatically.

**Consequences:** Cross-device ownership is possible without household-profile complexity. Signing out leaves local data intact.

---

## ADR-023 — Row Level Security and no privileged client secret

**Status:** Accepted

**Decision:** Client-accessible Supabase tables use RLS. Privileged service keys and third-party secrets never ship in the desktop client; privileged operations use Edge Functions or a later trusted API.

**Consequences:** Policies and policy tests are release-critical. Some operations require a server-side function.

---

## ADR-024 — Original media files remain outside Supabase

**Status:** Accepted

**Decision:** MediaHub does not automatically upload original movies, series episodes, or full local folder structures to Supabase Storage.

**Consequences:** Lower cost and privacy risk. Cloud sync concerns logical user state rather than media hosting.

---

## ADR-025 — Installable reduced-scope Windows 1.0

**Status:** Accepted

**Decision:** Windows 1.0 is a downloadable, signed, installable production application rather than an internal demo. It includes automatic collections, approved remote metadata/artwork, new-episode monitoring, Picture-in-Picture, and the screenshot gallery. AI, online subtitle providers, public user synchronization, authorized acquisition, and external plugin installation are post-1.0.

**Consequences:** The first release is useful on its own while preserving explicit expansion boundaries.

---

## ADR-026 — Favorite independent from rating

**Status:** Accepted

**Decision:** Favorite is an explicit first-class state with its own toggle. It does not set, clear, or derive from a star rating.

**Consequences:** Favorite rows and future synchronization require their own persisted state and tests.

---

## ADR-027 — Multi-episode physical files use segment mappings

**Status:** Accepted

**Decision:** One combined physical file remains one `MediaFile` and maps to multiple canonical episodes through ordered segment records. Chapters, validated runtimes, or manual mapping provide boundaries. Unknown boundaries produce combined-range playback without guessed per-episode progress; full completion completes all linked episodes.

**Consequences:** The domain and physical schema require a many-to-many segment mapping, while duplicate physical file rows are prohibited.

---

## ADR-028 — Playback continuation follows origin context

**Status:** Accepted

**Decision:** Next-episode autoplay is disabled by default but may be enabled. A Next Episode button remains available. When playback originates from a playlist, the playlist's next entry always wins over canonical episode, sequel, or franchise continuation.

**Consequences:** Playback sessions carry an origin context and playlist-entry identity.

---

## ADR-029 — Compact plain-text notes

**Status:** Accepted

**Decision:** Notes use bounded variable-length plain text with no HTML, Markdown, rich-text model, or client-side encryption requirement. RLS protects synchronized note rows; notes never enter logs, telemetry, AI, or provider requests.

**Consequences:** Storage and sync payloads remain small and simple; concurrent cloud conflicts still preserve both complete text versions.

---

## ADR-030 — Hungarian metadata fallback

**Status:** Accepted

**Decision:** Metadata resolution order in 1.0 is Hungarian, original language, then English.

**Consequences:** Provider selection and cache keys must support the three-stage fallback deterministically.

---

## ADR-031 — Daily provider-confirmed episode notification

**Status:** Accepted

**Decision:** MediaHub checks episode schedules after startup and once every 24 hours. Notification requires that the official air date has arrived and the approved provider reports Released/Available. Local-file availability is a separate state. In-app notification is retained; Windows toast is optional.

**Consequences:** Version 1.0 requires a reliable built-in metadata provider and deduplicated schedule state without requiring acquisition support.

---

## Pending prototype decisions

The following are intentionally pending implementation prototypes, not product questions:

- WPF versus WinUI 3;
- LibVLCSharp versus MPV;
- MSIX versus signed traditional installer or dual distribution;
- exact process-isolation technology for third-party plugins;
- local inference runtime for future semantic recommendations.

These decisions must be measured and recorded before production implementation.

---

## Document control

This document is part of the MediaHub revised baseline specification v1.2. Changes that affect architecture, security, data compatibility, plugin contracts, or user-visible behavior must be recorded in the Architecture Decision Record volume and reflected in the traceability matrix.
