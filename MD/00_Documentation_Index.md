# MediaHub Documentation Index

**Project:** MediaHub  
**Document set:** MediaHub Project Bible  
**Status:** Revised baseline specification v1.2  
**Language:** English  
**Target platform:** Windows desktop first  
**Last updated:** 2026-08-08  

---

## Purpose

This documentation set defines MediaHub as a production-grade, Windows-first media platform rather than a simple video player. It is intended to be detailed enough for architecture planning, implementation, code review, testing, commercial evaluation, and later onboarding of additional developers.

The baseline product decisions are:

- The product name is **MediaHub**.
- The first client is a native Windows desktop application supporting Windows 10 and Windows 11 on x64.
- The interface follows a cinematic, Netflix-inspired browsing model while remaining an original design.
- The application is local-first: playback and the local library work without an account or backend. Development and test builds use an automatically provisioned synthetic Supabase identity to exercise synchronization and backend contracts. Public 1.0 builds do not expose user-data synchronization or require a cloud identity.
- Only one local user profile is required in the Windows client. Optional cloud synchronization uses one account identity and does not introduce household profile switching.
- Local folders and SMB/NAS network shares are supported.
- Media scanning is real-time with a reconciliation scan for reliability.
- The player uses standard desktop controls: Space for play/pause, Left/Right for ten-second seek, mouse wheel for volume, and double-click for fullscreen.
- Remote metadata, Hungarian-first artwork, descriptions, and episode-airing information are part of the installable 1.0 product, with local metadata and generated artwork as offline fallbacks.
- Missing artwork is represented by a generated frame capture.
- Ratings use a five-star scale and each title may have an optional private note.
- Deleted media is moved to the Windows Recycle Bin while the logical library record, rating, note, history, collection membership, and playlist membership remain.
- Collections may be automatic or user-created; an item can belong to multiple collections.
- Favorites are a first-class state independent from the five-star rating.
- User-created playlists are supported.
- The 1.0 user interface is Hungarian. Localization architecture and English fallback resources remain in place for later language packs. The interface is fully themeable.
- The plugin foundation and approved built-in extensions are included in 1.0. Public installation of third-party `.mhpkg` packages is deferred.
- New episode detection combines official air-date information with provider release state, refreshes at startup and once daily, and keeps local-file availability as a separate state. Future authorized providers may optionally trigger automatic acquisition.
- The recommendation engine is post-1.0. When introduced, its learned profile is limited to genre and preferred quality; ratings and watch behavior are weighting signals. Similar-title and same-director suggestions may remain deterministic metadata rules rather than learned affinities.
- Picture-in-Picture and a manual screenshot gallery with “Jump to scene” are included in 1.0.
- Online subtitle-provider search and download are post-1.0; embedded, sidecar, and manually imported subtitles remain supported in 1.0.
- Updates are optional. Older versions remain usable, while unavailable newer features are clearly marked and release notes are shown.
- Long-term direction includes a mobile client, web capabilities, extensibility, AI features, and possible commercial distribution.

## Volume map

| Volume | File | Primary responsibility |
|---:|---|---|
| 01 | `01_Product_Vision_and_Scope.md` | Vision, audience, scope, business rules |
| 02 | `02_Product_Requirements_and_User_Flows.md` | Functional requirements and core journeys |
| 03 | `03_Nonfunctional_Requirements.md` | Performance, reliability, supportability |
| 04 | `04_System_Architecture.md` | Clean Architecture, modules, boundaries |
| 05 | `05_Domain_Model_and_Database.md` | Entities, relationships, persistence |
| 06 | `06_Desktop_UI_UX_Specification.md` | Screens, navigation, interaction |
| 07 | `07_Player_Engine.md` | Playback engine and cinematic controls |
| 08 | `08_Library_Scanner_and_Media_Identification.md` | Folder monitoring, parsing, matching |
| 09 | `09_Metadata_Artwork_and_Subtitles.md` | Metadata, artwork, subtitle workflows |
| 10 | `10_Watch_Tracking_Collections_Playlists_Statistics.md` | Progress and organization features |
| 11 | `11_Network_Shares_and_Storage.md` | SMB/NAS, offline paths, storage safety |
| 12 | `12_Plugin_Provider_and_Extension_SDK.md` | Extension contracts and isolation |
| 13 | `13_Download_Manager_and_Acquisition_Workflows.md` | Download queue and provider-neutral flows |
| 14 | `14_AI_Recommendation_Engine.md` | Explainable local recommendation design |
| 15 | `15_Security_Privacy_and_Threat_Model.md` | Security controls and abuse cases |
| 16 | `16_Localization_Themes_and_Accessibility.md` | i18n, themes, accessibility |
| 17 | `17_Application_Settings_Logging_and_Diagnostics.md` | Configuration, logs, diagnostics |
| 18 | `18_Update_Deployment_and_Release_Management.md` | Packaging, versions, optional updates |
| 19 | `19_Testing_QA_and_Acceptance.md` | Test strategy and release gates |
| 20 | `20_Developer_Standards_and_Repository_Workflow.md` | Coding standards and Git workflow |
| 21 | `21_API_Contracts_Events_and_Sequence_Flows.md` | Interfaces, events, sequences |
| 22 | `22_Roadmap_Commercialization_and_Product_Operations.md` | Releases and commercial readiness |
| 23 | `23_Architecture_Decision_Records.md` | Accepted architectural decisions |
| 24 | `24_Glossary_and_Traceability_Matrix.md` | Terminology and requirement mapping |
| 25 | `25_UI_Component_Catalog.md` | Reusable cinematic UI components |
| 26 | `26_Database_Physical_Schema_and_Migrations.md` | Physical database schema and migrations |
| 27 | `27_Background_Jobs_Caching_and_Performance.md` | Scheduling, caching, and performance budgets |
| 28 | `28_Error_Catalog_and_Recovery_Runbooks.md` | Error codes and recovery procedures |
| 29 | `29_Implementation_Backlog_and_Epics.md` | Implementation epics and release slicing |
| 30 | `30_File_Naming_and_Parsing_Rulebook.md` | Filename parsing and ambiguity rules |
| 31 | `31_Configuration_Schema_and_Defaults.md` | Versioned settings schema and defaults |
| 32 | `32_Plugin_SDK_Examples.md` | Concrete extension SDK examples |
| 33 | `33_Detailed_Test_Case_Catalog.md` | End-to-end and regression test cases |
| 34 | `34_User_Data_Portability_and_Disaster_Recovery.md` | Backup, migration, import/export, recovery |
| 35 | `35_Development_Backend_Supabase_and_Synchronization.md` | Local-first backend architecture, Supabase environments, synchronization |
| 36 | `36_Supabase_PostgreSQL_RLS_and_Edge_Functions.md` | Remote schema, RLS policies, Edge Functions, cloud migrations |

## Requirement identifiers

Requirements use stable identifiers:

- `PRD-*`: product requirements
- `FR-*`: functional requirements
- `NFR-*`: non-functional requirements
- `SEC-*`: security requirements
- `DATA-*`: persistence and data integrity requirements
- `UX-*`: user-experience requirements
- `SDK-*`: extension requirements
- `AI-*`: recommendation requirements
- `REL-*`: deployment and release requirements
- `TST-*`: verification requirements
- `SYNC-*`: local-to-cloud synchronization requirements
- `CLOUD-*`: hosted backend and environment requirements

Identifiers must not be reused after deletion. Deprecated requirements remain in history with a reason.

## Reading order

New developers should read Volumes 01–05 and Volumes 35–36 first, then the volume corresponding to their implementation area. Test engineers should additionally read Volumes 19 and 24. Plugin authors should read Volumes 12, 15, 17, and 21. Product and commercial stakeholders should read Volumes 01, 02, 18, 22, and 35.

## Revision 1.1 architectural change

The original baseline treated cloud synchronization as a future-only capability. Revision 1.1 establishes Supabase as the active development and testing backend while preserving SQLite as the local system of record for device-specific and offline-critical data. The client communicates with Supabase only through backend-neutral application interfaces implemented by `MediaHub.Infrastructure.Supabase`. No Supabase SDK types may cross into Domain or Application projects.

## Revision 1.2 product-scope lock

Revision 1.2 defines Windows 1.0 as a downloadable, installable, reduced-scope production application rather than a technical preview. Version 1.0 includes automatic collections, approved remote metadata and artwork, new-episode monitoring, Picture-in-Picture, and the screenshot gallery. AI recommendations, online subtitle providers, public cross-device synchronization, authorized acquisition, and public third-party plugin installation remain post-1.0 capabilities. Development and test builds may continue validating those future backend contracts behind non-production feature gates.

## Definition of complete

A feature is complete only when:

1. Its requirement is documented.
2. Its domain ownership is clear.
3. Failure behavior is specified.
4. Security and privacy implications are reviewed.
5. Automated tests cover the expected and failure paths.
6. Localization resources exist.
7. Accessibility behavior is validated.
8. Logging is sufficient for diagnosis without leaking sensitive data.
9. Database or settings migrations are reversible or recoverable.
10. User-facing documentation and release notes are updated.

---

## Document control

This document is part of the MediaHub revised baseline specification v1.2. Changes that affect architecture, security, data compatibility, plugin contracts, or user-visible behavior must be recorded in the Architecture Decision Record volume and reflected in the traceability matrix.
