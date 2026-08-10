insert into public.feature_flags(key, enabled, environment, payload)
values
    ('personal-synchronization', false, 'development', '{"release":"post-1.0"}'::jsonb),
    ('ai-recommendations', false, 'development', '{"release":"post-1.0"}'::jsonb),
    ('online-subtitle-providers', false, 'development', '{"release":"post-1.0"}'::jsonb),
    ('external-plugin-installation', false, 'development', '{"release":"post-1.0"}'::jsonb)
on conflict (key) do update
set enabled = excluded.enabled,
    environment = excluded.environment,
    payload = excluded.payload,
    updated_at = now();

insert into public.application_releases(
    id,
    version,
    channel,
    published_at,
    minimum_protocol,
    maximum_protocol,
    changelog,
    is_active)
values (
    '00000000-0000-0000-0000-000000000001',
    '0.0.0',
    'developer',
    '2026-08-10T00:00:00Z',
    1,
    1,
    '{"hu":{"added":["Fejlesztési alapverzió"]},"en":{"added":["Development foundation"]}}'::jsonb,
    true)
on conflict (version) do nothing;

insert into public.metadata_cache(
    provider_key,
    media_type,
    external_id,
    locale,
    payload,
    attribution,
    fetched_at,
    expires_at)
values (
    'synthetic',
    'movie',
    'sample-movie-1',
    'hu-HU',
    '{"title":"Példafilm","overview":"Szintetikus fejlesztési metaadat.","year":2026,"genres":["drama"]}'::jsonb,
    '{"provider":"MediaHub synthetic seed"}'::jsonb,
    now(),
    now() + interval '30 days')
on conflict (provider_key, media_type, external_id, locale) do nothing;

insert into public.episode_release_catalog(
    provider_key,
    series_external_id,
    season_number,
    episode_number,
    region_code,
    official_air_date,
    provider_available,
    provider_available_at)
values ('synthetic', 'sample-series-1', 1, 1, 'HU', current_date, true, now())
on conflict (provider_key, series_external_id, season_number, episode_number, region_code) do nothing;
