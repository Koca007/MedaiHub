# Volume 27 — Background Jobs, Caching and Performance

**Project:** MediaHub  
**Document set:** MediaHub Project Bible  
**Status:** Revised baseline specification v1.2  
**Language:** English  
**Target platform:** Windows desktop first  
**Last updated:** 2026-08-08  

---

## 1. Job scheduler

The scheduler coordinates work that may consume CPU, disk, network, database, or external-process resources.

Resource classes:

- PlaybackCritical
- Interactive
- DiskIntensive
- NetworkIntensive
- CpuIntensive
- Maintenance
- Plugin

Each job declares resource classes and estimated cost.

## 2. Priority

| Priority | Examples |
|---|---|
| 0 Critical | save playback checkpoint, database recovery |
| 10 Interactive | user-triggered rescan, subtitle search |
| 20 Responsive | watcher-discovered file |
| 30 Normal | metadata refresh |
| 40 Background | artwork generation |
| 50 Maintenance | reconciliation, cache cleanup |

`RefreshEpisodeSchedule` runs once after the shell becomes interactive and then no more frequently than every 24 hours. A persisted last-success/last-attempt timestamp prevents repeated startup checks.

Priority inversion is prevented by short jobs and resource semaphores rather than one giant queue.

## 3. Bounded concurrency

Example limits, configurable and benchmarked:

- ffprobe: 2–4 concurrent local jobs;
- network probes: 1–2 per share;
- artwork decode: bounded by memory;
- metadata HTTP: provider-specific;
- full hash: one per physical disk where identifiable;
- database write batches: serialized.

## 4. Playback-aware mode

When playback is active:

- reduce scanner concurrency;
- pause full hashing;
- reduce network downloads;
- defer cache cleanup;
- retain lightweight metadata requests only if they do not affect playback.

The user may disable this behavior.

## 5. Job persistence

Persist:

- downloads;
- long scans;
- migration stages;
- provider bulk refresh;
- resumable imports.

Do not persist:

- trivial UI refresh;
- short artwork decode;
- transient search.

Persisted jobs must tolerate application version changes through payload schemas.

## 6. Retry policy

Retry only transient failures:

- network timeout;
- temporary share outage;
- HTTP 429/5xx according to provider;
- file sharing violation;
- temporary database busy.

Do not retry automatically:

- permission denied;
- unsupported codec;
- invalid signature;
- schema error;
- checksum mismatch without re-download decision;
- explicit provider rejection.

Backoff uses jitter and maximum attempts.

## 7. Cancellation

Jobs define cancellation points. A database batch finishes or rolls back. File commit cannot be interrupted after atomic rename begins. UI reports “Cancelling” until safe stop.

## 8. Cache categories

### Metadata cache

Keyed by provider, entity, locale, and version. TTL according to content type.

### Artwork cache

Content-addressed with derived sizes.

### Search cache

Short-lived in-memory query results; invalidated by relevant events.

### Read-model cache

Home rows and details projections with event invalidation.

### Probe cache

Keyed by file fingerprint, size, and modified time.

### Recommendation cache

Ranked row plus model version and profile version.

This cache category is inactive in public Windows 1.0 and becomes available with the post-1.0 recommendation module.

### HTTP cache

Provider-specific headers and policy.

## 9. Cache rules

- cache is never the only copy of user data;
- cache can be cleared safely;
- user-selected artwork originals may be pinned;
- cache keys include schema/version;
- stale data can be served offline where safe;
- negative cache prevents repeated missing-provider requests for a short period;
- failures are not cached indefinitely.

## 10. Artwork memory

UI requests target pixel size. Image service:

- selects closest cached derivative;
- decodes off UI thread;
- limits parallel decode;
- uses weak or size-bounded memory cache;
- cancels off-screen requests;
- disposes native resources;
- prefetches only nearby cards.

## 11. Database query performance

- projections, not full graphs;
- pagination/keyset pagination;
- indexes verified with query plans;
- no N+1;
- aggregate statistics precomputed where justified;
- read-only no-tracking queries;
- batch updates;
- FTS for search.

## 12. Startup budget

Startup stages:

1. process and crash marker;
2. configuration load;
3. DI composition;
4. database lightweight open/check;
5. shell display;
6. read-model load;
7. background recovery/reconciliation;
8. provider/update checks after interactive.

Remote work never blocks shell.

## 13. Performance budgets

Illustrative budgets:

| Operation | Target |
|---|---:|
| Shell interactive | <3 s |
| Home cached render | <500 ms |
| Search first results | <100 ms |
| Details local data | <250 ms |
| Progress checkpoint | <50 ms database time typical |
| Watcher event enqueue | <10 ms |
| Card scroll frame | 60 FPS target on reference machine |
| Theme switch | <500 ms without restart where supported |

## 14. Scanner throughput measurement

Record:

- files enumerated/sec;
- probe duration;
- parse duration;
- database batch duration;
- match confidence distribution;
- watcher backlog;
- network share latency.

No media titles are sent in telemetry.

## 15. Backpressure

If queues grow:

- stop low-priority producers;
- aggregate duplicate watcher events;
- show degraded status;
- prioritize newest user-visible files;
- persist resumable checkpoint;
- avoid unbounded memory.

## 16. Maintenance windows

Maintenance is opportunistic when idle:

- vacuum only when beneficial;
- search rebuild;
- orphan cache cleanup;
- old log cleanup;
- backup rotation;
- provider cache expiry.

Never run expensive maintenance during playback by default.

## 17. Performance regression process

- benchmark baseline in source control;
- compare pull requests for hot paths;
- investigate >10% regression or threshold breach;
- capture profiler evidence;
- document justified tradeoff;
- test on SSD and network share;
- include cold and warm cache.

## 18. Acceptance criteria

- Remote provider does not delay startup.
- Queue growth is bounded.
- Playback reduces background load.
- Off-screen artwork requests are cancelled.
- Clearing cache preserves user data.
- Scanner overflow does not consume unbounded memory.
- Search meets target on reference dataset.


## 19. Synchronization jobs

These jobs are enabled in Development/Test builds with synthetic identities. Public Windows 1.0 keeps personal synchronization jobs disabled; non-personal metadata, episode schedule, compatibility, and release refresh jobs remain separate.

Jobs:

- `SyncPushOutboxBatch`
- `SyncPullDelta`
- `RefreshAuthenticationSession`
- `RefreshRemoteConfiguration`
- `RefreshReleaseCatalog`
- `ReconcileRemoteShadows`
- `RetryDeadLetterItem`

Synchronization uses a single logical coordinator per local profile. Push and pull may overlap only when aggregate ordering remains safe. Playback-critical jobs retain priority. Backend outages trigger exponential backoff with jitter and a maximum retry interval; user-requested retry may bypass the delay once.

Realtime messages are coalesced into a delta pull rather than processed one row at a time. Remote configuration and release data use cache-control and last-known-good snapshots.

---

## Document control

This document is part of the MediaHub revised baseline specification v1.2. Changes that affect architecture, security, data compatibility, plugin contracts, or user-visible behavior must be recorded in the Architecture Decision Record volume and reflected in the traceability matrix.
