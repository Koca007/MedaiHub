# Volume 28 — Error Catalog and Recovery Runbooks

**Project:** MediaHub  
**Document set:** MediaHub Project Bible  
**Status:** Revised baseline specification v1.2  
**Language:** English  
**Target platform:** Windows desktop first  
**Last updated:** 2026-08-08  

---

## 1. Error code format

```text
MH-<AREA>-<NUMBER>
```

Areas:

- APP
- DB
- LIB
- NET
- SCAN
- PLAY
- META
- SUB
- DL
- PLUG
- UPD
- SEC
- SYNC

## 2. Core errors

| Code | Meaning | Retryable |
|---|---|---|
| MH-APP-001 | Unexpected application failure | depends |
| MH-DB-001 | Database cannot be opened | no until corrected |
| MH-DB-002 | Integrity check failed | no; recovery |
| MH-DB-003 | Migration failed | no; recovery |
| MH-LIB-001 | Library root not reachable | yes |
| MH-LIB-002 | Permission denied | no until changed |
| MH-SCAN-001 | File remained unstable | yes |
| MH-SCAN-002 | Media probe failed | maybe |
| MH-SCAN-003 | Ambiguous match | user action |
| MH-PLAY-001 | Player failed to open file | maybe |
| MH-PLAY-002 | Decoder failure | fallback |
| MH-NET-001 | Network share offline | yes |
| MH-META-001 | Metadata provider unavailable | yes |
| MH-SUB-001 | Subtitle payload invalid | no |
| MH-DL-001 | Insufficient disk space | user action |
| MH-DL-002 | Checksum mismatch | re-download |
| MH-PLUG-001 | Plugin incompatible | no |
| MH-PLUG-002 | Plugin circuit open | later/manual |
| MH-UPD-001 | Update signature invalid | no |
| MH-SEC-001 | Unsafe path rejected | no |

## 3. User message principles

Do not expose stack traces. Example:

**Bad:** `IOException 0x80070035`

**Good:** “The network library is currently unavailable. Your titles and history are still safe. MediaHub will retry when the share reconnects.”

Details panel may include sanitized technical code and correlation ID.

## 4. Runbook: database open failure

1. Stop background services.
2. Confirm path and permissions.
3. Check free disk.
4. Attempt read-only open.
5. Locate latest verified backup.
6. Offer:
   - Retry
   - Open recovery mode
   - Restore backup
   - Export diagnostics
7. Never create an empty database over an existing unreadable file.
8. Preserve failed database copy for investigation.

## 5. Runbook: integrity failure

1. Mark application recovery state.
2. Disable writes.
3. Generate integrity report.
4. Copy database.
5. Attempt supported SQLite recovery on copy only.
6. Compare recovered critical tables.
7. Ask user before replacing.
8. Retain original and backup.

## 6. Runbook: failed migration

1. Record migration stage and error.
2. Roll back transaction.
3. Verify pre-migration backup.
4. Restore previous application/database pair where possible.
5. Start recovery mode.
6. Export diagnostics.
7. Prevent repeated automatic migration loop.
8. Show clear release-specific guidance.

## 7. Runbook: network share outage

1. Mark root Offline.
2. Mark child media TemporarilyUnavailable.
3. Stop destructive reconciliation.
4. Back off connectivity checks.
5. Keep cached metadata.
6. Notify once, not repeatedly.
7. On reconnect, authenticate and run targeted reconciliation.
8. Clear banner after successful verification.

## 8. Runbook: scanner backlog

1. Inspect queue size and event duplication.
2. Coalesce paths.
3. Pause low-priority metadata/artwork.
4. Reduce producer rate.
5. Persist scan checkpoint.
6. Show progress and allow pause.
7. Trigger reconciliation after backlog drains.

## 9. Runbook: player cannot open

1. Verify path and access.
2. Verify media file unchanged.
3. Capture engine error.
4. Retry once if transient.
5. Offer hardware acceleration fallback.
6. Offer alternate edition/file.
7. Offer diagnostics.
8. Do not mark watched or advance next episode.

## 10. Runbook: download checksum mismatch

1. Move partial/final payload to quarantine.
2. Do not index.
3. Record expected/actual hash.
4. Redact source token.
5. Offer re-download.
6. If repeated, disable provider candidate and warn.
7. Never allow “play anyway” for executable/mismatched type.

## 11. Runbook: plugin crash loop

1. Increment failure count.
2. Open circuit.
3. Stop plugin process.
4. Mark degraded/disabled.
5. Preserve host function.
6. Show plugin-specific notification.
7. Offer restart once, disable, or view details.
8. On next app start, do not auto-enable after threshold.

## 12. Runbook: update signature failure

1. Delete staged package or quarantine.
2. Record security event.
3. Do not install.
4. Disable automatic retry for the same manifest.
5. Re-fetch manifest from trusted endpoint later.
6. Notify user that the current version remains safe to use locally.
7. Escalate in product operations.

## 13. Runbook: low disk space

Priority cleanup suggestions:

1. temporary failed downloads;
2. expired provider cache;
3. derived artwork sizes;
4. old logs;
5. old verified backups beyond minimum;
6. user-approved partial downloads.

Never automatically delete media, database, notes, screenshots, or the only backup.

## 14. Recovery-mode UI

Capabilities:

- inspect backups;
- integrity check;
- restore;
- export user data if readable;
- disable plugins;
- reset theme;
- clear safe caches;
- open logs;
- launch with scanning disabled.

## 15. Correlation

Every failure shown to user includes optional reference:

```text
Reference: 7F3A-20260803
```

Reference maps to local logs without exposing sensitive data.

## 16. Acceptance criteria

- Recovery never overwrites unreadable database without preserving it.
- Network outage does not trigger deletion.
- Player failure does not mark watched.
- Invalid update cannot execute.
- Low-space cleanup preserves user-owned data.
- Plugin crash loop is contained.


## 17. Cloud and synchronization error catalog

| Code | Meaning | Default recovery |
|---|---|---|
| `MH-SYNC-001` | No valid cloud identity | Development/Test or future sync: reprovision/offer sign-in; continue locally |
| `MH-SYNC-002` | Session refresh failed | Protect local data; retry with backoff |
| `MH-SYNC-003` | Supabase unreachable | Queue enabled test/future-sync changes and continue locally |
| `MH-SYNC-004` | Remote protocol outside supported range | Disable affected cloud features only |
| `MH-SYNC-005` | Policy rejected operation | Stop retry loop; show diagnostic action |
| `MH-SYNC-006` | Concurrent note edits | Preserve both and request resolution |
| `MH-SYNC-007` | Remote or local DTO failed validation | Quarantine operation and log sanitized detail |
| `MH-SYNC-008` | Key reused with different payload | Dead-letter and require repair |
| `MH-SYNC-009` | Delta cursor unavailable or expired | Run bounded full reconciliation |
| `MH-SYNC-010` | Trusted Edge Function returned failure | Retry only when error class is retryable |

### Runbook: backend outage

In public Windows 1.0 this affects only enabled non-personal remote services. Personal outbox handling applies to Development/Test builds and the later public synchronization feature.

1. Mark connectivity degraded.
2. Keep local commands enabled.
3. Commit all edits with outbox records.
4. Back off background retries.
5. Display pending count without alarming playback users.
6. On recovery, authenticate, push idempotent batches, pull deltas, and verify cursors.

### Runbook: suspected RLS regression

1. Stop automatic retry for denied operations.
2. Preserve the outbox payload locally.
3. Capture request correlation ID and policy-safe metadata.
4. Verify environment and identity.
5. Run policy integration tests against the affected migration.
6. Repair backend policy, then manually retry quarantined operations.

---

## Document control

This document is part of the MediaHub revised baseline specification v1.2. Changes that affect architecture, security, data compatibility, plugin contracts, or user-visible behavior must be recorded in the Architecture Decision Record volume and reflected in the traceability matrix.
