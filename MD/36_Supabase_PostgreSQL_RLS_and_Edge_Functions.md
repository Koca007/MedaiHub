# Volume 36 — Supabase PostgreSQL, RLS, and Edge Functions

**Project:** MediaHub  
**Document set:** MediaHub Project Bible  
**Status:** Revised baseline specification v1.2  
**Language:** English  
**Target platform:** Windows desktop first  
**Last updated:** 2026-08-08  

---

## 1. Purpose

This volume specifies the remote database model, ownership rules, migration conventions, Row Level Security policies, server-side functions, storage boundaries, and validation requirements for the Supabase development and testing backend.

The public 1.0 desktop application has no sign-in and does not synchronize personal data. Its Supabase access is limited to non-personal metadata, episode-release state, release compatibility, and the application release catalog. User-owned tables are exercised in development and testing through an automatically provisioned synthetic user and become public product functionality only when account-based synchronization ships after 1.0.

SQL examples are normative in intent and may be refined during implementation benchmarking. Every deployed object must be represented by source-controlled migrations.

## 2. Database conventions

- PostgreSQL `uuid` is used for stable identifiers.
- `timestamptz` stores timestamps in UTC.
- `bigint` identity or controlled sequence values provide ordered deltas where required.
- Tables use `snake_case`.
- User-owned tables contain `user_id uuid not null`.
- Remote rows contain `created_at`, `updated_at`, and revision/sequence fields.
- Client-supplied ownership is never sufficient without RLS.
- Free-form JSON is limited to versioned payload envelopes and validated at boundaries.
- Deletions that must reach offline devices use tombstones.
- Database triggers are small, deterministic, and covered by tests.
- Public, non-personal endpoints never accept a caller-supplied `user_id`.
- User-owned relations use composite foreign keys so a valid row ID cannot be paired with another user's ownership ID.

## 3. Core identity tables

```sql
create table public.profiles (
    user_id uuid primary key references auth.users(id) on delete cascade,
    display_name text null,
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now()
);

create table public.devices (
    id uuid primary key,
    user_id uuid not null references auth.users(id) on delete cascade,
    device_name text not null,
    platform text not null,
    app_version text not null,
    protocol_version integer not null,
    last_seen_at timestamptz not null default now(),
    created_at timestamptz not null default now(),
    unique (user_id, id)
);
```

A device registration does not represent a separate viewing profile.

## 4. Media identity

Remote synchronization does not store physical files. It stores logical identities.

```sql
create table public.media_identities (
    id uuid primary key,
    user_id uuid not null references auth.users(id) on delete cascade,
    canonical_key text not null,
    media_type text not null,
    display_title text not null,
    release_year integer null,
    series_id uuid null,
    season_number integer null,
    episode_number integer null,
    external_ids jsonb not null default '{}'::jsonb,
    identity_fingerprint text null,
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now(),
    revision bigint not null default 1,
    unique (user_id, id),
    unique (user_id, canonical_key),
    foreign key (user_id, series_id)
        references public.media_identities(user_id, id)
);
```

Titles may be minimized in privacy-sensitive configurations. External IDs are allow-listed keys, not arbitrary provider payloads. `canonical_key` is stable across the user's devices: prefer an allow-listed provider identity, otherwise retain the first synchronized MediaHub identity rather than regenerating it from a mutable title.

Before the first future account synchronization, the client shows a merge preview and imports remote identities before matching the local library. Matching order is external ID, series/season/episode coordinates, then normalized title/year/fingerprint. Ambiguous matches require user review and are never overwritten automatically. Switching accounts requires explicitly detaching the current account.

## 5. Ratings and notes

```sql
create table public.ratings (
    id uuid primary key,
    user_id uuid not null references auth.users(id) on delete cascade,
    media_identity_id uuid not null,
    stars smallint not null check (stars between 1 and 5),
    updated_by_device_id uuid not null,
    updated_at timestamptz not null default now(),
    revision bigint not null default 1,
    idempotency_key uuid not null,
    unique (user_id, id),
    unique (user_id, media_identity_id),
    unique (user_id, idempotency_key),
    foreign key (user_id, media_identity_id)
        references public.media_identities(user_id, id),
    foreign key (user_id, updated_by_device_id)
        references public.devices(user_id, id)
);

create table public.favorites (
    id uuid primary key,
    user_id uuid not null references auth.users(id) on delete cascade,
    media_identity_id uuid not null,
    is_favorite boolean not null,
    updated_by_device_id uuid not null,
    updated_at timestamptz not null default now(),
    revision bigint not null default 1,
    idempotency_key uuid not null,
    unique (user_id, id),
    unique (user_id, media_identity_id),
    unique (user_id, idempotency_key),
    foreign key (user_id, media_identity_id)
        references public.media_identities(user_id, id),
    foreign key (user_id, updated_by_device_id)
        references public.devices(user_id, id)
);

create table public.notes (
    id uuid primary key,
    user_id uuid not null references auth.users(id) on delete cascade,
    media_identity_id uuid not null,
    note_text text not null check (char_length(note_text) <= 10000),
    updated_by_device_id uuid not null,
    updated_at timestamptz not null default now(),
    revision bigint not null default 1,
    idempotency_key uuid not null,
    unique (user_id, id),
    unique (user_id, media_identity_id),
    unique (user_id, idempotency_key),
    foreign key (user_id, media_identity_id)
        references public.media_identities(user_id, id),
    foreign key (user_id, updated_by_device_id)
        references public.devices(user_id, id)
);

create table public.note_versions (
    id uuid primary key,
    user_id uuid not null references auth.users(id) on delete cascade,
    note_id uuid not null,
    note_text text not null check (char_length(note_text) <= 10000),
    source_device_id uuid not null,
    base_revision bigint not null,
    conflict_state text not null check (conflict_state in ('accepted', 'conflict', 'resolved')),
    created_at timestamptz not null default now(),
    unique (user_id, id),
    foreign key (user_id, note_id)
        references public.notes(user_id, id) on delete cascade,
    foreign key (user_id, source_device_id)
        references public.devices(user_id, id)
);
```

Favorites are independent of ratings. Notes are compact plain text: no Markdown or HTML is stored, the client rejects more than 10,000 characters, and the remote copy relies on authentication plus RLS rather than client-side or end-to-end encryption. Concurrent note edits preserve both versions for an explicit merge instead of blindly overwriting either value.

## 6. Collections

```sql
create table public.collections (
    id uuid primary key,
    user_id uuid not null references auth.users(id) on delete cascade,
    name text not null,
    description text null,
    kind text not null check (kind in ('manual', 'automatic')),
    rule_payload jsonb null,
    sort_order integer not null default 0,
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now(),
    revision bigint not null default 1,
    deleted_at timestamptz null,
    unique (user_id, id),
    check (
        (kind = 'manual' and rule_payload is null)
        or (kind = 'automatic' and rule_payload is not null)
    )
);

create table public.collection_items (
    id uuid primary key,
    user_id uuid not null references auth.users(id) on delete cascade,
    collection_id uuid not null,
    media_identity_id uuid not null,
    added_at timestamptz not null default now(),
    deleted_at timestamptz null,
    revision bigint not null default 1,
    unique (user_id, id),
    unique (user_id, collection_id, media_identity_id),
    foreign key (user_id, collection_id)
        references public.collections(user_id, id) on delete cascade,
    foreign key (user_id, media_identity_id)
        references public.media_identities(user_id, id)
);
```

In 1.0 automatic collection rules and their evaluated membership are local. After account synchronization ships, all supported collection definitions synchronize automatically; automatic membership is re-evaluated locally and is not uploaded as a snapshot.

## 7. Playlists

```sql
create table public.playlists (
    id uuid primary key,
    user_id uuid not null references auth.users(id) on delete cascade,
    name text not null,
    description text null,
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now(),
    revision bigint not null default 1,
    deleted_at timestamptz null,
    unique (user_id, id)
);

create table public.playlist_items (
    id uuid primary key,
    user_id uuid not null references auth.users(id) on delete cascade,
    playlist_id uuid not null,
    media_identity_id uuid not null,
    position_key text not null,
    added_at timestamptz not null default now(),
    updated_at timestamptz not null default now(),
    deleted_at timestamptz null,
    revision bigint not null default 1,
    unique (user_id, id),
    unique (user_id, playlist_id, id),
    unique (user_id, playlist_id, position_key),
    foreign key (user_id, playlist_id)
        references public.playlists(user_id, id) on delete cascade,
    foreign key (user_id, media_identity_id)
        references public.media_identities(user_id, id)
);
```

`position_key` supports insertion/reordering without renumbering the entire playlist. The final algorithm is chosen during implementation and must be deterministic.

## 8. Playback summaries

```sql
create table public.playback_summaries (
    id uuid primary key,
    user_id uuid not null references auth.users(id) on delete cascade,
    media_identity_id uuid not null,
    position_seconds integer not null default 0,
    duration_seconds integer not null default 0,
    completion_state text not null,
    semantic_action text null,
    last_played_at timestamptz null,
    updated_by_device_id uuid not null,
    updated_at timestamptz not null default now(),
    revision bigint not null default 1,
    idempotency_key uuid not null,
    unique (user_id, id),
    unique (user_id, media_identity_id),
    unique (user_id, idempotency_key),
    foreign key (user_id, media_identity_id)
        references public.media_identities(user_id, id),
    foreign key (user_id, updated_by_device_id)
        references public.devices(user_id, id)
);
```

Raw per-second playback events are not stored. Optional watch summaries use aggregated fields and separate consent/configuration.

## 9. Preferences

```sql
create table public.user_preferences (
    user_id uuid primary key references auth.users(id) on delete cascade,
    payload jsonb not null default '{}'::jsonb,
    schema_version integer not null,
    updated_by_device_id uuid not null,
    updated_at timestamptz not null default now(),
    revision bigint not null default 1,
    foreign key (user_id, updated_by_device_id)
        references public.devices(user_id, id)
);
```

The payload schema contains only device-independent allow-listed fields. Backend validation rejects unknown secret-bearing or local-path fields.

## 10. Recommendation events and candidates after 1.0

These tables are dormant in the public 1.0 product. When AI recommendations ship, the learned preference space is limited to genre and quality. Star ratings and watch history contribute weights to those two dimensions only; director, cast, person, franchise, and unrelated behavioral affinity are excluded.

```sql
create table public.recommendation_events (
    id uuid primary key,
    user_id uuid not null references auth.users(id) on delete cascade,
    media_identity_id uuid null,
    event_type text not null,
    genre_keys text[] not null default '{}',
    quality_class text null,
    rating smallint null,
    watch_fraction double precision null check (watch_fraction between 0 and 1),
    occurred_at timestamptz not null,
    device_id uuid not null,
    idempotency_key uuid not null,
    unique (user_id, id),
    unique (user_id, idempotency_key),
    foreign key (user_id, media_identity_id)
        references public.media_identities(user_id, id),
    foreign key (user_id, device_id)
        references public.devices(user_id, id)
);

create table public.recommendation_candidates (
    id uuid primary key,
    user_id uuid not null references auth.users(id) on delete cascade,
    media_identity_id uuid not null,
    score double precision not null,
    explanation_code text not null,
    explanation_parameters jsonb not null default '{}'::jsonb,
    algorithm_version text not null,
    generated_at timestamptz not null default now(),
    expires_at timestamptz not null,
    unique (user_id, id),
    foreign key (user_id, media_identity_id)
        references public.media_identities(user_id, id)
);
```

No free-form notes, local paths, director keys, or cast keys are included. Same-director and similar-title rows remain deterministic catalog queries and do not alter the learned profile.

## 11. Release catalog and feature flags

```sql
create table public.application_releases (
    id uuid primary key,
    version text not null unique,
    channel text not null,
    published_at timestamptz not null,
    minimum_protocol integer not null,
    maximum_protocol integer null,
    changelog jsonb not null,
    download_reference text null,
    is_active boolean not null default true
);

create table public.feature_flags (
    key text primary key,
    enabled boolean not null,
    minimum_app_version text null,
    maximum_app_version text null,
    environment text not null,
    payload jsonb not null default '{}'::jsonb,
    updated_at timestamptz not null default now()
);

create table public.metadata_cache (
    provider_key text not null,
    media_type text not null,
    external_id text not null,
    locale text not null,
    payload jsonb not null,
    attribution jsonb not null default '{}'::jsonb,
    fetched_at timestamptz not null default now(),
    expires_at timestamptz not null,
    primary key (provider_key, media_type, external_id, locale)
);

create table public.episode_release_catalog (
    provider_key text not null,
    series_external_id text not null,
    season_number integer not null,
    episode_number integer not null,
    region_code text not null,
    official_air_date date null,
    provider_available boolean not null default false,
    provider_available_at timestamptz null,
    updated_at timestamptz not null default now(),
    primary key (
        provider_key,
        series_external_id,
        season_number,
        episode_number,
        region_code
    )
);
```

Client read access is controlled and these tables are not user-writable. Metadata responses resolve language in the order Hungarian, original language, then English. An episode becomes remotely available only when its official air date has arrived and the configured provider reports it released/available. The local file scanner separately reports `available in library`; that state is never inferred from this catalog. The desktop refreshes episode status after application start and no more than once per day unless the user explicitly requests a refresh.

## 12. Change stream

A consistent delta mechanism is required. One approach uses a global change table populated by controlled triggers or mutation functions.

```sql
create table public.user_change_log (
    sequence bigint generated always as identity primary key,
    user_id uuid not null,
    entity_type text not null,
    entity_id uuid not null,
    operation text not null,
    revision bigint not null,
    changed_at timestamptz not null default now()
);

create index user_change_log_user_sequence_idx
on public.user_change_log(user_id, sequence);
```

The client queries `sequence > cursor` for its authenticated user. Retention must exceed the supported offline window. If a cursor falls outside retention, the client performs a bounded full reconciliation.

## 13. Row Level Security baseline

Every user-owned table enables RLS.

```sql
alter table public.ratings enable row level security;

create policy ratings_select_own
on public.ratings
for select
to authenticated
using (user_id = auth.uid());

create policy ratings_insert_own
on public.ratings
for insert
to authenticated
with check (user_id = auth.uid());

create policy ratings_update_own
on public.ratings
for update
to authenticated
using (user_id = auth.uid())
with check (user_id = auth.uid());

create policy ratings_delete_own
on public.ratings
for delete
to authenticated
using (user_id = auth.uid());
```

Equivalent ownership policies apply to every synchronized user table. Foreign-key relationships must not allow cross-user references. Tests create at least two identities and verify isolation for select, insert, update, delete, RPC, Realtime visibility, and Storage access.

## 14. Trusted mutation functions

Complex idempotent mutations may use server-side database functions or Edge Functions. A trusted mutation must:

- authenticate the user;
- validate payload schema and size;
- derive ownership from the session;
- verify idempotency key;
- perform one transaction;
- increment revision;
- append change-log entry;
- return accepted revision and server timestamp;
- emit no secret data in errors.

Direct table upsert is acceptable only when the same guarantees are enforced by constraints, triggers, and RLS.

## 15. Edge Functions

Planned functions:

- `get-media-metadata` for the public 1.0 read path
- `get-episode-release-state` for the public 1.0 read path
- `check-release-compatibility`
- `register-device` after account synchronization ships
- `export-user-cloud-data` after account synchronization ships
- `delete-user-cloud-data` after account synchronization ships
- `generate-recommendations` after AI recommendations ship
- `validate-plugin-marketplace-entry` in a later commercial phase

Every function uses:

- explicit request DTO;
- schema validation;
- authenticated identity for user-owned operations;
- timeout and cancellation where supported;
- least-privilege database access;
- server-only secrets;
- structured error codes;
- correlation ID;
- rate limiting appropriate to risk;
- no raw stack trace returned to the client.

The two public 1.0 read functions accept no owner identifier, expose only allow-listed DTOs, and are independently rate-limited. Provider credentials remain server-side. Public 1.0 does not expose personal mutation, device registration, export, deletion, or recommendation endpoints in the UI.

## 16. Storage policy

Allowed future buckets:

- `user-exports`: short-lived generated exports;
- `support-bundles`: explicit user uploads with expiry;
- `synced-artwork`: optional, size-limited, image-validated assets;
- `release-assets`: trusted update metadata or references, not necessarily installer binaries.

Forbidden automatic uploads:

- original videos;
- full media directories;
- executable plugins from untrusted sources;
- plaintext secrets;
- raw application databases without explicit encrypted backup design.

Storage policies scope objects to the authenticated user where applicable.

## 17. Migrations

Migration rules:

1. migrations are immutable after shared deployment;
2. use additive changes first;
3. deploy indexes concurrently where operationally appropriate;
4. RLS enablement and policies ship in the same migration as client access;
5. destructive changes require a compatibility window;
6. data backfills are bounded, observable, and resumable;
7. functions and triggers are versioned;
8. every migration is tested from the oldest supported remote baseline;
9. staging promotion precedes production;
10. rollback or forward-fix steps are documented.

## 18. Seed data

`seed.sql` may contain:

- synthetic users or documented test creation workflow;
- synthetic media identities;
- release catalog samples;
- development feature flags;
- deterministic recommendation events only for post-1.0 contract tests;
- no copyrighted media files;
- no real email addresses, notes, tokens, or credentials.

## 19. CI verification

The backend pipeline performs:

- SQL formatting/linting;
- clean local database creation;
- migration application;
- RLS test suite;
- generated type drift check;
- Edge Function type checking and tests;
- forbidden-secret scanning;
- schema diff review;
- integration tests using two or more identities;
- export/import verification;
- protocol compatibility tests.
- a public-1.0 build assertion that no personal synchronization endpoint is reachable;
- metadata language-fallback and episode-release combination tests.

## 20. Operational maintenance

Scheduled maintenance may:

- remove expired recommendation candidates;
- compact old acknowledged idempotency records after the retry window;
- purge tombstones only after the supported offline period;
- rotate short-lived support bundles;
- monitor change-log retention;
- identify failed function executions;
- validate policy coverage on all user tables.

Maintenance never silently removes ratings, notes, collections, playlists, or active playback summaries.

## 21. Acceptance criteria

- A clean local Supabase environment can be created from Git.
- Two users cannot access each other’s rows through any supported client path.
- Retried mutations do not duplicate logical records.
- Composite foreign keys reject cross-user references even when the referenced UUID exists.
- Favorite state changes without changing the star rating, and rating changes without changing favorite state.
- Notes reject more than 10,000 characters and retain conflicting versions.
- A delta cursor can resume after process termination.
- A stale cursor enters deterministic reconciliation.
- Service-role credentials are absent from the desktop artifact.
- Edge Functions keep provider and AI secrets server-side.
- Public 1.0 can obtain metadata and episode-release state without creating a product account or writing personal rows.
- Public 1.0 never uploads ratings, favorites, notes, viewing history, collections, playlists, or preferences.
- Development and test environments provision one synthetic user automatically.
- Cloud export uses provider-neutral MediaHub identifiers.
- Stopping all Supabase services does not prevent local playback or editing.

---

## Document control

This document is part of the MediaHub revised baseline specification v1.2. Remote schema, RLS, function, and protocol changes require migration review, security tests, and compatibility documentation.
