# Volume 04 — System Architecture

**Project:** MediaHub  
**Document set:** MediaHub Project Bible  
**Status:** Revised baseline specification v1.2  
**Language:** English  
**Target platform:** Windows desktop first  
**Last updated:** 2026-08-08  

---

## 1. Architectural style

MediaHub uses Clean Architecture with explicit dependency direction:

```text
Presentation
    ↓
Application
    ↓
Domain

Infrastructure → Application abstractions
Player adapter → Application abstractions
Plugins → Versioned SDK contracts
```

The Domain project has no dependency on UI, database, operating-system, media-engine, HTTP, or plugin implementations.

## 2. Recommended solution layout

```text
MediaHub.sln
├── src/
│   ├── MediaHub.Desktop
│   ├── MediaHub.Presentation
│   ├── MediaHub.Application
│   ├── MediaHub.Domain
│   ├── MediaHub.Infrastructure
│   ├── MediaHub.Persistence
│   ├── MediaHub.Player
│   ├── MediaHub.Scanner
│   ├── MediaHub.Metadata
│   ├── MediaHub.NetworkStorage
│   ├── MediaHub.Downloads
│   ├── MediaHub.Recommendations
│   ├── MediaHub.PluginHost
│   ├── MediaHub.PluginSdk
│   └── MediaHub.Contracts
├── tests/
│   ├── MediaHub.Domain.Tests
│   ├── MediaHub.Application.Tests
│   ├── MediaHub.Persistence.Tests
│   ├── MediaHub.Scanner.Tests
│   ├── MediaHub.Player.Tests
│   ├── MediaHub.PluginCompatibility.Tests
│   └── MediaHub.Desktop.UiTests
├── tools/
│   ├── MediaHub.DatabaseTool
│   ├── MediaHub.PluginValidator
│   └── MediaHub.LibraryInspector
└── docs/
```

## 3. Layer responsibilities

### Domain

Owns business concepts and invariants:

- titles, movies, series, seasons, episodes;
- media files, editions, and multi-episode segment mappings;
- watch state;
- ratings and notes;
- favorites;
- collections and playlists;
- matching decisions;
- library-root identity;
- notification domain rules.

Domain entities expose behavior rather than public setters. Domain events describe meaningful changes.

### Application

Coordinates use cases:

- add library root;
- scan library;
- resolve ambiguous item;
- start playback;
- save progress;
- delete media to Recycle Bin;
- create collection;
- search library;
- refresh metadata;
- execute provider search;
- check updates.

Version 1.0 includes approved built-in remote metadata through the provider boundary. Public online subtitle providers, AI recommendations, user-data synchronization, acquisition providers, and public third-party plugin installation remain feature-gated post-1.0 capabilities.

Application uses commands and queries but does not require a heavy CQRS framework. Commands mutate state; queries return read models optimized for UI.

### Infrastructure

Implements:

- file-system access;
- Windows Recycle Bin;
- credential protection;
- network connectivity;
- HTTP clients;
- image caching;
- FFprobe execution;
- update service;
- system notifications;
- clock and environment services.

### Persistence

Implements repositories, migrations, transactions, indexes, backup, and database integrity checks. Entity Framework Core may be used internally, but domain entities must not be designed around EF requirements.

### Player

Wraps LibVLCSharp or the selected playback engine behind `IMediaPlayer`. Playback-engine-specific types do not cross the boundary.

### Scanner

Implements path enumeration, watcher events, parsing, fingerprinting, probing, match scoring, and reconciliation. Scanner emits application-level candidates, not direct UI changes.

### Plugin host

Registers approved built-in extensions, validates their manifests and contracts, resolves compatibility, applies capability restrictions, and collects health status. Extension contracts live in a stable SDK assembly. Package discovery and validation tooling may exist for developers in 1.0, but the public client does not install arbitrary third-party `.mhpkg` packages until the hardened external-plugin milestone.

## 4. Dependency injection

The composition root lives in the desktop executable. Each module exposes one registration method:

```csharp
public static class PlayerModule
{
    /// <summary>
    /// Registers the player abstraction and its runtime dependencies.
    /// </summary>
    public static IServiceCollection AddMediaHubPlayer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Registration only; no runtime work is started here.
        return services;
    }
}
```

Rules:

- no service locator;
- no global mutable singleton;
- singleton lifetime only for stateless, thread-safe coordinators or process-wide resources;
- scoped unit of work per command;
- background services have explicit start/stop ownership;
- optional modules register null-object or unavailable implementations where useful.

## 5. Messaging

In-process domain and integration events decouple modules.

Examples:

- `MediaFileDiscovered`
- `MediaFileBecameUnavailable`
- `PlaybackProgressCheckpointed`
- `PlayableItemCompleted`
- `MetadataUpdated`
- `NewEpisodeKnown`
- `PluginHealthChanged`
- `UpdateAvailable`

Events are immutable. Delivery is at-least-once within the local process where persistence is involved; handlers must be idempotent. Critical events use an outbox table committed with the originating transaction.

## 6. Background work

Background tasks use a central scheduler with bounded queues. Each job includes:

- stable job ID;
- type;
- priority;
- correlation ID;
- cancellation support;
- retry policy;
- progress;
- user-visible label;
- resumability classification.

Priority order:

1. playback-critical work;
2. user-requested actions;
3. file-system event processing;
4. metadata and artwork;
5. reconciliation;
6. maintenance.

Scanning and artwork work must reduce activity during playback when configured.

## 7. Read models

The cinematic UI should not hydrate large domain graphs. Dedicated read models include:

- home rows;
- movie card;
- series card;
- details page;
- season episode list;
- continue-watching card;
- search result;
- notification;
- download queue;
- diagnostics status.

Read models may be cached and rebuilt from domain state.

## 8. Error model

Application operations return typed outcomes or throw only well-defined exceptions at module boundaries.

Error categories:

```csharp
public enum ErrorCategory
{
    Validation,
    NotFound,
    Conflict,
    Permission,
    Connectivity,
    Unsupported,
    ProviderFailure,
    DataIntegrity,
    Cancelled,
    Unexpected
}
```

Expected errors do not use exceptions for normal control flow in hot paths. Unexpected exceptions are caught at process and command boundaries, logged with correlation IDs, and translated into user-safe messages.

## 9. Extensibility boundaries

Extension points are allowed only where versioned contracts exist:

- metadata search and retrieval;
- subtitle search and retrieval;
- authorized acquisition search and resolution;
- artwork transformations;
- import/export;
- recommendation contributors;
- dashboard row providers, after security review;
- theme packages.

Plugins cannot receive unrestricted service-provider access. They receive narrow host services and declared capabilities.

In public 1.0 these contracts are consumed only by approved built-in extensions. External package loading, user-facing permission approval, and third-party uninstall workflows are deferred without changing the SDK boundary.

## 10. Data flow example: playback

```text
UI command
→ PlayPlayableItem use case
→ Resolve preferred available MediaFile
→ Create PlaybackSession
→ Player adapter opens file
→ Player emits position events
→ Progress coordinator throttles checkpoints
→ Repository persists PlaybackState
→ Read-model invalidation event
→ Continue Watching refresh
```

## 11. Graceful degradation

- Without internet: local features remain available.
- Without metadata provider: local title and generated frame artwork are used.
- Without network share: files become unreachable, not deleted.
- Without plugin: plugin-supplied features show unavailable state.
- Without the post-1.0 AI module: Home omits the personalized recommendation row; deterministic “similar title” metadata may still appear on details pages.
- Without update service: application works and records the check failure.
- Without FFprobe: basic filename indexing works, but technical fields show unknown.

## 12. Architectural constraints

- UI must never access the database directly.
- Scanner must never mutate playback state.
- Provider packages must never receive raw database connections.
- Download manager must never execute files.
- Deleting a physical file must not delete the logical title aggregate.
- Network reachability is not equivalent to file deletion.
- Plugin contracts are versioned independently of application version.
- Domain identifiers are stable UUIDs suitable for active local-to-cloud synchronization.
- A multi-episode physical file remains one `MediaFile` and relates to several canonical episodes through validated ordered segment records.


## 13. Local-first backend architecture

```text
MediaHub.App
    │
    ├── MediaHub.Application abstractions
    │       ├── ICloudIdentityService
    │       ├── ICloudSyncService
    │       ├── IRemoteConfigurationService
    │       ├── IReleaseCatalogService
    │       └── IRecommendationEventStore
    │
    ├── MediaHub.Infrastructure.Local
    │       ├── SQLite
    │       ├── File system
    │       ├── Windows credentials / DPAPI
    │       └── Sync outbox and local checkpoints
    │
    └── MediaHub.Infrastructure.Supabase
            ├── Auth adapter
            ├── PostgREST/database adapter
            ├── Realtime adapter
            ├── Storage adapter
            ├── Edge Function adapter
            └── Delta synchronization implementation
```

Local SQLite is authoritative for device-owned state. Supabase is authoritative for accepted synchronized records. The synchronization layer reconciles the two without allowing either persistence technology to leak into the domain model.

## 14. Revised solution structure

```text
src/
├── MediaHub.App/
├── MediaHub.Domain/
├── MediaHub.Application/
├── MediaHub.Infrastructure.Local/
├── MediaHub.Infrastructure.Supabase/
├── MediaHub.Synchronization/
├── MediaHub.Player/
├── MediaHub.Scanner/
├── MediaHub.Metadata/
├── MediaHub.Plugins/
└── MediaHub.Shared.Contracts/

supabase/
├── config.toml
├── migrations/
├── seed.sql
├── functions/
└── tests/
```

`MediaHub.Shared.Contracts` contains MediaHub-owned remote DTOs and protocol constants only. It must not reference UI, EF Core entities, or Supabase SDK models.

## 15. Synchronization boundary

The synchronization subsystem owns:

- local outbox capture;
- remote delta cursors;
- idempotency keys;
- conflict policies;
- remote-to-local mapping;
- retry scheduling;
- authentication refresh coordination;
- protocol compatibility checks;
- sync diagnostics.

It does not own playback, media scanning, file deletion, media matching, or metadata-provider behavior.

## 16. Data ownership matrix

| Data | Local SQLite | Supabase | Synchronization |
|---|---|---|---|
| File paths and storage roots | authoritative | never stored | none |
| Probe results and track inventory | authoritative | not required | none |
| Playback checkpoint | authoritative | summarized/optional | local-first |
| Rating and note | authoritative while offline | synchronized canonical copy | bidirectional |
| Favorite state | authoritative while offline | synchronized canonical copy | bidirectional |
| Collections and playlists | local working copy | synchronized canonical copy | bidirectional |
| Feature flags and release catalog | cached | authoritative | remote-to-local |
| AI genre/quality events | local queue/profile | synchronized events/profile | local-to-remote and delta |
| Credentials and secrets | secure OS storage | server secret store | references only |

Public Windows 1.0 uses Supabase for approved non-personal remote capabilities such as metadata proxying, episode schedules, feature compatibility, and the release catalog. The synchronized user-data rows and workers are enabled only in Development/Test builds until the later public synchronization release.

## 17. Backend replacement rule

No application use case may directly call Supabase. All calls pass through MediaHub-owned interfaces. Replacing Supabase with an ASP.NET Core service or another backend must be possible by replacing infrastructure registrations and remote contracts, while preserving domain behavior and local database compatibility.

---

## Document control

This document is part of the MediaHub revised baseline specification v1.2. Changes that affect architecture, security, data compatibility, plugin contracts, or user-visible behavior must be recorded in the Architecture Decision Record volume and reflected in the traceability matrix.
