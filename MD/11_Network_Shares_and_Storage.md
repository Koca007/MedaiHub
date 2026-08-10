# Volume 11 — Network Shares and Storage

**Project:** MediaHub  
**Document set:** MediaHub Project Bible  
**Status:** Revised baseline specification v1.2  
**Language:** English  
**Target platform:** Windows desktop first  
**Last updated:** 2026-08-08  

---

## 1. Supported storage types

- local fixed drives;
- removable drives;
- mapped network drives;
- UNC SMB paths;
- NAS shares accessible through Windows;
- read-only roots;
- future cloud-mounted file systems, treated as file systems rather than direct cloud APIs.

## 2. Network root onboarding

When adding a network root, MediaHub validates:

- path syntax;
- reachability;
- read permission;
- optional write permission;
- latency sample;
- file-system watcher reliability;
- Recycle Bin support assumptions;
- credential availability through Windows.

The application must not ask for credentials if Windows can already access the share.

## 3. Credential handling

Preferred order:

1. Windows-integrated session credentials.
2. Windows Credential Manager reference.
3. Explicit credential prompt stored only with user consent in OS-protected storage.
4. Session-only credentials.

Credentials are never stored in the MediaHub database as plain text, never logged, and never exposed to plugins.

## 4. Reachability state

Root states:

- Online
- Degraded
- AuthenticationRequired
- Offline
- Disabled
- Unknown

Media file states derive from both root state and path verification. If a root is offline, contained files are `TemporarilyUnavailable`, not `MissingVerified`.

## 5. Monitoring network shares

Some SMB environments provide incomplete watcher behavior. Therefore:

- watcher mode is attempted and health monitored;
- polling may be used;
- reconciliation frequency may differ from local roots;
- reconnect triggers a targeted scan;
- repeated failures use backoff;
- the UI shows the effective monitoring mode.

## 6. Playback over network

Before playback:

- verify root online;
- verify file accessible;
- optionally perform a small read test;
- do not copy entire file locally by default;
- pass network path to player;
- show buffering state;
- preserve progress through interruptions.

Optional read-ahead caching may be added later with strict storage limits.

## 7. Network interruption

During playback:

1. Player reports buffering or read failure.
2. MediaHub keeps session open for a configurable grace period.
3. Root reachability is checked.
4. User sees Reconnecting.
5. On recovery, playback resumes where the engine permits.
6. On failure, a durable checkpoint is saved and retry is offered.

## 8. Deletion semantics

Windows Recycle Bin behavior is reliable primarily for local file systems. Network shares may:

- delete permanently;
- provide server-side recycle features;
- deny deletion;
- behave according to NAS configuration.

Therefore:

- MediaHub determines capability where possible.
- If safe recycle cannot be guaranteed, the UI must not label the action “Move to Recycle Bin.”
- Baseline default for unsupported network recycle is to disable physical deletion and offer Remove from Library or Open Location.
- Permanent network deletion requires a future explicitly designed workflow and is not baseline behavior.

## 9. Path normalization

Store original display path and normalized comparison path.

Normalization handles:

- drive-letter casing;
- UNC server/share casing where appropriate;
- separators;
- trailing separators;
- extended-length paths;
- Unicode normalization;
- mapped drive to UNC identity where resolvable.

Never assume path casing rules across every provider.

## 10. Removable drives

Library root stores a volume identity when available. If drive letter changes:

- detect the same volume;
- offer automatic path remap;
- verify file fingerprints;
- preserve root ID and media IDs.

## 11. Storage maintenance

MediaHub distinguishes:

- media bytes;
- database;
- logs;
- artwork cache;
- provider cache;
- screenshots;
- subtitle cache;
- backups;
- temporary downloads.

Settings show usage per category and safe cleanup actions. Clearing cache must not remove user screenshots, notes, or database state.

## 12. Free-space checks

Before downloads, artwork bulk generation, backups, or migrations:

- inspect free space;
- estimate requirement;
- reserve safety margin;
- fail before partial destructive work;
- clean temporary files only according to policy.

## 13. Backup destination

Backups default to local application data, not the same network share as media. User may configure another path. Backup integrity is verified after writing.

## 14. Offline browsing

When network media is offline, cached metadata and artwork remain browsable. Play actions show the root state and reconnect guidance. Notes, ratings, collections, and statistics remain editable.

## 15. Acceptance criteria

- An offline NAS does not erase media records.
- Reconnecting triggers targeted reconciliation.
- Windows credentials are not duplicated unnecessarily.
- Mapped-drive letter changes can be recovered.
- Network delete does not falsely promise Recycle Bin behavior.
- Playback interruption saves progress.
- Offline browsing remains functional.

---

## Document control

This document is part of the MediaHub revised baseline specification v1.2. Changes that affect architecture, security, data compatibility, plugin contracts, or user-visible behavior must be recorded in the Architecture Decision Record volume and reflected in the traceability matrix.
