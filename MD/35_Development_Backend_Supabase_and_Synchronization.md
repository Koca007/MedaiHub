# Volume 35 — Development Backend, Supabase, and Synchronization

**Project:** MediaHub  
**Document set:** MediaHub Project Bible  
**Status:** Revised baseline specification v1.2  
**Language:** English  
**Target platform:** Windows desktop first  
**Last updated:** 2026-08-08  

---

## 1. Purpose

This volume defines how MediaHub uses Supabase as its development and testing backend without turning the Windows client into an online-only application. It establishes the local-first synchronization architecture, environment model, client boundaries, conflict behavior, operational rules, public 1.0 feature gate, and future backend replacement strategy.

## 2. Architectural objective

The Windows client must provide immediate local behavior while a hosted backend enables realistic testing of future cross-device features. Development/Test builds automatically provision a synthetic identity. Public Windows 1.0 exposes no sign-in and does not synchronize personal state. The architecture therefore combines:

- SQLite for local, device-specific, offline-critical state;
- Supabase PostgreSQL for synchronized user-owned state and cloud catalogs;
- Supabase Auth for one optional cloud identity;
- Realtime as a change hint;
- Edge Functions for privileged logic and secrets;
- Storage only for explicitly permitted small artifacts;
- a MediaHub-owned synchronization layer between local and remote systems.

The backend is active in Development and Test configurations but never becomes a prerequisite for local playback.

## 3. Core invariants

1. Every user command succeeds or fails locally before cloud synchronization is considered.
2. Playback never waits for authentication or a remote response.
3. Scanner state and file paths remain local.
4. A remote outage creates pending operations, not lost edits.
5. Supabase SDK models never enter Domain or Application assemblies.
6. Remote requests are idempotent or safely read-only.
7. Remote records are protected by RLS.
8. Privileged secrets never ship in the desktop client.
9. Signing out does not erase local user data.
10. Original media files are never automatically uploaded.
11. Public Windows 1.0 cannot enable personal synchronization through settings, remote flags, command-line arguments, or endpoint substitution.
12. Approved public 1.0 remote calls are limited to metadata, episode schedules, feature compatibility, and release information.

## 4. Environment topology

### 4.1 Local

Used by individual developers and CI integration tests.

```text
Developer workstation
├── MediaHub desktop process
├── SQLite database
├── Supabase CLI
└── Docker containers
    ├── PostgreSQL
    ├── Auth
    ├── Realtime
    ├── Storage
    └── Edge runtime
```

The `supabase/` directory is the source of truth for local cloud schema, functions, policies, and seed data.

### 4.2 Development

A hosted shared project used for team integration, manual QA, and demonstrations. It contains synthetic data only. Breaking schema experiments first run locally and are promoted through migrations.

### 4.3 Staging

An isolated hosted project representing the next public release. It receives release-candidate migrations and functions. Staging uses production-like policy settings but no production user data.

### 4.4 Production

A future environment chosen before commercial launch. It may use managed Supabase, self-hosted Supabase, a MediaHub ASP.NET Core API backed by PostgreSQL, or another implementation. The client contract remains stable.

Public 1.0 may use a production remote environment for approved non-personal capabilities. User-owned synchronization tables and identity workflows remain inaccessible from the public feature surface until the later synchronization release.

## 5. Application project boundaries

```text
MediaHub.Application
└── Abstractions
    ├── ICloudIdentityService
    ├── ICloudSyncService
    ├── IRemoteConfigurationService
    ├── IRemoteNotificationService
    ├── IReleaseCatalogService
    └── IRecommendationEventStore

MediaHub.Infrastructure.Supabase
├── Authentication
├── Database
├── Realtime
├── Storage
├── EdgeFunctions
├── Mapping
└── Resilience

MediaHub.Synchronization
├── Outbox
├── Pull
├── Merge
├── ConflictResolution
├── Protocol
└── Diagnostics
```

`MediaHub.Synchronization` is backend-neutral. It operates on MediaHub contracts. `MediaHub.Infrastructure.Supabase` implements remote transport and mapping.

## 6. Data classification

### 6.1 Never synchronized

- full local paths;
- drive letters and UNC topology;
- network usernames or credentials;
- raw media files;
- executable or plugin binaries;
- complete local logs;
- subtitle archives from untrusted providers;
- note content for AI processing;
- screenshots unless a separate explicit feature is enabled.

### 6.2 Synchronized by default in development/testing

- stable logical media identities;
- five-star ratings;
- compact plain-text user notes;
- favorites;
- manual collections and memberships;
- playlists and ordered entries;
- selected UI/player preferences that are device-independent;
- compact playback state summaries;
- post-1.0 recommendation genre/quality preference events;
- notification acknowledgements.

### 6.3 Remote authoritative catalogs

- feature flags;
- supported protocol versions;
- application release catalog;
- changelog entries;
- remote notification envelopes;
- post-1.0 server-generated recommendation candidates;
- Edge Function audit metadata.

## 7. Identity model

MediaHub has one local profile. Development/Test builds automatically associate it with a synthetic authenticated account. Public Windows 1.0 does not expose cloud identity. A later synchronization release may associate the local profile with one authenticated account; there is never a household profile switcher.

The local profile has a stable `LocalProfileId`. In Development/Test it is associated automatically with a synthetic `UserId`; in the future public flow, association occurs after explicit sign-in. A device has a stable `DeviceId` generated on first run and stored locally. Device names are user-editable and sanitized.

Authentication session material is stored through a Windows-protected credential service. Settings contain only non-secret state such as whether synchronization is enabled.

Future first sign-in performs a previewed merge of local and remote state and never silently replaces either side. Changing to a different cloud account requires an explicit detach workflow and cannot automatically upload the existing local profile to the new account.

## 8. Local transactional outbox

A synchronized edit uses a single SQLite transaction:

1. load aggregate;
2. validate command;
3. update local aggregate;
4. increment local revision;
5. serialize an allow-listed synchronization payload;
6. insert `SyncOutboxMessage`;
7. commit;
8. publish local read-model invalidation;
9. return success to UI.

A cloud request never occurs inside this transaction.

### 8.1 Outbox properties

- globally unique message ID;
- aggregate type and stable ID;
- operation type;
- payload schema version;
- local revision/sequence;
- creation time;
- idempotency key;
- retry count and next-attempt time;
- last error classification;
- acknowledgement state.

### 8.2 Coalescing

High-frequency operations may be coalesced:

- multiple playback checkpoints become the newest summary;
- repeated rating edits keep the newest unsent value;
- collection add followed by remove may cancel before upload;
- note edits are not discarded when a conflicting remote version exists.

Coalescing never changes already-sent or ambiguous operations.

## 9. Push synchronization

The push worker:

1. verifies synchronization is enabled;
2. ensures a valid session;
3. checks protocol compatibility;
4. selects a bounded ordered batch;
5. maps payloads to remote DTOs;
6. sends idempotent mutations;
7. receives server revisions and timestamps;
8. acknowledges messages transactionally;
9. updates remote shadows;
10. schedules a pull when necessary.

Retry categories:

- transient connectivity: retry with exponential backoff and jitter;
- authentication: attempt refresh, then pause for sign-in;
- validation: dead-letter immediately;
- policy denied: pause affected stream and surface diagnostics;
- protocol mismatch: disable affected cloud capability;
- unknown outcome: retry with the same idempotency key.

## 10. Pull synchronization

A pull is triggered by:

- startup after local database readiness;
- periodic timer;
- successful sign-in;
- Realtime change hint;
- successful push;
- user-requested retry;
- application resume after extended sleep.

The client queries authorized changes after its last cursor. Deltas are ordered by server sequence. Each batch is validated and applied in a SQLite transaction together with the advanced cursor.

A Realtime event does not directly mutate the local database. It only schedules a pull, because events may be duplicated, omitted, delayed, or delivered out of order.

## 11. Conflict resolution

### Rating

The latest explicit edit wins using server-accepted ordering. Diagnostics retain the replaced value for a limited period.

### Note

Concurrent note changes produce a `SyncConflict`. Both texts are preserved. The UI provides local, remote, and manual merge options.

Notes are bounded compact plain text. They are not HTML/Markdown documents and are not client-side encrypted by the baseline; authentication and RLS protect remote access.

### Favorite

The latest accepted explicit toggle wins. Favorite remains independent from rating, and diagnostics retain the replaced state for a bounded period.

### Playback

The furthest credible position generally wins. A later explicit “restart from beginning,” “mark unwatched,” or manual completion action has higher semantic priority than distance.

### Collections

Membership uses stable membership IDs and tombstones. Independent additions merge. Add/remove races use revision ordering and retain diagnostics.

### Playlists

Entries have stable IDs and sortable position keys. Concurrent reorder may be deterministically normalized. Destructive ambiguity is surfaced for review.

### Preferences

Merge by field. Device-only settings never sync. Theme packages are not downloaded automatically from another device without trust validation.

## 12. Tombstones and retention

A synchronized deletion creates a tombstone with:

- aggregate ID;
- user ID;
- device ID;
- deletion revision;
- server timestamp;
- entity type.

Tombstones remain long enough for supported offline clients to receive them. Permanent cleanup is server maintenance and must respect supported offline duration and backup policy.

Deleting a physical video file only changes local availability. It does not create a remote title deletion.

## 13. Remote protocol compatibility

The client declares:

- protocol version;
- minimum supported remote version;
- maximum supported remote version;
- capabilities;
- application version;
- schema payload versions.

The backend exposes a compatibility snapshot. Additive fields are ignored safely. Breaking changes require a new protocol version and a compatibility period.

When incompatible:

- local features remain active;
- affected sync streams pause;
- queued local edits remain preserved;
- UI identifies which cloud features require an update;
- release catalog shows available changes.

## 14. Remote configuration and feature flags

Feature flags are remotely authoritative but cached locally. Flags may:

- enable a development-only feature;
- disable a failing cloud integration;
- select recommendation algorithm version;
- control provider rollout;
- define minimum protocol versions;
- enable diagnostics sampling with user privacy rules.

Flags may not disable core local playback or erase local data. The application uses last-known-good values when the backend is unavailable.

No remote flag may enable personal synchronization, public sign-in, AI event upload, online subtitle providers, acquisition, or external plugin installation in a Windows 1.0 production build.

## 15. Release catalog

Supabase stores application releases and changelog sections. The client checks periodically and displays:

- latest compatible version;
- release date;
- channel;
- security/reliability/functionality classification;
- new features;
- unavailable cloud features for the installed version;
- download location resolved through the trusted update system.

The catalog does not force installation. Update package authenticity remains governed by the update volume.

## 16. Post-1.0 AI event storage

After the recommendation milestone starts, the development backend may record compact genre/quality recommendation events to validate future cloud recommendations. Public Windows 1.0 records none. Events use controlled enums and stable media IDs. Notes, paths, filenames, free-form subtitle text, and learned director/cast affinity are excluded.

Server-generated recommendation candidates contain:

- media identity;
- score;
- explanation code and localized parameters;
- algorithm version;
- generated time;
- expiry;
- source features;
- optional diversity group.

The future desktop module combines remote candidates with local recommendations and can disable remote AI completely. Learned features remain limited to genre and quality; ratings and watch behavior are weighting signals.

## 17. Observability

Client metrics are local by default. Development builds may report sanitized operational metrics to the development backend:

- sync duration;
- batch size;
- result class;
- protocol version;
- retry count;
- function name;
- anonymous build/device correlation permitted by test policy.

Never include title names, notes, paths, tokens, or media filenames.

## 18. Failure behavior

| Failure | User impact | System response |
|---|---|---|
| No internet | Cloud status unavailable | Queue edits, continue locally |
| Expired token | Sync paused briefly | Refresh or request sign-in |
| Supabase outage | No remote updates | Backoff, preserve outbox |
| RLS denial | Affected operation blocked | Quarantine and diagnose |
| Invalid remote payload | No unsafe apply | Reject batch item, log safely |
| Realtime disconnected | Slower updates | Polling continues |
| Protocol incompatible | New cloud features unavailable | Local app remains usable |
| Conflict | Possible review badge | Preserve data and resolve by policy |

In public Windows 1.0, identity/token/outbox rows above are not part of the user experience because personal synchronization is disabled. Failures in approved non-personal remote services degrade only those services.

## 19. Testing requirements

- run all policy tests against local Supabase;
- test with network cut during every synchronization phase;
- terminate process after remote commit but before local ack;
- simulate duplicate and out-of-order deltas;
- test 30 days of offline edits;
- test clock skew;
- test stale client compatibility;
- test environment-key mismatch;
- verify no forbidden fields appear in remote rows or request logs;
- verify playback with all remote services stopped.
- verify production 1.0 cannot expose sign-in or enable personal synchronization;
- verify Development/Test synthetic identity provisioning is deterministic and isolated;
- verify future first-sign-in merge preview and account-detach safety.

## 20. Future backend replacement

The replacement plan is:

1. preserve MediaHub remote contracts;
2. export Supabase records using stable MediaHub IDs;
3. implement a new infrastructure adapter or compatible API;
4. run dual-read or migration validation in staging;
5. change composition-root registration and endpoint configuration;
6. retain SQLite and synchronization semantics;
7. retire Supabase-specific functions only after compatibility verification.

Supabase is therefore a production-capable option, not an irreversible dependency.

---

## Document control

This document is part of the MediaHub revised baseline specification v1.2. Changes that affect synchronization semantics, remote schema compatibility, identity, security, or user-visible cloud behavior require an Architecture Decision Record and migration/test updates.
