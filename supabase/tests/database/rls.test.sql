begin;

create extension if not exists pgtap with schema extensions;
set local search_path = public, extensions;

select plan(6);

insert into auth.users(id, aud, role, email, encrypted_password, created_at, updated_at)
values
    ('10000000-0000-0000-0000-000000000001', 'authenticated', 'authenticated', 'user-a@example.invalid', '', now(), now()),
    ('20000000-0000-0000-0000-000000000002', 'authenticated', 'authenticated', 'user-b@example.invalid', '', now(), now());

insert into public.devices(id, user_id, device_name, platform, app_version, protocol_version)
values
    ('10000000-0000-0000-0000-000000000011', '10000000-0000-0000-0000-000000000001', 'Synthetic A', 'windows', '0.0.0', 1),
    ('20000000-0000-0000-0000-000000000022', '20000000-0000-0000-0000-000000000002', 'Synthetic B', 'windows', '0.0.0', 1);

insert into public.media_identities(id, user_id, canonical_key, media_type, display_title)
values
    ('10000000-0000-0000-0000-000000000101', '10000000-0000-0000-0000-000000000001', 'synthetic:a', 'movie', 'Synthetic A'),
    ('20000000-0000-0000-0000-000000000202', '20000000-0000-0000-0000-000000000002', 'synthetic:b', 'movie', 'Synthetic B');

insert into public.ratings(
    id, user_id, media_identity_id, stars, updated_by_device_id, idempotency_key)
values (
    '20000000-0000-0000-0000-000000000212',
    '20000000-0000-0000-0000-000000000002',
    '20000000-0000-0000-0000-000000000202',
    5,
    '20000000-0000-0000-0000-000000000022',
    '20000000-0000-0000-0000-000000000213');

set local role authenticated;
select set_config('request.jwt.claim.sub', '10000000-0000-0000-0000-000000000001', true);
select set_config('request.jwt.claim.role', 'authenticated', true);

select is(
    (select count(*)::integer from public.ratings),
    0,
    'A user cannot select another user rating');

select lives_ok(
    $$insert into public.ratings(
        id, user_id, media_identity_id, stars, updated_by_device_id, idempotency_key)
      values (
        '10000000-0000-0000-0000-000000000112',
        '10000000-0000-0000-0000-000000000001',
        '10000000-0000-0000-0000-000000000101',
        4,
        '10000000-0000-0000-0000-000000000011',
        '10000000-0000-0000-0000-000000000113')$$,
    'A user can insert an owned rating');

select throws_ok(
    $$insert into public.ratings(
        id, user_id, media_identity_id, stars, updated_by_device_id, idempotency_key)
      values (
        '10000000-0000-0000-0000-000000000122',
        '20000000-0000-0000-0000-000000000002',
        '20000000-0000-0000-0000-000000000202',
        4,
        '20000000-0000-0000-0000-000000000022',
        '10000000-0000-0000-0000-000000000123')$$,
    '42501',
    null,
    'A user cannot insert a row owned by another user');

select throws_ok(
    $$insert into public.ratings(
        id, user_id, media_identity_id, stars, updated_by_device_id, idempotency_key)
      values (
        '10000000-0000-0000-0000-000000000132',
        '10000000-0000-0000-0000-000000000001',
        '20000000-0000-0000-0000-000000000202',
        4,
        '10000000-0000-0000-0000-000000000011',
        '10000000-0000-0000-0000-000000000133')$$,
    '23503',
    null,
    'Composite ownership foreign keys reject cross-user references');

select throws_ok(
    $$insert into public.notes(
        id, user_id, media_identity_id, note_text, updated_by_device_id, idempotency_key)
      values (
        '10000000-0000-0000-0000-000000000142',
        '10000000-0000-0000-0000-000000000001',
        '10000000-0000-0000-0000-000000000101',
        repeat('x', 10001),
        '10000000-0000-0000-0000-000000000011',
        '10000000-0000-0000-0000-000000000143')$$,
    '23514',
    null,
    'Notes reject more than 10000 characters');

select lives_ok(
    $$insert into public.favorites(
        id, user_id, media_identity_id, is_favorite, updated_by_device_id, idempotency_key)
      values (
        '10000000-0000-0000-0000-000000000152',
        '10000000-0000-0000-0000-000000000001',
        '10000000-0000-0000-0000-000000000101',
        true,
        '10000000-0000-0000-0000-000000000011',
        '10000000-0000-0000-0000-000000000153')$$,
    'Favorite state persists independently from rating');

select * from finish();
rollback;
