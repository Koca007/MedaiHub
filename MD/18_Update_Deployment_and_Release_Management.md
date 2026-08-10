# Volume 18 — Update, Deployment and Release Management

**Project:** MediaHub  
**Document set:** MediaHub Project Bible  
**Status:** Revised baseline specification v1.2  
**Language:** English  
**Target platform:** Windows desktop first  
**Last updated:** 2026-08-08  

---

## 1. Release principles

- updates are optional;
- older versions retain local functionality;
- new-version-only features are clearly marked;
- migrations are backed up and recoverable;
- packages are signed;
- release notes are detailed and readable;
- stable, beta, and developer channels remain separate.

## 2. Packaging options

Version 1.0 must ship as a downloadable, signed, user-installable package. The final packaging technology is selected through an ADR after validating:

- code signing;
- desktop shortcuts;
- file associations;
- protocol handlers;
- upgrade behavior;
- rollback/recovery;
- enterprise deployment;
- dependency packaging;
- uninstall behavior;
- Windows reputation.

Possible formats include MSIX and a signed traditional installer. Portable mode is deferred.

## 3. Installation scope

Baseline:

- per-user installation preferred;
- no administrator rights unless required;
- application data under user profile;
- bundled dependencies verified;
- uninstall does not delete library database without explicit user choice.
- installer and upgrade behavior are validated on Windows 10 and Windows 11 x64.

## 4. File associations

Optional associations:

- common video extensions;
- `.mhpkg` plugin package only after public external-plugin installation ships;
- future playlist/export formats.

The installer must not take over media associations without explicit opt-in.

## 5. Update check

The client requests a signed manifest containing:

- channel;
- latest version;
- minimum supported host for online services, if any;
- release date;
- package URI;
- size;
- hashes;
- signature;
- release notes URI/content;
- compatibility notes;
- required disk space;
- database migration level.

Update checking can be disabled.

## 6. User experience

When an update exists:

```text
MediaHub 1.6 is available
- New AI recommendation controls
- Improved network-share recovery
- Scanner performance fixes

[View details] [Download] [Remind me later]
```

No blocking modal is shown for ordinary updates.

## 7. Older version behavior

- local library and playback remain available;
- features not present in the build are not shown as errors;
- if a remote plugin/provider requires a newer SDK, show incompatible status;
- release notes explain unavailable features;
- a compatibility service may warn, but it must not disable unrelated local features.

## 8. Update download

Uses the download manager with:

- signed manifest verification before download;
- package hash;
- signature verification;
- resumable transfer;
- disk-space check;
- staging directory;
- explicit install action unless auto-install is later enabled;
- no plugin ability to substitute package.

## 9. Installation and migration

1. Close or coordinate running instance.
2. Persist playback state.
3. Verify package again.
4. Create database backup if schema changes.
5. Install binaries.
6. Start migration phase.
7. Validate schema and search index.
8. Mark update successful.
9. Retain recovery metadata.
10. Show release notes.

## 10. Rollback

Binary rollback may be supported when schema remains compatible. If not:

- restore pre-migration database backup;
- restore previous binaries through installer recovery;
- preserve newly downloaded media and user files;
- clearly explain any settings introduced by the newer version.

## 11. Release channels

### Stable

- signed production build;
- full QA;
- supported migration paths;
- no experimental feature enabled by default.

### Beta

- opt-in;
- more frequent;
- diagnostic logging may be increased with consent;
- rollback instructions;
- not used for commercial production support guarantees.

### Developer

- internal or contributor builds;
- may use separate data directory;
- must not silently open production database with incompatible schema.

## 12. Semantic versioning

Application version:

```text
MAJOR.MINOR.PATCH
```

- Major: compatibility or product-level breaking change.
- Minor: backward-compatible features.
- Patch: fixes.

Database schema and plugin SDK have separate version identifiers.

## 13. Changelog

Every release lists:

- Added
- Changed
- Fixed
- Security
- Deprecated
- Removed
- Migration notes
- Known issues

User-facing wording avoids internal ticket-only descriptions.

## 14. Feature flags

Feature flags support staged delivery but must not become permanent hidden complexity.

Each flag has:

- owner;
- default;
- expiry date;
- affected versions;
- migration impact;
- telemetry requirement, if any.

Security fixes are not gated behind user experiments.

## 15. Code signing and provenance

Production artifacts require:

- protected signing keys;
- reproducible build metadata where practical;
- dependency manifest;
- software bill of materials;
- checksum publication;
- build pipeline attestation for commercial readiness.

## 16. Acceptance criteria

- User can decline an update and keep using local features.
- Update package is rejected when signature/hash fails.
- Migration creates backup.
- Release notes identify new features.
- Plugin incompatibility is explained.
- Uninstall does not silently delete user database.
- Stable and beta data directories are protected from incompatible cross-use.


## 17. Backend environments

MediaHub uses four backend modes:

1. **Local:** Supabase CLI and Docker-backed local services for development and automated integration tests.
2. **Development:** hosted Supabase project for shared developer testing.
3. **Staging:** isolated hosted project matching release-candidate schema and policies.
4. **Production:** later commercial environment; may remain Supabase or be replaced behind the same client contracts.

Desktop artifacts are built with an environment identifier but never with privileged credentials. Production builds refuse development/staging endpoints unless an explicitly signed internal build allows them.

Public Windows 1.0 does not expose personal synchronization or sign-in. It may access approved public or rate-limited metadata, episode-schedule, feature-compatibility, and release endpoints using only a publishable client configuration. Development/Test builds may enable the full synthetic-identity synchronization stack.

## 18. Cloud migration release order

For backward-compatible releases:

1. deploy additive remote schema and policies;
2. deploy compatible Edge Functions;
3. run cloud integration tests;
4. publish remote protocol compatibility metadata;
5. release desktop client;
6. observe adoption and errors;
7. remove deprecated fields only after every supported client is outside the compatibility window.

A failed cloud deployment must be reversible without forcing a desktop update. Local features continue even when a cloud feature is disabled.

---

## Document control

This document is part of the MediaHub revised baseline specification v1.2. Changes that affect architecture, security, data compatibility, plugin contracts, or user-visible behavior must be recorded in the Architecture Decision Record volume and reflected in the traceability matrix.
