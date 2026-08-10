# Volume 10 — Watch Tracking, Collections, Playlists and Statistics

**Project:** MediaHub  
**Document set:** MediaHub Project Bible  
**Status:** Revised baseline specification v1.2  
**Language:** English  
**Target platform:** Windows desktop first  
**Last updated:** 2026-08-08  

---

## 1. Watch tracking model

MediaHub separates current resume state from historical viewing events.

- `PlaybackState` answers: Where should playback resume and is this item watched?
- `WatchEvent` answers: When and how long was it watched?
- `PlaybackSession` groups checkpoints from one opening of the player.

This avoids losing statistics when the current position changes.

## 2. Progress calculation

Movie progress:

```text
clamp(saved position / effective duration, 0, 1)
```

Episode progress uses the same calculation.

Season progress:

```text
completed canonical episodes / known released episodes
```

Series progress:

```text
completed canonical episodes / known released episodes
```

Unaired episodes are excluded. Specials are excluded by default from main percentage but may have a separate progress value. The user can include specials through settings.

Time-based series progress is optional and displayed as supplementary information.

## 3. Completion rules

- Default threshold: 90%.
- Natural playback end marks complete.
- Manual Mark Watched overrides threshold.
- Manual Mark Unwatched clears completion but may retain historical watch events.
- Rewatching a completed item can create another watch event without clearing watched state.
- Very short preview or accidental playback does not create a meaningful watch event until the minimum active-watch threshold.

## 4. Active watch time

Active watch time excludes:

- paused duration;
- time skipped through seeking;
- time while playback is stalled;
- playback after the application loses output where detectable.

At higher playback speed, statistics may store both wall-clock time and media-time consumed.

## 5. Continue Watching

An item appears when:

- it has meaningful incomplete progress;
- its file is reachable or the UI permits missing entries;
- it is not explicitly hidden.

Series are represented by the current episode rather than one card per episode. Sorting uses last played time.

An item leaves Continue Watching when:

- completed;
- progress cleared;
- manually removed from row;
- hidden by policy.

Removing from Continue Watching does not delete history.

## 6. Resume thumbnail

The player captures a frame near the checkpoint. To reduce spoilers and avoid unstable frames:

- capture shortly before or at saved position;
- avoid black frames;
- regenerate if edition changes;
- store timestamp and fingerprint;
- do not send the image externally.

## 7. Ratings

Baseline rating:

- one to five whole stars;
- optional;
- stored on movie or series title;
- clearable;
- used as a weighting signal only after the post-1.0 recommendation module is enabled;
- retained when media is missing.

The UI may allow rating from cards, details pages, and end-of-playback overlay.

## 8. Notes

One private note per title in the baseline release:

- plain text;
- variable-length compact storage with a 10,000-character safety limit;
- no HTML, Markdown, rich-text document model, or per-keystroke revision history;
- autosave with debounce;
- saved timestamp;
- optional Markdown support deferred;
- never included in provider requests;
- excluded from diagnostic logs.

## 9. Manual collections

A collection organizes titles, not arbitrary files.

Features:

- title;
- description;
- artwork;
- manual order;
- multiple membership;
- bulk add/remove;
- export;
- missing-item retention.

Example collections:

- Marvel Universe
- Christmas
- Watch With Friends
- Award Winners

## 10. Automatic collections

Automatic collections are saved rules evaluated against indexed data.

Rule syntax supports AND/OR groups and predicates such as:

```text
Genre contains "Science Fiction"
Director equals "Example Director"
Release year between 2000 and 2009
User rating >= 4
Availability = Available
Resolution >= 2160
Watched = false
```

Rules are stored as versioned JSON or a domain expression tree, not raw SQL.

Evaluation may be incremental when relevant fields change.

## 11. Playlists

Unlike collections, playlists reference playable items and have playback order.

Supported entries:

- movies;
- episodes;
- future trailers and extras.

Behavior settings:

- repeat none/all/one;
- shuffle;
- skip missing with notification;
- stop on unavailable item;
- autoplay next entry.

When a playback session originates from a playlist, the playlist sequence always supplies the next target. Canonical next episode, sequel, and franchise logic are ignored until the playlist session ends. Missing entries are skipped with a visible notification by default; a playlist may instead opt into Stop on unavailable.

One playable item may appear multiple times if the playlist allows duplicates.

## 12. Favorites

Favorites are a first-class explicit state. The UI presents a heart-style toggle, and changing it never changes the star rating. Favorites are retained for missing media, included in export, and added to synchronization when the public synchronization feature ships.

## 13. Statistics dashboard

Required metrics:

- total movies;
- total series;
- total episodes;
- available versus missing;
- total active watch time;
- weekly watch time;
- monthly watch time;
- yearly watch time;
- completed movies;
- completed series;
- favorite genre;
- favorite actor;
- favorite director;
- highest-rated genres;
- most watched series;
- current watch streak, optional;
- longest single session;
- most active day/month.

Statistics must explain methodology, especially for “favorite” calculations.

## 14. Favorite calculations

Examples:

- Favorite genre: weighted score using active watch time, completion, and rating.
- Favorite actor: titles watched and ratings, with minimum sample size.
- Favorite director: completed titles and ratings, with minimum sample size.

A single film must not make someone the favorite by default. The algorithm uses confidence thresholds and exposes “Not enough data” when appropriate.

Actor/director statistics are deterministic, local, on-demand summaries. They never become recommendation-profile fields and never cause ratings or watch history to weight a person affinity.

## 15. Privacy and reset

Statistics remain local. User can:

- export history;
- clear playback positions;
- clear watch events;
- clear ratings;
- clear notes;
- reset recommendation profile;
- purge all personal state separately from media indexing.

Each reset action previews impact.

Recommendation reset is shown only after the post-1.0 recommendation module is installed/enabled.

## 16. Missing media

When physical files are removed:

- rating remains;
- note remains;
- progress remains;
- watch events remain;
- collection membership remains;
- playlist entries remain;
- title displays Missing.

If a replacement file appears, historical state reconnects.

## 17. Version history and editions

MediaHub may track previous file versions:

- prior media file;
- quality;
- dates available;
- replacement relationship.

Version history supports understanding upgrades from 1080p to 4K without treating the title as newly watched. It does not retain deleted media bytes.

## 18. Acceptance criteria

- Watching half a movie creates Continue Watching.
- Natural end marks complete.
- A deleted file leaves rating and note intact.
- A title belongs to multiple collections.
- An automatic collection updates when rating or metadata changes.
- Playlist order is stable.
- Weekly/monthly/yearly totals exclude pause time.
- Favorite calculations do not claim a result below minimum sample size.


## 19. Synchronization behavior

In Development/Test builds, ratings, compact plain-text notes, favorites, collections, playlists, and summarized playback state participate in synchronization through an automatically provisioned synthetic identity. Public Windows 1.0 does not upload personal user state. When public synchronization ships, enabling it synchronizes every supported category automatically. Raw high-frequency player position events do not stream directly to Supabase. MediaHub writes throttled local checkpoints and publishes compact summaries through the outbox.

Statistics remain locally computable from local watch events. A future mobile client may use synchronized summaries, but the desktop client does not upload complete raw watch telemetry unless the user enables the corresponding cloud capability.

A title is identified remotely by stable MediaHub ID plus recognized external IDs when available. Local file paths and storage topology are excluded.

---

## Document control

This document is part of the MediaHub revised baseline specification v1.2. Changes that affect architecture, security, data compatibility, plugin contracts, or user-visible behavior must be recorded in the Architecture Decision Record volume and reflected in the traceability matrix.
