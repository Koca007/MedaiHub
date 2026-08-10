# Volume 09 — Metadata, Artwork and Subtitles

**Project:** MediaHub  
**Document set:** MediaHub Project Bible  
**Status:** Revised baseline specification v1.2  
**Language:** English  
**Target platform:** Windows desktop first  
**Last updated:** 2026-08-08  

---

## 1. Metadata philosophy

Metadata enriches the library but must not own the user's identity, rating, note, favorite, progress, or organization. Version 1.0 includes at least one approved built-in remote metadata implementation behind provider-neutral contracts. Local-only records, sidecars, cached metadata, and generated artwork remain complete offline fallbacks. Provider data is cached, attributed where required, and replaceable.

## 2. Metadata sources

Priority order:

1. user edits;
2. local sidecar metadata;
3. embedded tags;
4. previously accepted provider match;
5. remote metadata provider;
6. filename and folder-derived values.

User edits are never overwritten by automatic refresh. Each field stores provenance.

## 3. Field provenance

Every enrichable field may record:

- source type;
- provider ID;
- external entity ID;
- fetched timestamp;
- confidence;
- user-locked flag;
- original language;
- last modified timestamp.

This allows selective refresh and conflict resolution.

## 4. Local metadata

Supported baseline inputs may include:

- `.nfo` files;
- folder and filename conventions;
- embedded title tags;
- poster and backdrop files;
- local episode thumbnails;
- custom MediaHub JSON sidecar.

MediaHub-specific sidecar example:

```json
{
  "schemaVersion": 1,
  "type": "movie",
  "title": "Example Movie",
  "year": 2025,
  "edition": "Director's Cut",
  "externalIds": {
    "example-provider": "12345"
  }
}
```

Sidecars are untrusted input and must be schema-validated.

## 5. Provider contracts

Metadata providers support capabilities independently:

- search movies;
- search series;
- get movie;
- get series;
- get seasons and episodes;
- get people;
- get collection/franchise;
- get images;
- get release dates;
- get content ratings.

A provider may implement only a subset.

The built-in 1.0 provider selection is finalized through an ADR after terms, attribution, Hungarian localization coverage, episode air-date quality, rate limits, and operational reliability are verified. The domain and application contracts do not depend on that provider choice.

## 6. Matching

Remote search input uses:

- normalized title;
- year;
- media type;
- runtime;
- season/episode evidence;
- original language where known.

Match results show provider, candidate title, year, overview, artwork, and confidence. Automatic matching requires configured confidence and must not override a prior user decision.

## 7. Refresh policy

Metadata refresh types:

- missing fields only;
- standard refresh;
- force refresh;
- artwork only;
- episode availability refresh.

Remote calls use cache, rate limits, retry with jitter, provider terms, and cancellation. Provider errors do not erase cached metadata.

Episode schedule refresh runs after the shell becomes interactive at startup and no more than once per 24 hours. A new-episode notification requires both an official air date that has arrived and a provider state indicating that the episode is released or available. Local media-file availability is tracked independently.

## 8. Artwork model

Artwork types:

- poster;
- backdrop;
- logo;
- thumbnail;
- season poster;
- episode still;
- person image;
- collection artwork;
- resume thumbnail;
- user screenshot.

Artwork assets store:

- source;
- original URL where permitted;
- local cache path;
- dimensions;
- format;
- content hash;
- dominant color;
- blur placeholder;
- language;
- provider attribution;
- crop metadata.

## 9. Generated artwork fallback

When no poster or usable artwork exists, MediaHub generates a representative frame from the video.

Frame selection strategy:

1. sample candidate frames after the opening segment;
2. exclude nearly black frames;
3. exclude extreme low-contrast frames;
4. avoid frames near credits where detectable;
5. prefer a sharp, visually varied frame;
6. store source timestamp.

For series, generated episode frames may be combined with a typographic title card. Generated assets are clearly marked and can be regenerated.

## 10. Artwork processing

- decode in a sandboxed or constrained pipeline where practical;
- limit source dimensions and decoded memory;
- create size variants;
- preserve aspect ratio;
- crop only with recorded transform;
- cache by content hash;
- purge unused derivatives while retaining original provenance;
- use WebP, JPEG, PNG, or platform-appropriate formats according to transparency and quality.

## 11. Subtitle discovery

Subtitle sources:

- embedded;
- sidecar in media folder;
- configured subtitle directories;
- provider download;
- manual import.

Sidecar matching uses basename, language tags, forced/SDH tags, and episode identity.

Examples:

```text
Movie.Title.2025.hu.srt
Series.S01E02.en.forced.ass
EpisodeName.HearingImpaired.vtt
```

## 12. Post-1.0 subtitle download workflow

Provider-based subtitle search and download are not public 1.0 features. Version 1.0 supports embedded subtitles, local sidecars, configured local subtitle directories, and manual import. The following workflow becomes active in the online-subtitle milestone:

1. User opens subtitle search or automatic background discovery runs if enabled.
2. MediaHub sends only required media identity and optional fingerprint to configured providers.
3. Results display language, source, hearing-impaired/forced status, rating, sync method, and provider.
4. Downloaded file is validated, sanitized, and stored in managed subtitle cache or beside media according to setting.
5. Subtitle is added to available tracks.
6. It remains disabled until the user explicitly enables it.

The post-1.0 requirement is download capability without automatic activation.

## 13. Subtitle security

- reject executable or archive payloads unless a provider adapter safely extracts allowed files;
- maximum size;
- encoding validation;
- path sanitization;
- no script execution;
- HTML stripped from display;
- ASS/SSA features are handled by the playback engine with documented limitations;
- provider archives are treated as untrusted.

## 14. Subtitle synchronization

Initial release:

- manual delay adjustment;
- save delay per file;
- reset to zero.

Future:

- waveform or fingerprint-based synchronization;
- community sync scores;
- automatic offset suggestions.

## 15. Metadata editing

User can edit:

- title;
- sort title;
- year;
- overview;
- genres;
- tags;
- artwork selection;
- edition label;
- series/season/episode assignment;
- people associations where enabled.

Edits are validated and recorded. “Restore provider value” is available for fields with provenance.

## 16. Language behavior

Metadata has:

- original language;
- display language;
- fallback language chain;
- provider-specific availability.

The UI locale and metadata language are independent. Version 1.0 uses the fixed metadata fallback order `hu-HU → original language → en`.

## 17. Cache lifecycle

- HTTP cache respects provider policy.
- Metadata payload cache has expiry.
- Artwork cache is content-addressed.
- User-selected artwork is pinned.
- Cache cleanup never removes the only local copy of user-created screenshots.
- Offline mode serves stale cache with a stale indicator only where useful.

## 18. Acceptance criteria

- A local-only title remains fully playable without a provider.
- User-edited title is not overwritten by refresh.
- Missing artwork produces a generated frame.
- Downloaded subtitle is listed but not automatically active.
- Provider outage leaves previous metadata visible.
- Cache cleanup preserves selected and user-created assets.
- Ambiguous remote match requires user review.
- Hungarian metadata is preferred, followed by original-language metadata and then English.
- A released episode notification can exist without a local file, and local availability is shown separately.
- Version 1.0 does not expose online subtitle-provider search or download.

---

## Document control

This document is part of the MediaHub revised baseline specification v1.2. Changes that affect architecture, security, data compatibility, plugin contracts, or user-visible behavior must be recorded in the Architecture Decision Record volume and reflected in the traceability matrix.
