# Volume 31 — Configuration Schema and Defaults

**Project:** MediaHub  
**Document set:** MediaHub Project Bible  
**Status:** Revised baseline specification v1.2  
**Language:** English  
**Target platform:** Windows desktop first  
**Last updated:** 2026-08-08  

---

## 1. Configuration principles

- secure defaults;
- explicit types;
- versioned schema;
- separation of secrets;
- atomic writes;
- validation before activation;
- UI generated from domain settings metadata where practical.

## 2. Top-level shape

```json
{
  "schemaVersion": 1,
  "general": {},
  "appearance": {},
  "player": {},
  "library": {},
  "metadata": {},
  "episodes": {},
  "subtitles": {},
  "downloads": {},
  "plugins": {},
  "privacy": {},
  "updates": {},
  "diagnostics": {},
  "backend": {},
  "synchronization": {}
}
```

## 3. General defaults

```json
{
  "general": {
    "uiLocale": "hu-HU",
    "startPage": "home",
    "restoreWindow": true,
    "showWindowsNotifications": false,
    "closeBehavior": "exit"
  }
}
```

Validation:

- locale must be installed or supported;
- start page must exist;
- tray behavior only if tray feature enabled.

## 4. Player defaults

```json
{
  "player": {
    "seekSeconds": 10,
    "completionThreshold": 0.90,
    "minimumResumeSeconds": 60,
    "volume": 0.75,
    "hardwareAcceleration": "auto",
    "autoplayNextEpisode": false,
    "preventSleepWhilePlaying": true,
    "rememberExplicitTrackSelection": false,
    "subtitleAutoEnable": false,
    "controlsHideAfterSeconds": 3,
    "pictureInPictureEnabled": true,
    "screenshotGalleryEnabled": true
  }
}
```

Constraints:

- seek 1–600;
- completion 0.50–0.99;
- volume 0–1;
- controls timeout 1–30.

## 5. Library defaults

```json
{
  "library": {
    "realtimeMonitoring": true,
    "startupReconciliation": true,
    "reconciliationIntervalHours": 24,
    "probeConcurrency": 2,
    "hashMode": "onDemand",
    "showMissingItems": true,
    "generateArtworkFallback": true,
    "minimumMediaDurationSeconds": 60
  }
}
```

Root-specific settings override global where allowed.

## 6. Metadata defaults

```json
{
  "metadata": {
    "displayLanguages": ["hu-HU", "original", "en"],
    "refreshMode": "scheduledAndManual",
    "cacheDays": 30,
    "preferLocalSidecars": true,
    "preserveUserEdits": true
  }
}
```

## 7. Episode monitoring defaults

```json
{
  "episodes": {
    "refreshAfterStartup": true,
    "refreshIntervalHours": 24,
    "requireAirDateAndProviderRelease": true,
    "retainInAppNotification": true,
    "showWindowsToast": false
  }
}
```

## 8. Subtitle defaults

```json
{
  "subtitles": {
    "automaticDownload": false,
    "onlineProviderEnabled": false,
    "preferredLanguages": [],
    "autoEnableDownloaded": false,
    "searchHearingImpaired": "include",
    "storeMode": "managedCache",
    "defaultDelayMilliseconds": 0
  }
}
```

Public 1.0 keeps `onlineProviderEnabled = false` and supports embedded, sidecar, and manual import. When online providers ship later, the fixed product rule is `autoEnableDownloaded = false`; UI may not silently change it.

## 9. Download defaults

```json
{
  "downloads": {
    "maximumParallel": 2,
    "throttleDuringPlayback": true,
    "temporaryRetentionHours": 24,
    "verifyChecksumWhenAvailable": true,
    "allowAutomaticAcquisition": false
  }
}
```

## 10. Plugin defaults

```json
{
  "plugins": {
    "allowExternalPackages": false,
    "showPackageInstaller": false,
    "builtInExtensionsEnabled": true
  }
}
```

Public 1.0 does not accept arbitrary `.mhpkg` installation. Developer test-host configuration may override this only in a separately signed/internal build.

## 11. Privacy defaults

```json
{
  "privacy": {
    "telemetryEnabled": false,
    "includeFullPathsInLocalLogs": false,
    "includeCrashDumpInSupportBundle": false,
    "useNotesForSearch": false,
    "maximumNoteCharacters": 10000
  }
}
```

Notes are compact plain text and are never recommendation inputs.

## 12. Update defaults

```json
{
  "updates": {
    "checkEnabled": true,
    "channel": "stable",
    "checkIntervalHours": 24,
    "automaticDownload": false,
    "automaticInstall": false
  }
}
```

Checking may default on if privacy review confirms only version/platform data is sent; user can disable.

## 13. Theme defaults

Theme tokens are stored separately from user overrides. Override file contains only changed tokens.

## 14. Root configuration

```json
{
  "id": "uuid",
  "path": "\\\\server\\media",
  "kind": "unc",
  "contentType": "mixed",
  "enabled": true,
  "monitorMode": "auto",
  "credentialReference": null,
  "exclusions": [
    "**/sample/**",
    "**/*.part"
  ]
}
```

Paths are validated through platform services, not JSON alone.

## 15. Built-in extension settings

Each built-in extension has a namespace:

```text
plugins/<plugin-id>/settings.json
```

Host validates declarative schema. Secrets are references only.

## 16. Migration

Settings schema migration:

- load old;
- validate known version;
- create backup;
- transform;
- validate new;
- atomic replace;
- retain last-known-good.

Unknown newer schema starts safe mode rather than discarding settings.

## 17. Environment and command-line overrides

Only documented diagnostic/development settings may be overridden. Secrets through command line are discouraged because process lists may expose them.

Precedence:

```text
safe-mode forced defaults
→ command-line supported overrides
→ user settings
→ built-in defaults
```

## 18. Reset levels

- reset appearance;
- reset player;
- reset library behavior without removing roots;
- reset provider settings;
- reset all preferences;
- factory reset, requiring explicit data impact choices.

Factory reset does not silently delete database/media.

## 19. Acceptance criteria

- Invalid values preserve prior valid settings.
- Secrets never appear in JSON.
- Online subtitle providers remain unavailable in public 1.0; future downloaded subtitles remain disabled by default.
- External plugin packages remain unavailable in public 1.0.
- Automatic acquisition remains disabled.
- Telemetry remains opt-in.
- Unknown newer schema triggers safe handling.


## 20. Development/Test backend configuration

```json
{
  "backend": {
    "provider": "supabase",
    "environment": "development",
    "supabase": {
      "url": "",
      "publishableKey": "",
      "protocolVersion": 1
    }
  },
  "synchronization": {
    "enabled": true,
    "identityMode": "autoSynthetic",
    "pushIntervalSeconds": 15,
    "pullIntervalSeconds": 60,
    "batchSize": 100,
    "maximumAttemptsBeforeDeadLetter": 12,
    "realtimeHintsEnabled": true,
    "syncPlaybackSummaries": true,
    "syncWatchEventDetails": false
  }
}
```

Every supported user-owned synchronization category is enabled together in Development/Test. Per-category user toggles are not part of the future public product decision.

The URL and publishable key may be present in development configuration because the desktop client is untrusted by design, but authorization still relies on authentication and RLS. Privileged keys are forbidden. Production endpoint selection is enforced by signed build configuration.

Secrets such as refresh tokens are not stored in this JSON. They use Windows-protected credential storage.

## 21. Public 1.0 backend configuration

```json
{
  "backend": {
    "provider": "supabase",
    "environment": "production",
    "personalSynchronizationEnabled": false,
    "allowedPublicCapabilities": [
      "metadata",
      "episodeSchedule",
      "featureCompatibility",
      "releaseCatalog"
    ]
  },
  "synchronization": {
    "enabled": false
  }
}
```

## 22. Environment overrides

- `appsettings.json`: safe common defaults;
- `appsettings.Development.json`: local/shared development endpoint;
- `appsettings.Staging.json`: staging endpoint in internal builds;
- production endpoint: packaged signed configuration;
- environment variables: allowed for developer machines and CI;
- user settings: cannot enable personal synchronization in public 1.0 and cannot replace trusted production endpoints; a later release may expose the single synchronization toggle.

---

## Document control

This document is part of the MediaHub revised baseline specification v1.2. Changes that affect architecture, security, data compatibility, plugin contracts, or user-visible behavior must be recorded in the Architecture Decision Record volume and reflected in the traceability matrix.
