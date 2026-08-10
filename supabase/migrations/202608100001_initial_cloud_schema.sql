begin;

create extension if not exists pgcrypto with schema extensions;

create or replace function public.set_updated_at()
returns trigger
language plpgsql
security invoker
set search_path = ''
as $$
begin
    new.updated_at = now();
    return new;
end;
$$;

create or replace function public.advance_revision()
returns trigger
language plpgsql
security invoker
set search_path = ''
as $$
begin
    new.updated_at = now();
    new.revision = old.revision + 1;
    return new;
end;
$$;

create table public.profiles (
    user_id uuid primary key references auth.users(id) on delete cascade,
    display_name text null check (display_name is null or char_length(display_name) between 1 and 100),
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now()
);

create table public.devices (
    id uuid primary key,
    user_id uuid not null references auth.users(id) on delete cascade,
    device_name text not null check (char_length(device_name) between 1 and 100),
    platform text not null check (char_length(platform) between 1 and 50),
    app_version text not null check (char_length(app_version) between 1 and 50),
    protocol_version integer not null check (protocol_version > 0),
    last_seen_at timestamptz not null default now(),
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now(),
    unique (user_id, id)
);

create table public.media_identities (
    id uuid primary key,
    user_id uuid not null references auth.users(id) on delete cascade,
    canonical_key text not null check (char_length(canonical_key) between 1 and 500),
    media_type text not null check (media_type in ('movie', 'series', 'episode')),
    display_title text not null check (char_length(display_title) between 1 and 500),
    release_year integer null check (release_year between 1870 and 2200),
    series_id uuid null,
    season_number integer null check (season_number is null or season_number >= 0),
    episode_number integer null check (episode_number is null or episode_number > 0),
    external_ids jsonb not null default '{}'::jsonb check (jsonb_typeof(external_ids) = 'object'),
    identity_fingerprint text null check (identity_fingerprint is null or char_length(identity_fingerprint) <= 500),
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now(),
    revision bigint not null default 1 check (revision > 0),
    unique (user_id, id),
    unique (user_id, canonical_key),
    foreign key (user_id, series_id) references public.media_identities(user_id, id),
    check (
        (media_type = 'episode' and series_id is not null and season_number is not null and episode_number is not null)
        or (media_type <> 'episode' and series_id is null and season_number is null and episode_number is null)
    )
);

create table public.ratings (
    id uuid primary key,
    user_id uuid not null references auth.users(id) on delete cascade,
    media_identity_id uuid not null,
    stars smallint not null check (stars between 1 and 5),
    updated_by_device_id uuid not null,
    idempotency_key uuid not null,
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now(),
    revision bigint not null default 1 check (revision > 0),
    unique (user_id, id),
    unique (user_id, media_identity_id),
    unique (user_id, idempotency_key),
    foreign key (user_id, media_identity_id) references public.media_identities(user_id, id),
    foreign key (user_id, updated_by_device_id) references public.devices(user_id, id)
);

create table public.favorites (
    id uuid primary key,
    user_id uuid not null references auth.users(id) on delete cascade,
    media_identity_id uuid not null,
    is_favorite boolean not null,
    updated_by_device_id uuid not null,
    idempotency_key uuid not null,
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now(),
    revision bigint not null default 1 check (revision > 0),
    unique (user_id, id),
    unique (user_id, media_identity_id),
    unique (user_id, idempotency_key),
    foreign key (user_id, media_identity_id) references public.media_identities(user_id, id),
    foreign key (user_id, updated_by_device_id) references public.devices(user_id, id)
);

create table public.notes (
    id uuid primary key,
    user_id uuid not null references auth.users(id) on delete cascade,
    media_identity_id uuid not null,
    note_text text not null check (char_length(note_text) <= 10000),
    updated_by_device_id uuid not null,
    idempotency_key uuid not null,
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now(),
    revision bigint not null default 1 check (revision > 0),
    unique (user_id, id),
    unique (user_id, media_identity_id),
    unique (user_id, idempotency_key),
    foreign key (user_id, media_identity_id) references public.media_identities(user_id, id),
    foreign key (user_id, updated_by_device_id) references public.devices(user_id, id)
);

create table public.note_versions (
    id uuid primary key,
    user_id uuid not null references auth.users(id) on delete cascade,
    note_id uuid not null,
    note_text text not null check (char_length(note_text) <= 10000),
    source_device_id uuid not null,
    base_revision bigint not null check (base_revision > 0),
    conflict_state text not null check (conflict_state in ('accepted', 'conflict', 'resolved')),
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now(),
    unique (user_id, id),
    foreign key (user_id, note_id) references public.notes(user_id, id) on delete cascade,
    foreign key (user_id, source_device_id) references public.devices(user_id, id)
);

create table public.collections (
    id uuid primary key,
    user_id uuid not null references auth.users(id) on delete cascade,
    name text not null check (char_length(name) between 1 and 200),
    description text null check (description is null or char_length(description) <= 2000),
    kind text not null check (kind in ('manual', 'automatic')),
    rule_payload jsonb null,
    sort_order integer not null default 0,
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now(),
    revision bigint not null default 1 check (revision > 0),
    deleted_at timestamptz null,
    unique (user_id, id),
    check (
        (kind = 'manual' and rule_payload is null)
        or (kind = 'automatic' and jsonb_typeof(rule_payload) = 'object')
    )
);

create table public.collection_items (
    id uuid primary key,
    user_id uuid not null references auth.users(id) on delete cascade,
    collection_id uuid not null,
    media_identity_id uuid not null,
    added_at timestamptz not null default now(),
    updated_at timestamptz not null default now(),
    deleted_at timestamptz null,
    revision bigint not null default 1 check (revision > 0),
    unique (user_id, id),
    unique (user_id, collection_id, media_identity_id),
    foreign key (user_id, collection_id) references public.collections(user_id, id) on delete cascade,
    foreign key (user_id, media_identity_id) references public.media_identities(user_id, id)
);

create table public.playlists (
    id uuid primary key,
    user_id uuid not null references auth.users(id) on delete cascade,
    name text not null check (char_length(name) between 1 and 200),
    description text null check (description is null or char_length(description) <= 2000),
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now(),
    revision bigint not null default 1 check (revision > 0),
    deleted_at timestamptz null,
    unique (user_id, id)
);

create table public.playlist_items (
    id uuid primary key,
    user_id uuid not null references auth.users(id) on delete cascade,
    playlist_id uuid not null,
    media_identity_id uuid not null,
    position_key text not null check (char_length(position_key) between 1 and 200),
    added_at timestamptz not null default now(),
    updated_at timestamptz not null default now(),
    deleted_at timestamptz null,
    revision bigint not null default 1 check (revision > 0),
    unique (user_id, id),
    unique (user_id, playlist_id, position_key),
    foreign key (user_id, playlist_id) references public.playlists(user_id, id) on delete cascade,
    foreign key (user_id, media_identity_id) references public.media_identities(user_id, id)
);

create table public.playback_summaries (
    id uuid primary key,
    user_id uuid not null references auth.users(id) on delete cascade,
    media_identity_id uuid not null,
    position_seconds integer not null default 0 check (position_seconds >= 0),
    duration_seconds integer not null default 0 check (duration_seconds >= 0),
    completion_state text not null check (completion_state in ('not_started', 'in_progress', 'completed', 'unwatched')),
    semantic_action text null check (semantic_action is null or semantic_action in ('restart', 'mark_unwatched', 'mark_watched')),
    last_played_at timestamptz null,
    updated_by_device_id uuid not null,
    idempotency_key uuid not null,
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now(),
    revision bigint not null default 1 check (revision > 0),
    unique (user_id, id),
    unique (user_id, media_identity_id),
    unique (user_id, idempotency_key),
    foreign key (user_id, media_identity_id) references public.media_identities(user_id, id),
    foreign key (user_id, updated_by_device_id) references public.devices(user_id, id),
    check (position_seconds <= duration_seconds or duration_seconds = 0)
);

create table public.user_preferences (
    user_id uuid primary key references auth.users(id) on delete cascade,
    payload jsonb not null default '{}'::jsonb check (jsonb_typeof(payload) = 'object'),
    schema_version integer not null check (schema_version > 0),
    updated_by_device_id uuid not null,
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now(),
    revision bigint not null default 1 check (revision > 0),
    foreign key (user_id, updated_by_device_id) references public.devices(user_id, id),
    check (octet_length(payload::text) <= 32768)
);

create table public.recommendation_events (
    id uuid primary key,
    user_id uuid not null references auth.users(id) on delete cascade,
    media_identity_id uuid null,
    event_type text not null check (event_type in ('genre_viewed', 'genre_completed', 'quality_selected', 'rated', 'shown', 'opened', 'dismissed')),
    genre_keys text[] not null default '{}',
    quality_class text null check (quality_class is null or char_length(quality_class) <= 50),
    rating smallint null check (rating is null or rating between 1 and 5),
    watch_fraction double precision null check (watch_fraction is null or watch_fraction between 0 and 1),
    occurred_at timestamptz not null,
    device_id uuid not null,
    idempotency_key uuid not null,
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now(),
    revision bigint not null default 1 check (revision > 0),
    unique (user_id, id),
    unique (user_id, idempotency_key),
    foreign key (user_id, media_identity_id) references public.media_identities(user_id, id),
    foreign key (user_id, device_id) references public.devices(user_id, id)
);

create table public.recommendation_candidates (
    id uuid primary key,
    user_id uuid not null references auth.users(id) on delete cascade,
    media_identity_id uuid not null,
    score double precision not null,
    explanation_code text not null check (char_length(explanation_code) between 1 and 100),
    explanation_parameters jsonb not null default '{}'::jsonb check (jsonb_typeof(explanation_parameters) = 'object'),
    algorithm_version text not null check (char_length(algorithm_version) between 1 and 50),
    generated_at timestamptz not null default now(),
    expires_at timestamptz not null,
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now(),
    revision bigint not null default 1 check (revision > 0),
    unique (user_id, id),
    foreign key (user_id, media_identity_id) references public.media_identities(user_id, id),
    check (expires_at > generated_at)
);

create table public.application_releases (
    id uuid primary key default gen_random_uuid(),
    version text not null unique check (char_length(version) between 1 and 50),
    channel text not null check (channel in ('stable', 'beta', 'developer')),
    published_at timestamptz not null,
    minimum_protocol integer not null check (minimum_protocol > 0),
    maximum_protocol integer null check (maximum_protocol is null or maximum_protocol >= minimum_protocol),
    changelog jsonb not null check (jsonb_typeof(changelog) = 'object'),
    download_reference text null,
    is_active boolean not null default true,
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now()
);

create table public.feature_flags (
    key text primary key check (char_length(key) between 1 and 100),
    enabled boolean not null,
    minimum_app_version text null,
    maximum_app_version text null,
    environment text not null check (environment in ('local', 'development', 'staging', 'production')),
    payload jsonb not null default '{}'::jsonb check (jsonb_typeof(payload) = 'object'),
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now()
);

create table public.metadata_cache (
    provider_key text not null,
    media_type text not null check (media_type in ('movie', 'series', 'season', 'episode', 'person')),
    external_id text not null,
    locale text not null,
    payload jsonb not null check (jsonb_typeof(payload) = 'object'),
    attribution jsonb not null default '{}'::jsonb check (jsonb_typeof(attribution) = 'object'),
    fetched_at timestamptz not null default now(),
    expires_at timestamptz not null,
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now(),
    primary key (provider_key, media_type, external_id, locale),
    check (expires_at > fetched_at)
);

create table public.episode_release_catalog (
    provider_key text not null,
    series_external_id text not null,
    season_number integer not null check (season_number >= 0),
    episode_number integer not null check (episode_number > 0),
    region_code text not null check (char_length(region_code) between 2 and 10),
    official_air_date date null,
    provider_available boolean not null default false,
    provider_available_at timestamptz null,
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now(),
    primary key (provider_key, series_external_id, season_number, episode_number, region_code),
    check (provider_available or provider_available_at is null)
);

create table public.user_change_log (
    sequence bigint generated always as identity primary key,
    user_id uuid not null references auth.users(id) on delete cascade,
    entity_type text not null,
    entity_id uuid not null,
    operation text not null check (operation in ('insert', 'update', 'delete')),
    revision bigint not null check (revision > 0),
    changed_at timestamptz not null default now()
);

create index media_identities_user_updated_idx on public.media_identities(user_id, updated_at);
create index collection_items_user_updated_idx on public.collection_items(user_id, updated_at);
create index playlist_items_user_updated_idx on public.playlist_items(user_id, updated_at);
create index recommendation_candidates_user_expiry_idx on public.recommendation_candidates(user_id, expires_at);
create index user_change_log_user_sequence_idx on public.user_change_log(user_id, sequence);
create index metadata_cache_expiry_idx on public.metadata_cache(expires_at);
create index episode_release_catalog_air_date_idx on public.episode_release_catalog(official_air_date);

create or replace function public.append_user_change()
returns trigger
language plpgsql
security definer
set search_path = ''
as $$
declare
    owner_id uuid;
    row_id uuid;
    row_revision bigint;
begin
    if tg_op = 'DELETE' then
        owner_id := old.user_id;
        row_id := old.id;
        row_revision := old.revision;
    else
        owner_id := new.user_id;
        row_id := new.id;
        row_revision := new.revision;
    end if;

    insert into public.user_change_log(user_id, entity_type, entity_id, operation, revision)
    values (owner_id, tg_table_name, row_id, lower(tg_op), row_revision);

    if tg_op = 'DELETE' then
        return old;
    end if;

    return new;
end;
$$;

create trigger profiles_set_updated_at before update on public.profiles
for each row execute function public.set_updated_at();
create trigger devices_set_updated_at before update on public.devices
for each row execute function public.set_updated_at();
create trigger note_versions_set_updated_at before update on public.note_versions
for each row execute function public.set_updated_at();
create trigger application_releases_set_updated_at before update on public.application_releases
for each row execute function public.set_updated_at();
create trigger feature_flags_set_updated_at before update on public.feature_flags
for each row execute function public.set_updated_at();
create trigger metadata_cache_set_updated_at before update on public.metadata_cache
for each row execute function public.set_updated_at();
create trigger episode_release_catalog_set_updated_at before update on public.episode_release_catalog
for each row execute function public.set_updated_at();

create trigger media_identities_advance_revision before update on public.media_identities
for each row execute function public.advance_revision();
create trigger ratings_advance_revision before update on public.ratings
for each row execute function public.advance_revision();
create trigger favorites_advance_revision before update on public.favorites
for each row execute function public.advance_revision();
create trigger notes_advance_revision before update on public.notes
for each row execute function public.advance_revision();
create trigger collections_advance_revision before update on public.collections
for each row execute function public.advance_revision();
create trigger collection_items_advance_revision before update on public.collection_items
for each row execute function public.advance_revision();
create trigger playlists_advance_revision before update on public.playlists
for each row execute function public.advance_revision();
create trigger playlist_items_advance_revision before update on public.playlist_items
for each row execute function public.advance_revision();
create trigger playback_summaries_advance_revision before update on public.playback_summaries
for each row execute function public.advance_revision();
create trigger user_preferences_advance_revision before update on public.user_preferences
for each row execute function public.advance_revision();
create trigger recommendation_events_advance_revision before update on public.recommendation_events
for each row execute function public.advance_revision();
create trigger recommendation_candidates_advance_revision before update on public.recommendation_candidates
for each row execute function public.advance_revision();

create trigger media_identities_change after insert or update or delete on public.media_identities
for each row execute function public.append_user_change();
create trigger ratings_change after insert or update or delete on public.ratings
for each row execute function public.append_user_change();
create trigger favorites_change after insert or update or delete on public.favorites
for each row execute function public.append_user_change();
create trigger notes_change after insert or update or delete on public.notes
for each row execute function public.append_user_change();
create trigger collections_change after insert or update or delete on public.collections
for each row execute function public.append_user_change();
create trigger collection_items_change after insert or update or delete on public.collection_items
for each row execute function public.append_user_change();
create trigger playlists_change after insert or update or delete on public.playlists
for each row execute function public.append_user_change();
create trigger playlist_items_change after insert or update or delete on public.playlist_items
for each row execute function public.append_user_change();
create trigger playback_summaries_change after insert or update or delete on public.playback_summaries
for each row execute function public.append_user_change();

alter table public.profiles enable row level security;
alter table public.devices enable row level security;
alter table public.media_identities enable row level security;
alter table public.ratings enable row level security;
alter table public.favorites enable row level security;
alter table public.notes enable row level security;
alter table public.note_versions enable row level security;
alter table public.collections enable row level security;
alter table public.collection_items enable row level security;
alter table public.playlists enable row level security;
alter table public.playlist_items enable row level security;
alter table public.playback_summaries enable row level security;
alter table public.user_preferences enable row level security;
alter table public.recommendation_events enable row level security;
alter table public.recommendation_candidates enable row level security;
alter table public.user_change_log enable row level security;
alter table public.application_releases enable row level security;
alter table public.feature_flags enable row level security;
alter table public.metadata_cache enable row level security;
alter table public.episode_release_catalog enable row level security;

create policy profiles_own on public.profiles for all to authenticated
using (user_id = (select auth.uid())) with check (user_id = (select auth.uid()));
create policy devices_own on public.devices for all to authenticated
using (user_id = (select auth.uid())) with check (user_id = (select auth.uid()));
create policy media_identities_own on public.media_identities for all to authenticated
using (user_id = (select auth.uid())) with check (user_id = (select auth.uid()));
create policy ratings_own on public.ratings for all to authenticated
using (user_id = (select auth.uid())) with check (user_id = (select auth.uid()));
create policy favorites_own on public.favorites for all to authenticated
using (user_id = (select auth.uid())) with check (user_id = (select auth.uid()));
create policy notes_own on public.notes for all to authenticated
using (user_id = (select auth.uid())) with check (user_id = (select auth.uid()));
create policy note_versions_own on public.note_versions for all to authenticated
using (user_id = (select auth.uid())) with check (user_id = (select auth.uid()));
create policy collections_own on public.collections for all to authenticated
using (user_id = (select auth.uid())) with check (user_id = (select auth.uid()));
create policy collection_items_own on public.collection_items for all to authenticated
using (user_id = (select auth.uid())) with check (user_id = (select auth.uid()));
create policy playlists_own on public.playlists for all to authenticated
using (user_id = (select auth.uid())) with check (user_id = (select auth.uid()));
create policy playlist_items_own on public.playlist_items for all to authenticated
using (user_id = (select auth.uid())) with check (user_id = (select auth.uid()));
create policy playback_summaries_own on public.playback_summaries for all to authenticated
using (user_id = (select auth.uid())) with check (user_id = (select auth.uid()));
create policy user_preferences_own on public.user_preferences for all to authenticated
using (user_id = (select auth.uid())) with check (user_id = (select auth.uid()));
create policy recommendation_events_own on public.recommendation_events for all to authenticated
using (user_id = (select auth.uid())) with check (user_id = (select auth.uid()));
create policy recommendation_candidates_select_own on public.recommendation_candidates for select to authenticated
using (user_id = (select auth.uid()));
create policy user_change_log_select_own on public.user_change_log for select to authenticated
using (user_id = (select auth.uid()));

revoke all on all tables in schema public from anon, authenticated;
grant usage on schema public to authenticated;
grant select, insert, update, delete on table
    public.profiles,
    public.devices,
    public.media_identities,
    public.ratings,
    public.favorites,
    public.notes,
    public.note_versions,
    public.collections,
    public.collection_items,
    public.playlists,
    public.playlist_items,
    public.playback_summaries,
    public.user_preferences,
    public.recommendation_events
to authenticated;
grant select on public.recommendation_candidates, public.user_change_log to authenticated;
grant usage, select on sequence public.user_change_log_sequence_seq to authenticated;

revoke all on function public.set_updated_at() from public, anon, authenticated;
revoke all on function public.advance_revision() from public, anon, authenticated;
revoke all on function public.append_user_change() from public, anon, authenticated;

commit;
