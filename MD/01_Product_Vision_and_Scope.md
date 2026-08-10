# Volume 01 — Product Vision and Scope

**Project:** MediaHub  
**Document set:** MediaHub Project Bible  
**Status:** Revised baseline specification v1.2  
**Language:** English  
**Target platform:** Windows desktop first  
**Last updated:** 2026-08-08  

---

## 1. Product vision

MediaHub provides one coherent place to discover, organize, play, track, and understand a personal video library. The application removes the need to browse raw folders, remember episode positions, manually group seasons, or maintain separate notes and ratings.

The product must feel immediate and cinematic. The default experience is visual: large artwork, horizontal content rows, smooth focus transitions, and a player that hides controls when they are not needed. The design may be inspired by established streaming applications, but it must use original branding, layouts, icons, copy, and interaction details.

## 2. Product principles

### 2.1 Offline-first

Core functionality must not depend on an account, a cloud service, or continuous internet access. Local playback, library browsing, progress tracking, notes, ratings, collections, playlists, and settings remain available offline.

### 2.2 User-owned data

Library state belongs to the user. The database must be exportable in documented formats. Ratings, notes, progress, and collections must not be lost when a file is temporarily unavailable or deleted.

### 2.3 Modular by default

Playback, scanning, metadata, recommendations, downloads, and integrations are separate modules behind explicit contracts. This prevents a provider failure from destabilizing local playback.

### 2.4 Safe automation

Automation must be visible, reversible when practical, and controllable. Examples include metadata matching, subtitle downloads, file imports, and future acquisition providers. The user can inspect why an action occurred.

### 2.5 Explainable intelligence

Recommendations must provide understandable reasons such as “because you frequently watch science fiction” or “available in the quality you usually choose.” A deterministic row may also say that a title has the same director as one named source title, but this does not become learned director affinity. AI must never silently modify or delete the library.

## 3. Target audience

### Primary audience

- Windows users with locally stored movies and series.
- Users with media distributed across internal disks, external disks, and NAS/SMB shares.
- Users who value a polished streaming-style interface but prefer local ownership and offline access.
- Users who watch multi-season series and need reliable progress tracking.

### Secondary audience

- Home theater PC users.
- Advanced users who want plugins and custom themes.
- Developers who may build metadata or library extensions.
- Future commercial users who require stable updates and support.

## 4. Product boundaries

### In scope for the first production release

- Downloadable and installable native Windows 10/11 x64 desktop client.
- Single local application profile.
- Local and SMB/NAS libraries.
- Film, series, season, and episode organization.
- Embedded video playback using a proven playback engine.
- Resume, watched state, and progress calculation.
- Five-star ratings and private notes.
- Automatic and manual collections.
- A first-class Favorite state independent from rating.
- Manual playlists.
- Real-time folder monitoring plus reconciliation.
- Approved remote metadata, Hungarian-first descriptions, artwork, episode schedules, and local/generated fallbacks.
- Embedded, sidecar, and manual subtitle import; provider-based subtitle search/download remains post-1.0.
- Plugin foundation with versioned contracts and approved built-in extensions; no public third-party package installation.
- Theme customization and a Hungarian user interface built on localization-ready resources.
- Picture-in-Picture without a second playback session.
- Manual screenshot gallery with scene timestamps and “Jump to scene.”
- Update notifications without mandatory upgrades.
- Diagnostics, logging, backup, import, and export.

### Planned after the first production release

- Online subtitle-provider search and download.
- Authorized provider-based acquisition automation.
- Local AI recommendations that learn genre and quality preference.
- Full mobile client.
- Optional user-facing cross-device synchronization.
- Public third-party `.mhpkg` installation, plugin discovery, or marketplace.
- Web companion or remote control.
- Commercial licensing and support tiers.

### Explicitly outside the core product

- Hosting or distributing copyrighted media.
- Circumventing access controls, paywalls, authentication, DRM, CAPTCHA, or technical protection measures.
- Executing downloaded programs or scripts.
- Replacing a general-purpose torrent client or web browser.
- Multi-user household profiles in the first Windows client.

## 5. Success criteria

The product is successful when a user can install it, select media folders, and obtain a usable library without manually entering every title. Playback must begin quickly, progress must persist reliably, and the application must remain usable when network shares or online services are unavailable.

Key success measurements:

| Area | Target |
|---|---|
| Cold start with an existing medium library | under 3 seconds to interactive shell |
| Local search after indexing | typical result under 100 ms |
| Resume position loss | zero under normal shutdown and recoverable after abnormal exit |
| False file deletion | zero; deletion requires explicit confirmation |
| Scanner duplication | less than 0.1% after reconciliation |
| Crash-free sessions | at least 99.5% during stable release |
| Core offline availability | 100% without sign-in |
| Accessibility | keyboard-complete primary workflows |
| Upgrade safety | automatic backup before schema migration |

## 6. Business rules

- A media title can exist without a currently available file.
- One title can have multiple media files and editions.
- A normal media file maps to one canonical playable item. A verified multi-episode file may map to several canonical episodes through explicit ordered segments while remaining one physical file record.
- A movie or series can belong to multiple collections and playlists.
- Favorite state is independent from rating and collection membership.
- A collection may be automatic or manual.
- A playlist preserves user order unless explicitly sorted.
- An episode is completed when playback reaches the configurable completion threshold; default 90%.
- A film's rating and note belong to the logical title, not the physical file.
- A series rating belongs to the series. Optional episode ratings may be added later without changing the series rating.
- Subtitle download does not imply automatic subtitle activation.
- Default audio behavior respects the media file's default track flag unless the user explicitly selects another track.
- Deleting from MediaHub moves files to the Windows Recycle Bin by default and marks the related file record unavailable.
- Update availability does not block application use unless a future backend protocol has an unavoidable minimum version; local features remain usable.
- New features unavailable on an older build must be clearly identified rather than failing silently.
- When playback was started from a playlist, playlist order overrides canonical series continuation.
- Automatic next-episode playback is disabled by default. A Next Episode action is always available when a canonical successor exists, and autoplay may be enabled explicitly.

## 7. Product terminology

- **Title:** canonical movie or series record.
- **Playable item:** movie, episode, trailer, or future bonus content that can be opened by the player.
- **Media file:** physical or network file containing audiovisual data.
- **Edition:** a distinct cut or release of the same title, such as theatrical or director’s cut.
- **Library root:** configured local folder or network share.
- **Missing item:** a logical record with no currently reachable media file.
- **Provider:** extension that supplies metadata, subtitles, search results, or authorized acquisition information.
- **Plugin:** broader extension package loaded through the MediaHub SDK.
- **Reconciliation:** scan that compares the database with the actual file system to repair missed file-system events.

## 8. User promise

MediaHub must never make the user fear losing their history or organization. Files may move, disks may disconnect, and metadata services may fail, but the application should preserve the user's ratings, notes, progress, collections, playlists, and decisions.


## 9. Development backend strategy

MediaHub adopts a local-first hybrid model from the beginning:

- SQLite remains the authoritative store for device-specific library state, paths, scan results, playback-critical checkpoints, local artwork, and offline operation.
- Supabase PostgreSQL is the development and testing backend for synchronized ratings, compact plain-text notes, favorites, collections, playlists, watch summaries, preferences, future recommendation events, feature flags, application releases, metadata proxy operations, and notifications.
- Development and test builds use an automatically provisioned synthetic identity. The public 1.0 client does not expose sign-in or user-data synchronization.
- Supabase Storage may hold small cloud assets such as exported settings, diagnostics explicitly uploaded by the user, or future synced artwork; original video files are never uploaded automatically.
- Supabase Realtime may invalidate or refresh synchronized read models but is not trusted as the only delivery mechanism.
- Supabase Edge Functions contain privileged operations and third-party secrets.
- The application layer depends on MediaHub-owned interfaces, not Supabase client types.

The product may later retain Supabase, place a MediaHub ASP.NET Core API in front of it, or replace it entirely. This replacement must not require changes to playback, scanning, domain entities, or desktop UI use cases.

## 10. Cloud feature availability rule

Cloud-connected features are optional capabilities. A compatible local application continues to function when:

- Supabase is unavailable;
- the user is signed out;
- synchronization is disabled;
- a remote schema is temporarily newer than the client;
- a feature flag disables a cloud feature;
- the installed version no longer supports a newly introduced remote-only feature.

The UI must identify unavailable cloud features without presenting the local library as broken.

## 11. Version 1.0 release definition

Version 1.0 is a user-installable production application with a signed installer, uninstall support, optional updates, recovery tooling, and documented limitations. It is intentionally smaller than the long-term MediaHub product but is not an internal demo. Public 1.0 uses remote services only for approved non-personal capabilities such as metadata, episode schedules, feature compatibility, and release information. Development/test synchronization remains available behind internal configuration for validating future cross-device behavior.

---

## Document control

This document is part of the MediaHub revised baseline specification v1.2. Changes that affect architecture, security, data compatibility, plugin contracts, or user-visible behavior must be recorded in the Architecture Decision Record volume and reflected in the traceability matrix.
