# Volume 24 — Glossary and Traceability Matrix

**Project:** MediaHub  
**Document set:** MediaHub Project Bible  
**Status:** Revised baseline specification v1.2  
**Language:** English  
**Target platform:** Windows desktop first  
**Last updated:** 2026-08-08  

---

## 1. Glossary

| Term | Definition |
|---|---|
| Acquisition | Authorized process of obtaining a file through a provider or external client |
| Aggregate | Domain consistency boundary |
| Artwork | Poster, backdrop, logo, thumbnail, or screenshot asset |
| Canonical episode | Logical episode identity independent from filename |
| Collection | Group of titles, manual or rule-based |
| Completion threshold | Percentage after which an item is considered watched |
| Continue Watching | Row containing incomplete meaningful playback |
| Edition | Distinct cut/release of the same title |
| External identifier | Provider-specific identity |
| Fingerprint | File-derived identity evidence |
| Favorite | Explicit title state independent from star rating |
| Library root | Configured folder or network location |
| Logical title | Movie or series record that survives file changes |
| Media file | Physical playable file |
| Multi-episode segment | Ordered mapping from one physical file to one of several canonical episodes |
| Missing | Verified absence while logical record remains |
| Playable item | Movie or episode eligible for playback |
| Playlist | Ordered list of playable items |
| Provider | Extension supplying metadata, subtitles, search, or authorized acquisition |
| Reconciliation | Scan comparing database to file system |
| Resume thumbnail | Local frame capture near saved position |
| Sidecar | Metadata, artwork, or subtitle file beside media |
| Temporarily unavailable | Not reachable because root or file access is uncertain |
| Watch event | Historical record of active watching |
| Watcher | OS mechanism that reports file changes |

## 2. Product decision traceability

| Decision | Implemented in |
|---|---|
| No multiple profiles | Vol. 01, 05, ADR-003 |
| Netflix-inspired cinematic UI | Vol. 01, 06 |
| Cinematic player controls | Vol. 02, 06, 07 |
| Remote metadata/artwork in 1.0 with local fallback | Vol. 01, 09, ADR-025/030 |
| SMB/NAS support | Vol. 02, 11 |
| Built-in plugin foundation in 1.0; external installation later | Vol. 04, 12, ADR-011/012 |
| Post-1.0 AI learns only genre and quality | Vol. 14, ADR-013 |
| Five-star rating | Vol. 02, 05, 10 |
| Optional notes | Vol. 02, 05, 10, 15 |
| Favorite independent from rating | Vol. 02, 05, 10, ADR-026 |
| Provider-confirmed daily new-episode notification | Vol. 02, 09, 21, ADR-031 |
| Future automatic provider acquisition | Vol. 02, 13 |
| Space pause, ten-second seek, wheel volume | Vol. 02, 06, 07 |
| Screenshot fallback artwork | Vol. 06, 07, 09 |
| Ask on ambiguous same-title match | Vol. 02, 08 |
| Similar/same-director suggestions are deterministic | Vol. 14, ADR-013 |
| Automatic and custom collections | Vol. 02, 10 |
| Multiple collection membership | Vol. 05, 10 |
| User playlists | Vol. 02, 10 |
| Full statistics list | Vol. 10 |
| Fully customizable themes | Vol. 06, 16 |
| Picture-in-Picture in 1.0 | Vol. 06, 07, ADR-025 |
| Screenshot gallery with scene jump in 1.0 | Vol. 02, 06, 07, ADR-025 |
| Multi-episode segment mapping | Vol. 02, 05, 07, 30, ADR-027 |
| Playlist-origin continuation precedence | Vol. 02, 07, 10, ADR-028 |
| Future full mobile client | Vol. 22 |
| Online subtitle download post-1.0 and never auto-enabled | Vol. 07, 09, ADR-009 |
| Default file audio track | Vol. 07, ADR-010 |
| Real-time scanner | Vol. 08, ADR-006 |
| Delete to Recycle Bin, preserve history | Vol. 02, 05, 10, 15, ADR-007/008 |
| Optional update | Vol. 02, 18, ADR-014 |
| Hungarian 1.0 UI on localization-ready architecture | Vol. 16, ADR-015 |
| Windows 10 and 11 x64 support | Vol. 03, 18, ADR-001 |
| Long-term commercial ecosystem | Vol. 22 |

## 3. Requirement-to-test matrix

| Requirement area | Core tests |
|---|---|
| Library roots | onboarding integration and permission tests |
| Real-time scan | watcher/reconciliation scenarios |
| File matching | parser and scoring matrix |
| Network shares | disconnect, auth, reconnect, latency |
| Playback controls | UI and player adapter tests |
| Resume | checkpoint crash-recovery test |
| Completion | threshold boundary tests |
| Rating/note retention | deletion/restore end-to-end test |
| Favorite | independent toggle, retention, export, future sync tests |
| Collections | many-to-many and rule reevaluation |
| Playlists | ordering and missing-entry behavior |
| Subtitle behavior | embedded/sidecar/manual import in 1.0; provider download tests post-1.0 |
| Default audio | stream-selection fixture tests |
| Deletion | preview token, Recycle Bin, network capability |
| Updates | manifest signature, migration, decline path |
| Plugins | built-in capability/crash isolation and proof external installation is unavailable in 1.0 |
| AI | post-1.0 genre/quality learning, explanation, reset, privacy, no director affinity |
| Themes | token validation, contrast, pseudolocalization |
| Accessibility | keyboard and screen-reader walkthrough |
| Security | path, archive, executable, secret-redaction tests |

## 4. Cross-volume invariants

The following statements must remain true throughout implementation:

1. Logical user data is not owned by a file path.
2. Network unavailability is not deletion.
3. Local playback does not require internet or account.
4. Provider failure cannot disable the core library.
5. Downloaded content is never executed.
6. Plugins do not receive direct database access.
7. User notes do not enter logs, telemetry, or recommendation features.
8. Updates are optional for local operation.
9. Ambiguous matching is reviewed rather than guessed.
10. Destructive operations identify exact files and preserve logical history by default.
11. The UI thread does not perform blocking storage or network work.
12. Every background operation is bounded, observable, and cancellable where safe.
13. Theme and localization support are architectural, not post-processing.
14. Recommendation reasons must be truthful and explainable.
15. Migration occurs only after a verified backup.
16. Public Windows 1.0 does not upload personal user state or expose sign-in.
17. One multi-episode physical file is never duplicated into multiple physical records.
18. Playlist-origin playback follows playlist order.

## 5. Initial epic map

| Epic | Related volumes |
|---|---|
| Platform foundation | 03, 04, 05, 17, 20 |
| Cinematic desktop shell | 06, 16 |
| Local library | 02, 05, 08 |
| Playback | 07, 21 |
| Progress and organization | 10 |
| Metadata/artwork/subtitles | 09 |
| Network storage | 11 |
| Plugins/providers | 12, 15 |
| Downloads/acquisition | 13 |
| Recommendations | 14 |
| Updates/distribution | 18 |
| Quality and release | 19, 22 |

## 6. Baseline acceptance summary

A baseline production candidate is acceptable when a Windows user can:

- install the signed application;
- select local and SMB libraries;
- build and search the library;
- resolve ambiguous files;
- browse cinematic title pages;
- play common video formats;
- use standard controls;
- resume reliably;
- track movie, season, and series progress;
- rate and write notes;
- mark Favorites independently from ratings;
- create collections and playlists;
- use automatic collections;
- browse approved remote Hungarian-first metadata and artwork with local fallback;
- receive daily provider-confirmed new-episode notifications;
- use Picture-in-Picture and a screenshot gallery with scene jump;
- view statistics;
- use embedded, sidecar, and manually imported subtitles;
- delete local files to Recycle Bin without losing history;
- browse missing network titles offline;
- receive optional update notices;
- use the Hungarian UI and customizable themes;
- recover from plugin, network, or database problems without avoidable data loss.


## 7. Synchronization terminology

| Term | Definition |
|---|---|
| Local-first | Local operations commit and remain usable before any cloud synchronization succeeds |
| Outbox | Local transactional queue containing remote mutations to be retried |
| Remote shadow | Local record of the accepted remote revision used for conflict detection |
| Delta cursor | Last fully applied remote sequence or timestamp for a synchronized stream |
| Tombstone | Retained deletion marker allowing offline devices to observe a deletion |
| Idempotency key | Stable request identifier that makes retries safe |
| RLS | PostgreSQL Row Level Security restricting rows by authenticated identity |
| Edge Function | Trusted server-side function for secrets and privileged operations |
| Backend adapter | Infrastructure implementation of MediaHub-owned cloud interfaces |

Traceability additions:

| Requirement | Design owner | Verification |
|---|---|---|
| FR-SYNC-001–004 | Synchronization + SQLite UoW | Offline and crash tests |
| FR-SYNC-005–010 | Sync merge engine + UI | Multi-device integration tests |
| SEC-CLOUD-001–010 | Supabase adapter, RLS, Edge Functions | Policy/security test suite |
| NFR-CLOUD-001–010 | Scheduler, adapters, protocol | Resilience/performance tests |

The synchronization rows and tests are active in Development/Test builds using synthetic identities. Public Windows 1.0 exposes no personal synchronization; the later public feature synchronizes every supported user-owned category when enabled.

---

## Document control

This document is part of the MediaHub revised baseline specification v1.2. Changes that affect architecture, security, data compatibility, plugin contracts, or user-visible behavior must be recorded in the Architecture Decision Record volume and reflected in the traceability matrix.
