# Volume 12 — Plugin, Provider and Extension SDK

**Project:** MediaHub  
**Document set:** MediaHub Project Bible  
**Status:** Revised baseline specification v1.2  
**Language:** English  
**Target platform:** Windows desktop first  
**Last updated:** 2026-08-08  

---

## 1. Goals

The plugin foundation exists in version 1 so later features do not require invasive architecture changes. Public 1.0 ships only approved built-in extensions. Contracts, manifests, compatibility validation, capability enforcement, and developer packaging tools are real, but end users cannot install arbitrary third-party `.mhpkg` packages until the hardened external-plugin milestone.

## 2. Extension categories

- metadata provider;
- subtitle provider;
- authorized search provider;
- authorized acquisition resolver;
- import/export provider;
- recommendation contributor;
- artwork processor;
- theme package;
- dashboard row provider, restricted;
- developer diagnostics extension, restricted.

## 3. Future external package format

Recommended package:

```text
plugin-id.version.mhpkg
├── manifest.json
├── lib/
│   └── netX/
├── resources/
├── locales/
├── assets/
├── LICENSE
└── README.md
```

The format is reserved and validated by developer tooling in 1.0. Public installation is disabled. When external installation ships, a package extracts into a plugin-specific directory only after validation. A trusted publisher signature establishes origin; a recorded hash may additionally detect later modification but never substitutes for publisher trust. Installing an untrusted local package requires an explicit risk and capability decision in the future hardened workflow.

## 4. Manifest

```json
{
  "schemaVersion": 1,
  "id": "com.example.metadata",
  "name": "Example Metadata",
  "version": "1.2.0",
  "publisher": "Example Publisher",
  "entryAssembly": "Example.Metadata.dll",
  "minimumHostVersion": "1.0.0",
  "sdkVersion": "1.0",
  "capabilities": ["metadata.search", "metadata.read", "network.https"],
  "supportedLocales": ["en", "hu"],
  "homepage": "https://example.invalid",
  "license": "MIT"
}
```

Unknown required fields fail installation. Unknown optional fields are ignored according to schema rules.

## 5. Versioning

- Plugin SDK follows semantic versioning.
- Major changes may break binary compatibility.
- Minor changes add backward-compatible capability.
- Patch changes fix behavior.
- Host negotiates SDK range.
- Compatibility tests validate old sample plugins against new host builds.

## 6. Capability model

Plugins declare capabilities:

- outbound HTTPS to configured domains;
- read metadata context;
- write provider cache;
- download subtitle file;
- submit acquisition candidate;
- contribute UI row;
- read selected media fingerprint;
- no direct file-system access by default.

The host provides narrow APIs. Built-in capabilities are declared and security-reviewed before release. Capability prompts are introduced with public external installation; no prompt can silently grant an undeclared capability.

## 7. Isolation

Runtime options:

- in-process for trusted built-in extensions;
- separate process for future third-party network plugins;
- timeouts and cancellation;
- memory and output limits;
- circuit breaker;
- disable after repeated crashes;
- no access to raw service provider or database.

Process isolation is required unless a future security ADR documents an equally strong alternative for untrusted external plugins.

## 8. Provider interfaces

```csharp
public interface IMetadataProvider
{
    ProviderDescriptor Descriptor { get; }

    Task<IReadOnlyList<MetadataSearchResult>> SearchAsync(
        MetadataSearchRequest request,
        ProviderContext context,
        CancellationToken cancellationToken);

    Task<MetadataEnvelope?> GetAsync(
        ExternalMediaId id,
        MetadataRequestOptions options,
        ProviderContext context,
        CancellationToken cancellationToken);
}
```

```csharp
public interface ISubtitleProvider
{
    Task<IReadOnlyList<SubtitleCandidate>> SearchAsync(
        SubtitleSearchRequest request,
        ProviderContext context,
        CancellationToken cancellationToken);

    Task<SubtitleDownload> DownloadAsync(
        SubtitleCandidateId id,
        ProviderContext context,
        CancellationToken cancellationToken);
}
```

Authorized acquisition contracts return candidates and a resolver result. They do not directly mutate the library or invoke arbitrary executables.

## 9. Host services

Safe services may include:

- provider HTTP client with domain restrictions;
- cache;
- localization;
- structured plugin logger;
- clock;
- user consent prompt;
- temporary file service;
- secure secret reference service;
- rate limiter;
- progress reporter.

Plugins cannot retrieve other plugin secrets.

## 10. Secrets

A provider may request a named secret. The host stores it in OS-protected storage and returns a scoped token or value only to that plugin runtime. Secrets are masked in UI and logs.

## 11. HTTP policy

Provider HTTP clients enforce:

- HTTPS by default;
- allowed hostnames;
- redirects policy;
- timeout;
- size limit;
- decompression limit;
- user agent identification;
- rate limits;
- retry only for safe operations;
- certificate validation;
- no local-network access unless explicitly permitted.

## 12. UI contributions

To preserve design and security, plugins do not inject arbitrary WPF controls in baseline third-party mode. They return declarative cards, rows, commands, or settings schemas rendered by MediaHub.

Example declarative setting:

```json
{
  "key": "preferredLanguage",
  "type": "select",
  "labelResource": "Settings.PreferredLanguage",
  "options": ["hu", "en", "ja"]
}
```

## 13. Plugin lifecycle

### Public 1.0 built-in lifecycle

1. Discover the built-in manifest from the signed application bundle.
2. Validate manifest and host compatibility.
3. Apply pre-approved capabilities.
4. Activate through the versioned contract.
5. Monitor health and circuit-break failures.
6. Allow enable/disable only where disabling does not break required core behavior.

### Future external package lifecycle

1. Discover package.
2. Validate archive.
3. Verify signature/hash.
4. Parse manifest.
5. Check compatibility.
6. Present permissions.
7. Install disabled if untrusted.
8. Activate.
9. Health check.
10. Load capabilities.
11. Monitor errors.
12. Upgrade or rollback.
13. Disable or uninstall.

Uninstall preserves user-owned provider mappings unless the user chooses to purge them.

The public 1.0 interface does not expose package installation, third-party permission approval, or external uninstall actions.

## 14. Plugin state

Each plugin receives isolated state:

- settings;
- cache;
- schema version;
- secrets references;
- health history.

Plugin migrations cannot access host tables directly.

## 15. Failure policy

- timeout returns provider failure;
- malformed result is rejected;
- repeated failure opens circuit;
- plugin is marked degraded;
- host local features continue;
- user can inspect health and disable plugin;
- no silent fallback to a different provider for destructive or acquisition actions.

## 16. Developer tooling

SDK distribution includes:

- NuGet package;
- manifest schema;
- sample providers;
- test host;
- compatibility validator;
- packaging command;
- signing guidance;
- security checklist;
- localization guide.

## 17. Marketplace readiness

Even before a marketplace exists, manifests include publisher and license. Future marketplace requirements:

- signed packages;
- review;
- permission disclosure;
- privacy policy;
- update channel;
- vulnerability revocation;
- user ratings separated from code trust.

## 18. Acceptance criteria

- An incompatible built-in extension fails validation with a clear diagnostic and cannot activate.
- A crashing built-in extension does not crash playback.
- Plugin secrets do not appear in logs.
- A plugin cannot read the database directly.
- Built-in capabilities are inspectable in diagnostics.
- Plugin state survives compatible updates.
- The host can disable a provider without damaging local metadata.
- Public 1.0 exposes no action that installs an arbitrary `.mhpkg` package.


## Revision 1.1 backend integration note

Cloud-connected status and actions must use the same local-first UX language: local content remains usable, synchronization state is non-blocking, errors are actionable, and no screen presents backend availability as equivalent to media-library availability. Plugin and UI contracts must consume backend-neutral services rather than Supabase SDK objects.

---

## Document control

This document is part of the MediaHub revised baseline specification v1.2. Changes that affect architecture, security, data compatibility, plugin contracts, or user-visible behavior must be recorded in the Architecture Decision Record volume and reflected in the traceability matrix.
