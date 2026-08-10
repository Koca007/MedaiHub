# Volume 03 — Non-Functional Requirements

**Project:** MediaHub  
**Document set:** MediaHub Project Bible  
**Status:** Revised baseline specification v1.2  
**Language:** English  
**Target platform:** Windows desktop first  
**Last updated:** 2026-08-08  

---

## 1. Performance

- **NFR-PERF-001:** The shell shall become interactive within three seconds on the reference machine with an already indexed 10,000-item library.
- **NFR-PERF-002:** Local text search shall produce first results within 100 ms at the 95th percentile after index warm-up.
- **NFR-PERF-003:** Opening a details page shall not block on remote metadata.
- **NFR-PERF-004:** Playback startup for a local H.264/H.265 file shall normally begin within two seconds, excluding disk wake time.
- **NFR-PERF-005:** The UI thread shall not perform file hashing, media probing, database migration, remote requests, or directory enumeration.
- **NFR-PERF-006:** Initial scanning shall use bounded concurrency.
- **NFR-PERF-007:** Artwork decoding shall be size-aware and cached.
- **NFR-PERF-008:** Memory consumption must remain predictable; decoded full-resolution artwork shall not be retained unnecessarily.

Reference machine for benchmarks:

- Four modern CPU cores
- 16 GB RAM
- SSD system drive
- Integrated or mid-range discrete GPU
- Windows 10 or Windows 11
- 10,000 titles and 40,000 media files

## 2. Reliability

- **NFR-REL-001:** Playback progress shall use periodic durable checkpoints.
- **NFR-REL-002:** Database writes affecting multiple aggregates shall be transactional.
- **NFR-REL-003:** File-system events shall be treated as hints, not the only source of truth.
- **NFR-REL-004:** Reconciliation shall repair missed create, delete, rename, and change events.
- **NFR-REL-005:** An unavailable network share shall not remove titles automatically.
- **NFR-REL-006:** Schema migration shall create a backup before modification.
- **NFR-REL-007:** A failed migration shall preserve the previous database and start recovery mode.
- **NFR-REL-008:** Plugin failure shall be isolated and shall not crash the host where process isolation is configured.
- **NFR-REL-009:** The application shall tolerate abrupt termination without logical database corruption.

## 3. Scalability

The architecture shall support:

- 100,000 logical titles;
- 500,000 media files;
- multiple library roots;
- high-latency network shares;
- thousands of artwork records;
- long-lived watch history;
- stable synchronization identifiers and remote revisions.

SQLite is the local system of record for the single-user desktop client when indexes, write batching, and connection usage are designed correctly. Repository interfaces and synchronization contracts isolate it from the Supabase PostgreSQL backend.

## 4. Maintainability

- Nullable reference types enabled.
- Compiler warnings treated as errors in CI, with documented exceptions.
- Public and extension-facing APIs documented using XML documentation.
- Domain logic must not depend on WPF, Windows APIs, EF Core, VLC, or network clients.
- Infrastructure dependencies are introduced through interfaces.
- High-complexity classes must be decomposed before exceeding agreed thresholds.
- Generated code and third-party code must be isolated from authored domain code.
- Architectural boundaries are enforced using automated tests.

## 5. Observability

- Structured logs with event identifiers.
- Correlation IDs for scans, downloads, plugin calls, and playback sessions.
- User-facing diagnostics package generation.
- Metrics available locally for scanner throughput, failed matches, playback errors, and plugin latency.
- Sensitive paths may be redacted in exported diagnostics.
- No notes, credentials, provider secrets, or media content are logged.

## 6. Compatibility

- Windows 10 and Windows 11 are supported desktop platforms for version 1.0.
- Release CI and manual player validation shall include one currently supported build from each declared Windows generation.
- x64 is required initially; ARM64 may be added after dependency validation.
- Network paths support UNC and mapped drives.
- The player must support common containers and codecs through its embedded engine.
- Unsupported media shall fail gracefully and expose technical diagnostics.

## 7. Accessibility

- All primary actions reachable by keyboard.
- Visible focus indicators.
- Logical tab order.
- Screen-reader names for interactive elements.
- Captions and subtitles remain readable at high DPI.
- Text scales without clipping at supported system scaling values.
- Animation reduction respects OS preference and a MediaHub setting.
- Color is not the only indicator of progress, error, or watched state.

## 8. Privacy

- No account required.
- No telemetry enabled by default.
- Optional telemetry must be consent-based, documented, and revocable.
- Notes use compact variable-length plain-text storage. In Development/Test synchronization and the later public synchronization feature, RLS protects remote rows; no client-side note encryption or Markdown/HTML payload is required by the baseline.
- Public Windows 1.0 does not synchronize personal user state. Development/Test synchronization uses synthetic identities and synthetic or explicitly approved test data.
- Provider requests disclose only information required for the request.
- Privacy-sensitive data categories are documented in the security volume.

## 9. Usability

- First-run onboarding must be completable without technical terminology.
- Advanced settings are separated from common settings.
- Destructive actions require confirmation and explain consequences.
- Long-running tasks show status and can be cancelled where safe.
- Empty states explain how to proceed.
- User choices are remembered without creating hidden irreversible automation.

## 10. Supportability

A support bundle shall be generated without including media files. It may include:

- application version;
- OS version;
- anonymized dependency versions;
- sanitized configuration;
- recent structured logs;
- database integrity result;
- plugin inventory;
- scanner statistics;
- crash reference files.

The user must preview included categories before export.

## 11. Quality gates

A release candidate fails if any of the following is true:

- known data-loss defect;
- unmitigated critical security issue;
- database migration without tested rollback or recovery;
- primary keyboard navigation broken;
- playback progress regression;
- installer cannot upgrade the previous stable version;
- unsigned production binaries;
- plugin API changed without versioning or compatibility notes.


## 12. Backend and synchronization quality requirements

These quality requirements apply to Development/Test synchronization and the later public synchronization feature. They must not make public Windows 1.0 dependent on a user account.

- **NFR-CLOUD-001:** Local startup shall not block on DNS, authentication, Supabase health, or remote configuration.
- **NFR-CLOUD-002:** Cloud requests shall use explicit timeouts, cancellation, bounded retries, exponential backoff, and jitter.
- **NFR-CLOUD-003:** Remote mutations shall be idempotent and safe to retry after an unknown outcome.
- **NFR-CLOUD-004:** A complete Supabase outage shall not prevent playback, local scanning, editing, or browsing.
- **NFR-CLOUD-005:** The client shall tolerate clock skew by using server timestamps for remote ordering and monotonic local sequence numbers for local event order.
- **NFR-CLOUD-006:** Synchronization shall be resumable after process termination without duplicating user-visible records.
- **NFR-CLOUD-007:** Remote schema changes shall be backward-compatible for every supported desktop release or protected by protocol-version negotiation.
- **NFR-CLOUD-008:** Development, staging, and production datasets and secrets shall be isolated.
- **NFR-CLOUD-009:** P95 synchronization of a small user edit under normal connectivity should complete within five seconds without blocking the UI.
- **NFR-CLOUD-010:** Cloud-specific dependencies shall be confined to the Supabase infrastructure project and composition root.

---

## Document control

This document is part of the MediaHub revised baseline specification v1.2. Changes that affect architecture, security, data compatibility, plugin contracts, or user-visible behavior must be recorded in the Architecture Decision Record volume and reflected in the traceability matrix.
