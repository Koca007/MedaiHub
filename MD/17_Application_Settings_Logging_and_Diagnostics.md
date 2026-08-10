# Volume 17 — Settings, Logging and Diagnostics

**Project:** MediaHub  
**Document set:** MediaHub Project Bible  
**Status:** Revised baseline specification v1.2  
**Language:** English  
**Target platform:** Windows desktop first  
**Last updated:** 2026-08-08  

---

## 1. Settings architecture

Settings are divided into:

- user preferences;
- machine settings;
- library-root settings;
- plugin settings;
- secure secrets;
- experimental flags;
- policy-controlled settings for future managed deployments.

Each setting has:

- stable key;
- data type;
- default;
- validation;
- scope;
- introduced version;
- migration behavior;
- restart requirement;
- sensitivity classification.

## 2. Settings categories

### General

- interface language when more than one complete language pack is installed; 1.0 is Hungarian-only;
- startup behavior;
- minimize to tray, optional;
- default landing page;
- notification preferences;
- update-check frequency.

### Appearance

- theme;
- accent;
- font scale;
- card density;
- motion;
- backdrop treatment.

### Player

- volume;
- seek interval;
- completion threshold;
- autoplay next episode;
- hardware acceleration;
- screenshot location;
- sleep prevention;
- subtitle styling.

### Library

- roots;
- exclusions;
- real-time monitoring;
- reconciliation frequency;
- missing-item visibility;
- probe concurrency;
- artwork generation.

### Metadata and subtitles

- provider order;
- metadata language order, fixed in 1.0 to Hungarian → original → English;
- local subtitle languages;
- post-1.0 automatic subtitle download settings, hidden in 1.0;
- cache limits.

### Episodes

- refresh after startup;
- 24-hour background refresh interval;
- in-app notification retention;
- optional Windows toast.

### Downloads

- destination;
- concurrency;
- bandwidth;
- playback throttling;
- temporary retention.

### Privacy

- telemetry consent;
- diagnostic redaction;
- history retention;
- recommendation reset.

### Plugins

- approved built-in extensions;
- declared capabilities;
- built-in update compatibility;
- health.

Public external package installation, permissions approval, and third-party update policy are hidden until the external-plugin milestone.

## 3. Configuration storage

Non-secret settings may use JSON plus database-backed structured settings where relational queries are beneficial. Writes are atomic:

1. serialize to temporary file;
2. flush;
3. validate;
4. replace;
5. keep last-known-good backup.

Secrets are stored separately through Windows-protected storage.

## 4. Validation

Invalid settings:

- are not applied;
- show field-specific guidance;
- preserve previous valid value;
- are logged without sensitive content;
- may trigger safe-mode defaults at startup.

## 5. Logging

Structured logging fields:

- timestamp UTC;
- level;
- event ID;
- category;
- message template;
- correlation ID;
- operation ID;
- plugin/provider ID where applicable;
- exception type;
- sanitized context.

Levels:

- Trace: development-only high volume.
- Debug: diagnostic details.
- Information: lifecycle and completed actions.
- Warning: recoverable unexpected condition.
- Error: failed operation.
- Critical: integrity or startup failure.

## 6. Event ID ranges

```text
1000–1999 Application lifecycle
2000–2999 Database
3000–3999 Scanner
4000–4999 Player
5000–5999 Metadata/subtitles
6000–6999 Downloads
7000–7999 Plugins/providers
8000–8999 Updates
9000–9999 Security/recovery
```

Event IDs are documented and stable.

## 7. Sensitive data redaction

Redact or hash:

- credentials;
- API tokens;
- private notes;
- full network usernames;
- query parameters containing secrets;
- optionally full paths in exported diagnostics.

Internal local logs may retain paths only according to privacy setting and security review. Provider response bodies are not logged by default.

## 8. Log retention

- rolling files;
- size and age limits;
- separate crash logs;
- automatic cleanup;
- user can open log folder;
- user can export a sanitized bundle;
- no unlimited debug logging in production.

## 9. Diagnostics dashboard

Shows:

- application version;
- runtime and OS;
- database status;
- library root health;
- scanner queue;
- player engine version;
- FFprobe version;
- plugin health;
- cache usage;
- last update check;
- recent errors;
- background jobs.

Actions:

- run database integrity check;
- run root reachability test;
- rebuild search index;
- rescan selected root;
- clear safe caches;
- generate support bundle;
- open recovery mode.

## 10. Support bundle

User selects categories before export. Bundle may include:

- sanitized logs;
- settings without secrets;
- dependency inventory;
- plugin manifests;
- integrity report;
- scanner statistics;
- crash dumps if explicitly included;
- correlation IDs.

It excludes:

- media files;
- screenshots by default;
- notes;
- credentials;
- provider tokens;
- full database unless explicitly chosen and warned.

## 11. Safe mode

Safe mode starts with:

- optional/nonessential built-in extensions disabled; future third-party extensions are also disabled when that feature exists;
- default theme;
- no background scanning until confirmed;
- local database read access;
- recovery tools;
- optional player fallback settings.

Safe mode can be triggered automatically after repeated startup failure or manually with a command-line switch.

## 12. Command-line options

Potential supported options:

```text
--safe-mode
--open <path>
--play <path>
--diagnostics
--reset-window
--disable-plugins
--log-level <level>
```

Arguments are validated and do not enable arbitrary plugin commands.

## 13. Crash handling

Unexpected UI or background exceptions:

- save durable progress where safe;
- write crash record;
- avoid recursive logging;
- show recovery guidance on next start;
- correlate crash with recent plugin or job;
- never claim data is safe without integrity verification after severe failure.

## 14. Acceptance criteria

- Invalid setting does not corrupt configuration.
- Secrets are absent from settings export.
- Support bundle is previewable.
- Safe mode disables optional extensions without preventing core local recovery.
- Logs contain stable event IDs.
- Debug logging rotates and cannot fill disk indefinitely.
- Database and root health are visible without technical commands.


## 15. Backend diagnostics

The full synchronization panel is visible in Development/Test builds. Public Windows 1.0 shows only non-personal remote-service health relevant to metadata, episode schedules, feature compatibility, and releases; it does not show sign-in or personal-sync controls.

The diagnostics page shall expose:

- configured environment: Local, Development, Staging, Production;
- backend provider name;
- authentication state without exposing tokens;
- last successful push and pull;
- pending, failed, and dead-letter outbox counts;
- remote protocol version;
- local schema version and remote migration version;
- Realtime connection state;
- last remote configuration refresh;
- sanitized error history;
- manual “Retry synchronization” and “Rebuild remote cache” actions.

Logs use correlation IDs that connect a local command, outbox message, remote request, and resulting delta. Authorization headers, access tokens, refresh tokens, publishable keys in full, and remote secret values are always redacted.

---

## Document control

This document is part of the MediaHub revised baseline specification v1.2. Changes that affect architecture, security, data compatibility, plugin contracts, or user-visible behavior must be recorded in the Architecture Decision Record volume and reflected in the traceability matrix.
