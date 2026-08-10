# Volume 15 — Security, Privacy and Threat Model

**Project:** MediaHub  
**Document set:** MediaHub Project Bible  
**Status:** Revised baseline specification v1.2  
**Language:** English  
**Target platform:** Windows desktop first  
**Last updated:** 2026-08-08  

---

## 1. Security objectives

MediaHub processes untrusted filenames, media containers, images, subtitle files, metadata payloads, plugin packages, network paths, and downloaded data. Security is a first-class architectural property.

Primary objectives:

- prevent code execution from media workflows;
- protect credentials and tokens;
- prevent path traversal and unauthorized file access;
- isolate plugins and providers;
- preserve database integrity;
- avoid leaking private viewing data;
- make destructive actions explicit;
- ensure update authenticity.

## 2. Trust boundaries

```text
User input
File system / network share
Media files and containers
FFprobe / playback engine
Remote providers
Downloaded files
Plugin packages
Update service
Operating-system credential store
MediaHub database
```

Anything outside the MediaHub domain/application layers is treated as untrusted until validated.

## 3. Threat actors and failures

- malicious or malformed media file;
- malicious subtitle or artwork;
- compromised provider;
- compromised plugin package;
- local malware attempting to read secrets;
- network attacker interfering with provider traffic;
- accidental user deletion;
- buggy scanner causing data loss;
- update server compromise;
- archive bomb;
- path traversal through metadata;
- credential leakage through logs;
- denial of service through huge libraries or files.

## 4. Secure coding requirements

- **SEC-001:** Use parameterized database access.
- **SEC-002:** Never concatenate untrusted input into shell commands.
- **SEC-003:** Use argument arrays for external processes.
- **SEC-004:** Normalize and validate paths against allowed roots.
- **SEC-005:** Enforce maximum lengths and collection sizes.
- **SEC-006:** Parse remote JSON with explicit schemas and limits.
- **SEC-007:** Validate redirects and final domains.
- **SEC-008:** Use TLS certificate validation.
- **SEC-009:** Store secrets in OS-protected storage.
- **SEC-010:** Redact secrets and private notes from logs.
- **SEC-011:** Production binaries and updates are signed.
- **SEC-012:** Deserialization must not instantiate arbitrary types.
- **SEC-013:** Temporary files use restrictive ACLs.
- **SEC-014:** Security-sensitive comparisons use canonical representations.

## 5. Path safety

Before file operations:

1. reject null bytes and invalid characters;
2. canonicalize;
3. resolve relative segments;
4. resolve symbolic links/reparse points according to policy;
5. verify target remains inside allowed root for provider-controlled paths;
6. reject device paths unless explicitly supported;
7. verify operation type and permissions.

Display paths must not be reused as trusted normalized paths.

## 6. External process safety

FFprobe and future helper processes run with:

- fixed executable selected by MediaHub;
- no shell;
- explicit arguments;
- working directory outside media folder;
- timeout and cancellation;
- bounded stdout/stderr;
- limited environment variables;
- version verification;
- integrity checks for bundled binaries.

## 7. Media and image handling

Media parsers and decoders have a history of vulnerabilities. Controls:

- keep playback and probing dependencies updated;
- prefer vendor security releases;
- disable unnecessary protocols;
- do not allow arbitrary URL playback by default;
- cap image dimensions and decompressed size;
- reject malformed artwork;
- consider process isolation for thumbnail generation in hardened editions;
- create crash containment around engine calls.

## 8. Subtitle safety

Subtitles are data, never code. MediaHub:

- rejects executable extensions;
- limits archive extraction count and total size;
- prevents extraction outside temporary directory;
- strips unsafe HTML in UI previews;
- does not open subtitle files with shell execution;
- validates text encodings;
- records provider provenance.

## 9. Plugin security

- least-privilege capabilities;
- manifest validation;
- signature/hash;
- restricted HTTP domains;
- no raw database access;
- isolated state;
- timeout;
- circuit breaker;
- optional out-of-process execution;
- user-visible permissions;
- revocation capability.

Built-in trust does not excuse validation.

## 10. Credential storage

Secrets include:

- provider API keys;
- network credentials;
- update tokens if ever used;
- plugin tokens.

Store with Windows credential APIs or DPAPI-bound protection. Database stores only a reference ID. Export excludes secrets unless a dedicated encrypted backup feature is later designed.

## 11. Update security

- signed update manifest;
- signed package;
- cryptographic hash;
- HTTPS;
- rollback protection balanced with recovery;
- package verification before execution;
- no update command from unsigned plugin;
- release channel separation;
- transparent version and publisher.

## 12. Privacy data inventory

Potential personal data:

- titles watched;
- timestamps;
- ratings;
- private notes;
- searches;
- screenshots;
- network paths;
- provider accounts;
- device characteristics.

Default retention is local. Optional telemetry must use coarse, non-content operational metrics unless separately consented.

## 13. Telemetry principles

- disabled by default;
- opt-in;
- category-level controls;
- visible payload description;
- no filenames;
- no notes;
- no watch titles;
- no credentials;
- no media fingerprints unless explicitly required for a user-requested provider action;
- deletion and consent withdrawal.

## 14. Audit trail

Local audit entries record sensitive operations:

- library root added/removed;
- delete requested and result;
- plugin installed/permission changed;
- provider secret updated;
- database repair;
- import/export;
- update installed.

Audit entries avoid private content and can be cleared only through an explicit privacy workflow.

## 15. Threat table

| Threat | Control |
|---|---|
| Path traversal from provider filename | canonical destination validation and generated filename |
| Malicious plugin | permissions, signature, isolation, disable |
| Executable disguised as video | extension + magic + probe validation |
| NAS outage interpreted as deletion | root reachability state and delayed missing verification |
| Credential in log | structured redaction and secret types |
| Corrupt database migration | backup, transaction, recovery mode |
| MITM provider response | HTTPS and certificate validation |
| Compromised update | signature and hash verification |
| Archive bomb | extraction limits |
| UI spoofing by plugin | declarative host-rendered UI |

## 16. Incident response

- disable affected plugin/provider;
- revoke package version;
- publish advisory;
- preserve diagnostics;
- rotate signing or API keys if needed;
- provide repair tool;
- document affected versions;
- provide offline update package where necessary.

## 17. Security testing

- static analysis;
- dependency vulnerability scanning;
- secret scanning;
- fuzz filename and parser inputs;
- malformed JSON and image tests;
- path traversal tests;
- archive extraction tests;
- plugin sandbox tests;
- update signature tests;
- database corruption recovery;
- penetration testing before commercial launch.

## 18. Acceptance criteria

- Download manager cannot commit executable payloads.
- Provider filename cannot escape destination.
- Secrets are absent from logs and exports.
- Unsigned update is rejected.
- Offline NAS does not trigger destructive cleanup.
- Plugin permissions are enforceable.
- Database migration failure enters recovery safely.


## 19. Supabase-specific security controls

For Windows 1.0, user-owned synchronization controls are active only in Development/Test builds using synthetic identities and approved data. Public 1.0 remote access is limited to approved non-personal capabilities such as metadata proxying, episode schedules, compatibility data, and the release catalog. The same controls become mandatory for the later public synchronization feature.

- **SEC-CLOUD-001:** The desktop client may contain only a publishable/public client key appropriate for untrusted clients.
- **SEC-CLOUD-002:** A `service_role` or equivalent privileged key shall never be shipped in the executable, configuration bundle, logs, crash dumps, or plugin context.
- **SEC-CLOUD-003:** Every user-owned remote table shall enable Row Level Security before client access is permitted.
- **SEC-CLOUD-004:** Policies shall verify ownership from the authenticated identity and shall not trust a client-supplied `user_id` without policy enforcement.
- **SEC-CLOUD-005:** Privileged provider secrets and AI credentials shall exist only in server-side secret storage and be used through Edge Functions.
- **SEC-CLOUD-006:** Authentication sessions shall be stored through Windows-protected credential storage, not plaintext settings.
- **SEC-CLOUD-007:** Realtime payloads shall be schema-validated and followed by an authorized delta query where data integrity matters.
- **SEC-CLOUD-008:** Development, staging, and production Supabase projects shall use separate keys, databases, storage buckets, and auth users.
- **SEC-CLOUD-009:** Remote logs and analytics shall exclude media paths, notes, filenames that reveal sensitive content, and credentials.
- **SEC-CLOUD-010:** Database functions exposed to clients shall use least privilege, explicit search paths, and reviewed execution rights.

Compact note text is stored as ordinary variable-length text protected by authentication and RLS when Development/Test synchronization or the later public synchronization feature is enabled. Client-side/end-to-end note encryption is not a baseline requirement. Notes remain excluded from logs, telemetry, AI inputs, and provider requests.

Public Windows 1.0 cannot install third-party `.mhpkg` packages. Built-in extensions are part of the signed application bundle and still undergo manifest, capability, input, and failure-isolation validation.

## 20. Cloud threat additions

Threats include stolen refresh tokens, over-broad RLS policies, malicious remote rows, replayed mutations, compromised Edge Functions, environment-key mix-ups, dependency compromise in the community C# client, and accidental synchronization of local-sensitive fields. Controls include short-lived access tokens, refresh-token protection, idempotency keys, strict DTO allow-lists, dependency pinning, integration tests against policies, and release-time environment validation.

---

## Document control

This document is part of the MediaHub revised baseline specification v1.2. Changes that affect architecture, security, data compatibility, plugin contracts, or user-visible behavior must be recorded in the Architecture Decision Record volume and reflected in the traceability matrix.
