# Volume 13 — Download Manager and Acquisition Workflows

**Project:** MediaHub  
**Document set:** MediaHub Project Bible  
**Status:** Revised baseline specification v1.2  
**Language:** English  
**Target platform:** Windows desktop first  
**Last updated:** 2026-08-08  

---

## 1. Scope

The download manager is provider-neutral infrastructure. Version 1.0 uses it for trusted updates, remote artwork, and other approved application assets. Online subtitle-provider downloads and legally permitted media acquisition workflows are post-1.0 capabilities. It is not a copyright-distribution service and does not bypass access controls.

## 2. Responsibilities

- queue jobs;
- pause/resume where supported;
- retry transient failures;
- display speed and progress;
- verify size and checksum;
- write through temporary files;
- sanitize final names;
- enforce extension allow-list;
- prevent execution;
- notify scanner when a final media file is committed.

## 3. Job model

Fields:

- job ID;
- category;
- source provider;
- display title;
- source URI or opaque provider token;
- destination root;
- temporary path;
- final path;
- expected size;
- checksum;
- state;
- bytes transferred;
- created/started/completed timestamps;
- retry count;
- failure reason;
- correlation ID;
- related media title/episode ID.

States:

```text
Queued
Resolving
WaitingForSpace
Downloading
Paused
Verifying
Committing
Completed
Failed
Cancelled
Quarantined
```

## 4. Download flow

The following generic flow is active only for categories enabled in the current release. In 1.0, media acquisition candidates and external-client handoff remain disabled.

1. Provider returns an authorized candidate.
2. User selects Download or an opt-in automation selects it.
3. Resolver returns a time-limited direct source or external-client handoff.
4. Host validates scheme, domain, size, filename, and permissions.
5. Space check runs.
6. Data writes to `.part` in the target file system.
7. Resume metadata is stored.
8. Checksum and media-type validation run.
9. File is atomically renamed where possible.
10. Scanner receives a high-priority import event.
11. Notification reports completion.

## 5. Security allow-list

Media acquisition baseline allows media and subtitle extensions configured by policy. The manager rejects or quarantines:

```text
.exe .msi .bat .cmd .ps1 .scr .com .dll .js .vbs .lnk
```

MIME type, extension, magic bytes, and probe result are compared. Mismatch is treated as suspicious.

## 6. Temporary files

- unique job directory;
- no execution permission where supported;
- restrictive ACL;
- cleaned after cancellation according to resume policy;
- bounded age cleanup;
- never indexed by scanner until committed;
- partial suffix excluded globally.

## 7. Resume

HTTP resume requires:

- range support;
- matching entity tag or last-modified evidence;
- unchanged expected source;
- valid local partial metadata.

If validation fails, restart rather than append corrupt content.

## 8. Parallelism

Global and per-provider limits:

- simultaneous jobs;
- bandwidth cap;
- active connection count;
- playback-aware throttling;
- network-share write limits.

Default should favor playback quality over download speed.

## 9. Future external client integration

MediaHub may hand off an authorized URI, magnet, or package to an explicitly configured external client. The integration:

- never stores unrelated client credentials without consent;
- uses official local API when available;
- displays the target client;
- tracks only jobs it created;
- imports completed files from configured destination;
- does not circumvent provider restrictions.

## 10. Provider search presentation

This presentation belongs to the post-1.0 authorized acquisition milestone.

External search results must display:

- provider;
- title;
- media type;
- season/episode;
- quality;
- language;
- size;
- source type;
- availability/health indicators;
- terms or attribution;
- whether acquisition is direct or external handoff.

Local and provider results are visually separated.

## 11. Automatic new-episode acquisition

Future opt-in automation:

- disabled by default;
- configured per series;
- requires an enabled authorized provider;
- uses quality and language rules;
- has maximum size and destination;
- generates a notification before or after acquisition according to user choice;
- prevents duplicate jobs;
- never selects a low-confidence episode match;
- can be paused globally.

## 12. Selection policy

A deterministic policy may rank candidates by:

- exact episode identity;
- preferred quality;
- supported codec;
- file size boundaries;
- language metadata;
- provider trust;
- checksum availability;
- duplicate avoidance.

AI is not required for acquisition selection. The user can preview and override.

## 13. Failure handling

- expired link: re-resolve once;
- network error: retry with backoff;
- checksum failure: quarantine and do not import;
- disk full: pause and notify;
- permission denied: stop before repeated writes;
- unsupported content: quarantine;
- provider revoked: disable future resolutions;
- destination unavailable: keep queued without losing job.

## 14. Deletion and cleanup

Cancelling offers:

- keep partial for resume;
- delete partial;
- cancel only.

Completed downloads are never deleted by queue cleanup. History can be cleared without deleting files.

## 15. Legal and policy boundary

The host architecture must allow providers to document authorization and terms. The application does not claim that public accessibility equals permission to download. Provider authors and users remain responsible for lawful use. MediaHub must not include built-in behavior to bypass authentication, CAPTCHA, DRM, rate limits, or technical protection.

## 16. Acceptance criteria

- Partial files are not imported.
- Executable payloads are rejected.
- Disk-full condition does not corrupt existing files.
- Completed media triggers scanner import.
- Duplicate automatic jobs are prevented.
- Provider failure leaves local playback operational.
- Automatic acquisition cannot activate without explicit opt-in.
- Version 1.0 cannot create media-acquisition or online-subtitle jobs through public UI or plugin installation.

---

## Document control

This document is part of the MediaHub revised baseline specification v1.2. Changes that affect architecture, security, data compatibility, plugin contracts, or user-visible behavior must be recorded in the Architecture Decision Record volume and reflected in the traceability matrix.
