# Volume 25 — UI Component Catalog

**Project:** MediaHub  
**Document set:** MediaHub Project Bible  
**Status:** Revised baseline specification v1.2  
**Language:** English  
**Target platform:** Windows desktop first  
**Last updated:** 2026-08-08  

---

## 1. Purpose

This volume defines reusable presentation components so screens remain visually consistent, accessible, themeable, and testable. Components are rendered by MediaHub; plugins use declarative models rather than injecting arbitrary controls.

## 2. MediaCard

Variants:

- Poster card
- Backdrop card
- Episode card
- Continue Watching card
- Compact search result
- Missing media card

Common properties:

- title;
- subtitle;
- artwork;
- progress;
- watched state;
- availability;
- rating;
- favorite state;
- badges;
- primary action;
- context actions;
- accessible label.

States:

```text
Default
Hover
Keyboard focus
Pressed
Loading
Unavailable
Selected
Error
```

Rules:

- focus and hover are visually distinct;
- progress bar remains visible without relying only on color;
- missing state does not erase artwork or metadata;
- card action is deterministic;
- context menu is keyboard accessible;
- artwork uses lazy decoding and placeholder.

## 3. ContentRow

A horizontal virtualized collection.

Properties:

- heading;
- optional description;
- item source;
- see-all command;
- loading state;
- empty state;
- row type.

Behavior:

- focus enters at last remembered item or first item;
- Left/Right navigates;
- Up/Down moves between rows while preserving horizontal position where practical;
- row fetches more items near the end;
- virtualization must not lose selected or focused identity;
- screen reader announces row name and item count.

## 4. HeroBanner

Used for highlighted title or collection.

Elements:

- backdrop;
- logo/title;
- overview excerpt;
- metadata line;
- primary action;
- secondary action;
- progress;
- gradient scrim.

Rules:

- rotating hero content is disabled by default to avoid motion and unexpected changes;
- user input freezes any automatic transition;
- text remains contrast-safe;
- hero is not required for access to content.

## 5. ProgressBar

Types:

- playback progress;
- scan progress;
- download progress;
- indeterminate operation;
- series completion.

Required semantic values:

- minimum;
- maximum;
- current;
- text alternative;
- state.

A 50% series progress bar is announced as “31 of 62 episodes watched, 50 percent.”

## 6. StarRating

- one to five whole stars;
- mouse, keyboard, and touch-friendly;
- Left/Right adjusts;
- Delete or Clear action removes rating;
- current value announced;
- hover preview does not save until activation;
- note autosave status is separate.

## 7. NoteEditor

- plain text baseline;
- autosave after debounce;
- manual save on focus loss and close;
- character count near limit;
- 10,000-character maximum;
- plain text only, with no HTML/Markdown toolbar;
- private-data label;
- no provider or AI use notice;
- conflict resolution if two windows edit, though baseline avoids multiple editors.

## 8. EpisodeList

Columns or content:

- episode number;
- title;
- runtime;
- air date;
- state icon;
- progress;
- local quality;
- audio/subtitle summary;
- actions.

Compact and rich modes adapt to width. Season header shows progress and available/missing counts.

## 9. FilterChip

- active/inactive;
- removable;
- keyboard focus;
- clear-all;
- count where useful;
- not color-only.

Filter drawer supports saved filter presets in later versions.

## 10. SearchBox

- debounce local search;
- immediate keyboard navigation;
- Escape clears or closes according to state;
- search history local and clearable;
- provider search requires explicit context switch;
- no remote request from simple local typing.

## 11. JobIndicator

Shows background work without blocking.

States:

- queued;
- running;
- paused;
- completed;
- failed;
- needs attention.

Compact shell indicator opens Jobs panel. Multiple scan events aggregate.

## 12. NotificationCard

Fields:

- severity;
- title;
- message;
- time;
- actions;
- read state;
- source module.

Notifications must not use alarming language for recoverable expected network outages.

## 13. ConfirmationDialog

Required fields:

- clear action title;
- consequence;
- affected item list;
- retained data;
- warnings;
- primary/destructive button;
- Cancel default focus.

The destructive button label describes the operation, such as “Move 3 files to Recycle Bin,” not “OK.”

## 14. TrackSelector

Audio and subtitle selector displays:

- language;
- title;
- codec/format;
- channels;
- source;
- default/forced/SDH indicators;
- Off for subtitles.

Downloaded subtitles appear but are not selected automatically.

In public 1.0, the selector contains embedded, sidecar, and manually imported tracks. Provider-downloaded tracks appear only after the online-subtitle milestone.

## 15. TimelineControl

Features:

- seek;
- current/duration;
- chapter markers;
- buffered range;
- hover timestamp;
- keyboard increments;
- high-contrast mode;
- screen-reader value.

Future preview thumbnails must be throttled and not block seek.

## 16. ThemeEditor

Sections:

- base preset;
- colors;
- typography;
- cards;
- motion;
- player overlay;
- preview.

Includes reset per section and reset all. Invalid tokens are rejected before save.

## 17. LibraryRootCard

Displays:

- path;
- type;
- online state;
- content classification;
- monitoring mode;
- last scan;
- item count;
- errors;
- actions.

For credentials, display only account label or “Windows session,” never secret.

## 18. ReviewCandidateCard

Displays matching evidence, not just candidate title. User can view why confidence is low and create a durable rule.

## 19. DownloadRow

- title;
- provider/source;
- progress;
- speed;
- ETA;
- destination;
- state;
- pause/resume/cancel;
- failure details;
- quarantine warning.

## 20. FavoriteToggle

- explicit active/inactive heart state;
- independent from `StarRating`;
- keyboard and screen-reader operable;
- persists for missing media;
- never infers state from a five-star rating.

## 21. ScreenshotGallery

- title-specific captures;
- thumbnail, timestamp, creation date, and source-edition status;
- “Ugrás ehhez a jelenethez” action;
- fingerprint/edition validation before seek;
- safe fallback when the original file is missing or changed;
- keyboard navigation and accessible timestamp labels.

## 22. PictureInPictureWindow

- reuses the active playback session;
- always-on-top state is explicit;
- Play/Pause, seek, Next, return, and close actions;
- playlist-origin Next behavior remains unchanged;
- closing PiP does not create a false completion event.

## 23. Accessibility component checklist

Every component must define:

- automation name;
- role;
- state;
- value;
- keyboard interactions;
- focus behavior;
- high-contrast appearance;
- reduced-motion behavior;
- localization expansion;
- minimum target size.

## 24. Visual regression

Key components are rendered in:

- default dark;
- light;
- high contrast;
- pseudolocalized;
- 100%, 150%, and 200% scaling;
- compact and wide windows;
- loading/error/missing states.

Reference screenshots are version-controlled with an approved-update workflow.

The 1.0 reference set uses complete Hungarian resources. Pseudolocalization and English fallback remain development validation modes rather than advertised 1.0 locales.


## Revision 1.1 backend integration note

Cloud-connected status and actions must use the same local-first UX language: local content remains usable, synchronization state is non-blocking, errors are actionable, and no screen presents backend availability as equivalent to media-library availability. Plugin and UI contracts must consume backend-neutral services rather than Supabase SDK objects.

---

## Document control

This document is part of the MediaHub revised baseline specification v1.2. Changes that affect architecture, security, data compatibility, plugin contracts, or user-visible behavior must be recorded in the Architecture Decision Record volume and reflected in the traceability matrix.
