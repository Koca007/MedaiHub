# Volume 20 — Developer Standards and Repository Workflow

**Project:** MediaHub  
**Document set:** MediaHub Project Bible  
**Status:** Revised baseline specification v1.2  
**Language:** English  
**Target platform:** Windows desktop first  
**Last updated:** 2026-08-08  

---

## 1. Engineering principles

- object-oriented design with behavior-rich domain objects;
- SOLID applied pragmatically;
- dependency inversion at external boundaries;
- small cohesive classes;
- asynchronous I/O;
- cancellation support;
- secure defaults;
- explicit failure handling;
- English developer documentation and comments;
- tests as part of implementation, not a later phase.

## 2. C# standards

- current supported C# version for chosen .NET baseline;
- nullable enabled;
- file-scoped namespaces;
- one public type per file unless tightly coupled records are clearer;
- immutable records for messages and DTOs;
- sealed classes by default unless inheritance is intended;
- `readonly` where applicable;
- `DateTimeOffset` or `Instant`-style abstraction for timestamps;
- UTC persistence;
- no `async void` except UI event bridges;
- `CancellationToken` last parameter;
- avoid blocking `.Result` and `.Wait()`;
- no empty catch blocks;
- culture specified for parsing and formatting;
- `ConfigureAwait` policy documented per project.

## 3. Naming

- namespaces: `MediaHub.<Module>.<Area>`;
- interfaces: `IName`;
- asynchronous methods: `Async`;
- commands: imperative, e.g. `AddLibraryRootCommand`;
- queries: noun or question, e.g. `GetHomeRowsQuery`;
- events: past tense, e.g. `MediaFileDiscovered`;
- database migrations: timestamp plus purpose;
- resource keys: semantic dotted names.

## 4. Documentation comments

Public SDK and non-obvious domain APIs use XML documentation.

```csharp
/// <summary>
/// Moves the selected local media files to the Windows Recycle Bin while
/// preserving the logical title and all user-owned metadata.
/// </summary>
/// <remarks>
/// Network locations without verified recycle support are rejected.
/// </remarks>
public Task<DeleteMediaResult> MoveToRecycleBinAsync(
    DeleteMediaRequest request,
    CancellationToken cancellationToken);
```

Comments explain why and constraints, not restate obvious code.

## 5. Error handling

- expected validation returns typed result;
- infrastructure exceptions translated at boundary;
- original exception retained as inner exception;
- user messages localized outside domain;
- log once at the responsible boundary;
- cancellation is not logged as an error;
- security validation failures use stable event IDs.

## 6. Async and concurrency

- no unbounded `Task.WhenAll`;
- use channels or bounded queues;
- database context not shared across threads;
- background jobs own scopes;
- progress reporting throttled;
- file-system race conditions expected;
- locks have narrow scope and documented order;
- avoid global locks around playback.

## 7. Dependency management

- central package version management;
- lock or deterministic restore policy;
- dependency licenses tracked;
- vulnerability scanning;
- no abandoned package without review;
- wrappers around critical third-party APIs;
- update playback and parsing libraries on security releases;
- SBOM in release pipeline.

## 8. Project boundaries

Forbidden references are tested:

- Domain → Infrastructure
- Domain → WPF
- Application → concrete database
- Plugin SDK → host implementation
- UI → EF Core
- Scanner → player implementation

## 9. Testing standards

- test names describe behavior;
- Arrange/Act/Assert or Given/When/Then;
- no dependence on execution order;
- deterministic clock and IDs through abstractions;
- temporary directories isolated;
- integration tests clean up;
- flaky test quarantining requires owner and expiry;
- coverage is a signal, not sole quality metric.

## 10. Repository layout

```text
/
├── .github/
├── build/
├── docs/
├── eng/
├── samples/
├── src/
├── tests/
├── tools/
├── Directory.Build.props
├── Directory.Packages.props
├── MediaHub.sln
├── README.md
├── SECURITY.md
├── CONTRIBUTING.md
├── CODE_OF_CONDUCT.md
└── LICENSE
```

## 11. Branching

Recommended trunk-based workflow:

- protected `main`;
- short-lived feature branches;
- pull request required;
- linear or squash merge;
- release tags;
- support branches only when necessary.

## 12. Commit messages

Use clear imperative summaries, optionally Conventional Commits:

```text
feat(scanner): preserve identity across file rename
fix(player): checkpoint progress before decoder fallback
docs(sdk): clarify provider capability permissions
```

Commits should be reviewable and avoid mixing unrelated refactors.

## 13. Pull request checklist

- linked requirement/issue;
- architecture impact;
- security impact;
- migration impact;
- tests;
- screenshots for UI;
- localization;
- accessibility;
- documentation;
- performance evidence when hot path;
- no secrets;
- dependency justification.

## 14. Code review

Reviewers examine:

- correctness;
- invariants;
- error paths;
- cancellation;
- concurrency;
- security;
- privacy;
- database query behavior;
- UI thread usage;
- test adequacy;
- maintainability;
- backwards compatibility.

At least one domain owner reviews changes to persistence, player, plugin SDK, updates, or deletion.

## 15. Continuous integration

Pipeline stages:

1. restore;
2. format check;
3. compile with warnings as errors;
4. unit tests;
5. architecture tests;
6. integration tests;
7. security/static analysis;
8. dependency scan;
9. package test;
10. UI smoke tests on Windows;
11. Windows 10 and Windows 11 x64 release-matrix tests;
12. artifact signing in protected release pipeline;
13. SBOM.

## 16. Feature flags

No dead or permanent flags. Every flag includes:

- issue;
- owner;
- removal version/date;
- default;
- test matrix.

## 17. API compatibility

- public SDK API reviewed;
- API baseline file;
- breaking-change detection;
- obsolete before removal where possible;
- migration guide;
- sample plugin compatibility tests.

Public 1.0 compatibility tests exercise approved built-in extensions and the developer test host. They must also prove that arbitrary external `.mhpkg` installation is unavailable in the production UI and command surface.

## 18. Documentation workflow

Architecture and requirement changes update docs in the same pull request. ADR required for:

- framework/engine change;
- persistence technology;
- plugin permission model;
- update packaging;
- privacy default;
- destructive-operation semantics.

## 19. Security reporting

Repository includes `SECURITY.md` with private reporting channel, supported versions, response expectations, and disclosure policy before commercial launch.

## 20. Release ownership

Release manager verifies:

- version numbers;
- migrations;
- signatures;
- release notes;
- rollback plan;
- support bundle;
- known issues;
- distribution links;
- source and binary provenance.


## 21. Supabase repository workflow

Remote backend changes are source-controlled under `supabase/`.

- Every schema change is a numbered migration.
- RLS policy changes require tests in the same pull request.
- Edge Functions use TypeScript, strict mode, input schemas, structured errors, and server-side secret access.
- Seed data contains synthetic media identities only.
- Developers use local Supabase where practical; shared Development is for integration, not personal schema experiments.
- Generated database types may be committed only in an isolated generated-code directory.
- C# mapping code translates generated/SDK types into MediaHub contracts.
- Pull requests that modify synchronized entities must update both local and remote migration plans plus conflict tests.
- Development/Test identities are synthetic and provisioned automatically; production user-data synchronization remains feature-gated after 1.0.

---

## Document control

This document is part of the MediaHub revised baseline specification v1.2. Changes that affect architecture, security, data compatibility, plugin contracts, or user-visible behavior must be recorded in the Architecture Decision Record volume and reflected in the traceability matrix.
