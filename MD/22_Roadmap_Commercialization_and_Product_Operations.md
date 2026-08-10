# Volume 22 — Roadmap, Commercialization and Product Operations

**Project:** MediaHub  
**Document set:** MediaHub Project Bible  
**Status:** Revised baseline specification v1.2  
**Language:** English  
**Target platform:** Windows desktop first  
**Last updated:** 2026-08-08  

---

## 1. Long-term product direction

MediaHub is designed to grow from a Windows local-media application into a broader media ecosystem:

```text
Windows client
→ mature plugin/provider SDK
→ optional mobile client
→ optional web/remote capabilities
→ optional synchronization
→ commercial distribution and support
```

Expansion is conditional on a stable local core.

## 2. Milestone roadmap

### Phase 0 — Technical prototypes

- compare WPF and WinUI 3;
- compare LibVLCSharp and MPV;
- validate hardware acceleration;
- validate SMB playback;
- validate Recycle Bin API;
- validate FFprobe packaging;
- prototype cinematic card rows;
- confirm SQLite scale;
- produce ADRs.

Exit: architecture choices supported by measurements.

### Phase 1 — Foundation

- solution structure;
- dependency injection;
- domain model;
- database/migrations;
- settings/logging;
- basic shell;
- library roots;
- scanner skeleton;
- plugin manifest and host foundation.

Exit: app indexes a small library and survives restart.

### Phase 2 — Local library MVP

- movie and episode detection;
- details pages;
- local search;
- basic artwork generation;
- missing media;
- review queue;
- collections and playlists baseline.

Exit: folder browsing is no longer required for normal use.

### Phase 3 — Player MVP

- playback;
- standard controls;
- fullscreen;
- audio/subtitle tracks;
- resume;
- completion;
- next episode;
- progress persistence.

Exit: daily local viewing is reliable.

### Phase 4 — Product-quality v1

- network shares;
- real-time scan and reconciliation;
- ratings and notes;
- statistics;
- automatic collections;
- approved remote metadata, Hungarian-first artwork/descriptions, and daily new-episode monitoring;
- Hungarian UI on localization-ready resources;
- theme engine;
- Picture-in-Picture;
- screenshot gallery with scene jump;
- update notifications;
- diagnostics/recovery;
- plugin SDK baseline;
- installer and signing.

Exit: stable Windows 1.0.

The Phase 4 exit artifact is a downloadable, signed, installable Windows 10/11 x64 application, not an internal demo.

### Phase 5 — Enrichment

- subtitle providers;
- deeper metadata/provider coverage beyond the approved 1.0 implementation;
- richer editions;
- performance tuning.

### Phase 6 — Recommendations

- local genre and quality profile;
- similar title recommendations;
- deterministic same-director suggestions without learned director affinity;
- explanations and feedback;
- optional local semantic model.

### Phase 7 — Authorized acquisition ecosystem

- provider-neutral search;
- download manager;
- external-client handoff;
- opt-in per-series automation;
- plugin review and permission hardening.

### Phase 8 — Multi-device future

- public user-facing synchronization and account UI built on the already tested Development/Test protocol;
- mobile client;
- remote control;
- optional account;
- optional web capability.

## 3. Commercial models

Possible models, to be validated later:

- free open-source core with paid convenience services;
- one-time desktop license;
- free personal edition and paid Pro;
- subscription only for optional sync/cloud;
- commercial plugin marketplace revenue share;
- OEM/home-theater licensing.

The product should avoid making local playback dependent on a subscription.

## 4. Edition strategy example

### Community/Personal

- local playback;
- library;
- progress;
- ratings;
- notes;
- collections;
- themes;
- plugins.

### Pro, potential

- advanced multi-device sync;
- premium support;
- managed backups;
- advanced automation;
- business deployment tools.

This is exploratory and not a committed pricing plan.

## 5. Open-source readiness

If open-sourced:

- clear license;
- third-party notices;
- contribution guide;
- governance;
- code of conduct;
- trademark policy;
- plugin naming policy;
- security disclosure process.

The MediaHub name and visual identity may be protected separately from code license.

## 6. Product analytics

Because privacy is central, product analytics should be opt-in and content-free.

Useful operational metrics:

- startup duration;
- scanner duration;
- crash category;
- feature use counts without title identity;
- plugin failure rates;
- update success;
- device capability categories.

Never collect watched titles, filenames, notes, screenshots, or provider credentials for general analytics.

## 7. Support model

Before commercial launch:

- supported OS/version matrix;
- support channels;
- diagnostic bundle process;
- response targets by tier;
- known-issues database;
- migration support;
- data recovery guide;
- security advisory process.

## 8. Legal and compliance planning

Commercial readiness requires review of:

- software licenses;
- codec and media engine distribution;
- metadata/artwork provider terms;
- privacy policy;
- telemetry consent;
- consumer law;
- accessibility obligations;
- trademark;
- plugin marketplace terms;
- update disclosures;
- export and sanctions considerations where relevant.

This documentation does not replace jurisdiction-specific legal advice.

## 9. Provider governance

Built-in providers require:

- documented authorization;
- terms review;
- rate-limit compliance;
- attribution;
- privacy inventory;
- security review;
- maintenance owner;
- disable/revoke plan.

Marketplace providers must clearly state what they access and where data is sent.

## 10. Brand direction

MediaHub branding should communicate:

- ownership;
- cinematic quality;
- organization;
- privacy;
- extensibility.

Avoid visual confusion with established streaming brands. The Netflix-inspired interaction goal refers to cinematic ease, not imitation.

## 11. Product operations

Release operations include:

- roadmap;
- triage;
- security queue;
- dependency updates;
- plugin compatibility;
- translation updates;
- beta community;
- crash analysis;
- performance baselines;
- documentation maintenance.

## 12. Key risks

| Risk | Mitigation |
|---|---|
| Scope too broad | milestone gates and strict v1 scope |
| Codec/player instability | proven engine and adapter |
| Scanner false matches | confidence and review queue |
| NAS unreliability | reachability model and reconciliation |
| Plugin security | capability system and isolation |
| Commercial legal complexity | provider governance and legal review |
| Privacy concerns | offline-first and opt-in telemetry |
| UI performance | read models, virtualization, artwork cache |
| Database growth | indexes, maintenance, archival |
| Mobile expansion too early | stable desktop protocol first |

## 13. Go/no-go for commercial launch

Go requires:

- stable signed installer;
- recovery-tested migrations;
- privacy policy;
- security review;
- dependency license audit;
- crash-free target;
- support process;
- accessible primary flows;
- reliable update service;
- clear provider terms;
- backup/export;
- no known data-loss issue.

## 14. Product north star

A user should be able to trust MediaHub as the durable memory of their media life: what they own locally, what they watched, where they stopped, what they loved, and how they organized it—without giving up control of their data.


## 15. Revised backend roadmap

### Development baseline

- local SQLite persistence;
- Supabase CLI project and migrations;
- hosted Development project;
- automatically provisioned synthetic cloud identity adapter;
- synchronized ratings, notes, collections, playlists, and selected preferences;
- release catalog and feature flags;
- synchronization diagnostics.

The public 1.0 client keeps personal synchronization disabled and exposes no sign-in. It may use approved non-personal metadata, episode-schedule, compatibility, and release services.

### Staging readiness

- isolated Staging project;
- policy test suite;
- migration promotion process;
- sanitized operational dashboards;
- Edge Function deployment pipeline;
- protocol compatibility checks.

### Commercial production decision

Before public commercial release, evaluate:

- Supabase operating cost and quotas at expected scale;
- data residency and contractual requirements;
- support and incident-response needs;
- whether a MediaHub ASP.NET Core API should mediate all client traffic;
- whether self-hosted or managed PostgreSQL is preferable;
- portability of migrations and synchronized data.

Because the client uses backend-neutral interfaces, this decision does not block desktop development.

---

## Document control

This document is part of the MediaHub revised baseline specification v1.2. Changes that affect architecture, security, data compatibility, plugin contracts, or user-visible behavior must be recorded in the Architecture Decision Record volume and reflected in the traceability matrix.
