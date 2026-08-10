# Volume 06 — Desktop UI/UX Specification

**Project:** MediaHub  
**Document set:** MediaHub Project Bible  
**Status:** Revised baseline specification v1.2  
**Language:** English  
**Target platform:** Windows desktop first  
**Last updated:** 2026-08-08  

---

## 1. Design language

MediaHub uses an original cinematic interface with:

- dark-first visual presentation;
- large artwork and restrained chrome;
- horizontal content rows;
- clear typography;
- layered backdrops;
- subtle motion;
- strong keyboard support;
- user-configurable themes.

The supported 1.0 user-interface language is Hungarian. All UI remains resource-based so later language packs can be added without redesign.

The design must not copy protected assets, exact layouts, brand marks, or animations from another service.

## 2. Application shell

Persistent regions:

```text
┌──────────────────────────────────────────────────────────────┐
│ Top bar: Back | Search | Jobs | Notifications | Settings    │
├──────────────┬───────────────────────────────────────────────┤
│ Navigation   │ Main content                                  │
│ Home         │                                               │
│ Movies       │                                               │
│ Series       │                                               │
│ Collections  │                                               │
│ Playlists    │                                               │
│ Downloads    │                                               │
└──────────────┴───────────────────────────────────────────────┘
```

Navigation may collapse to icons. The player becomes a dedicated immersive surface and does not retain the full shell.

## 3. Home screen

Default rows:

1. Continue Watching
2. Recently Added
3. Next Episodes
4. Movies
5. Series
6. Favorites
7. Recently Rated
8. Missing Media, only when relevant

`Recommended For You` is added after 1.0 when the local recommendation module ships. Details pages may still show deterministic provider-based similar titles without creating a learned AI profile.

Row behavior:

- horizontal keyboard and wheel navigation;
- “See all” action;
- lazy artwork loading;
- card context menu;
- deterministic focus restoration;
- skeleton placeholders during loading.

Continue Watching card displays:

- title;
- episode context;
- progress bar;
- remaining time;
- resume thumbnail where available;
- last watched relative time;
- Resume action.

## 4. Movie details

Visual regions:

- backdrop;
- poster;
- title and release year;
- metadata summary;
- primary playback action;
- availability and preferred file;
- five-star rating;
- Favorite toggle independent from rating;
- private note;
- technical versions;
- collections and playlists;
- cast and director;
- similar titles;
- history and statistics;
- missing-media status when applicable.

Primary action logic:

- Play if never started;
- Resume if progress is meaningful;
- Play Again if completed;
- Locate File if no file is reachable.

## 5. Series details

Required elements:

- series artwork and overview;
- overall percentage;
- watched/known episodes;
- season selector;
- episode rows;
- next episode;
- newly known episode indicator;
- missing local episode indicator;
- series rating and note;
- automatic acquisition setting only after the authorized-acquisition milestone;
- remote release state and local-file availability as separate indicators;

Episode row states:

- Unwatched
- In progress
- Watched
- Missing
- Not aired
- Newly available
- Review needed

## 6. Search

Search opens from the shell and may use a command palette style.

Features:

- instant local results;
- grouped result types;
- filter chips;
- keyboard selection;
- recent searches stored locally;
- optional provider search tab;
- clear indication of local versus external result;
- typo tolerance and aliases;
- no search request sent externally until the user invokes an external provider context.

## 7. Library management

Settings page provides:

- list of roots;
- status and reachability;
- content type;
- last scan;
- watch mode;
- exclusions;
- rescan;
- remove root;
- credential handling;
- file count and storage summary.

Removing a root must clarify whether logical records remain. Default behavior retains history and marks items missing.

## 8. Review queue

Ambiguous items appear in a dedicated workflow:

```text
Filename
Detected attributes
Current suggested match
Alternative candidates
Preview frame
Folder context
[Accept] [Choose another] [Create local title] [Ignore]
```

Bulk actions are supported only when filenames share a safe rule. The UI previews the resulting mappings before applying.

## 9. Collections

Collection page supports:

- hero artwork;
- description;
- manual or automatic label;
- item count;
- custom ordering;
- grid/list toggle;
- edit rules for automatic collections;
- add/remove without deleting media.

Automatic collection rules can use:

- genre;
- director;
- actor;
- year range;
- rating;
- watched state;
- quality;
- language;
- tags;
- title franchise metadata.

## 10. Playlists

Playlist editor supports:

- mixed movie and episode entries;
- drag-and-drop;
- keyboard reordering;
- missing-item indicators;
- playback behavior;
- duplicate-entry policy;
- optional shuffle.

## 11. Player surface

Cinematic behavior:

- video fills the available viewport while respecting aspect ratio;
- controls fade after inactivity;
- cursor hides with controls;
- mouse movement or key input restores controls;
- top overlay shows title and episode context;
- bottom overlay shows timeline, time, volume, tracks, settings, next episode, fullscreen;
- overlays use contrast-safe background treatment.

Default controls:

| Input | Action |
|---|---|
| Space | Play/Pause |
| Left | Seek -10 seconds |
| Right | Seek +10 seconds |
| Up/Down | Volume step, optional |
| Mouse wheel | Volume |
| Double-click | Toggle fullscreen |
| Esc | Exit fullscreen or close overlay |
| M | Mute |
| F | Fullscreen |
| S | Subtitle menu |
| A | Audio track menu |
| Ctrl+S | Capture screenshot |

Shortcuts are customizable in a later milestone; baseline actions must be documented and conflict-free.

Manual captures open in a title-specific gallery. Each capture exposes its media timestamp and a Hungarian “Ugrás ehhez a jelenethez” action when the original compatible edition remains available.

## 12. Mini player

Picture-in-picture is supported as a compact always-on-top window where the playback engine permits stable rendering.

Controls:

- play/pause;
- seek;
- close;
- return to full player;
- next episode.

Mini player must not create a second playback session.

Picture-in-Picture is a required 1.0 capability, subject to the selected engine passing the prototype stability gate.

## 13. Notifications

In-app notification center supports:

- new episodes;
- scan completed;
- ambiguous matches;
- disconnected library;
- update available;
- plugin disabled;
- download completed;
- database recovery.

Notifications have read state and optional actions. Windows toast notifications are opt-in by category.

New-episode checks run after startup and once per 24 hours. The in-app item is always retained; the Windows toast remains optional.

## 14. Empty states

Examples:

- no library roots: “Add a folder to build your library.”
- no Continue Watching: show recently added or suggested local items;
- disconnected NAS: show reconnect and diagnostics;
- no metadata: show generated frame artwork and local file details;
- no recommendations: explain that recommendations improve after ratings and viewing.
- recommendation module unavailable in 1.0: omit the personalized row rather than displaying an error.

## 15. Destructive confirmation

Deletion dialog must show:

- title;
- number of files;
- exact paths in expandable section;
- operation: Move to Recycle Bin;
- retained data: rating, note, history, collections, playlists;
- files unavailable on network shares that may not support Recycle Bin;
- cancellation.

The default focused button is Cancel.

## 16. Theme customization

Theme editor exposes safe design tokens:

- background layers;
- surface colors;
- text colors;
- accent;
- focus color;
- corner radius;
- card density;
- font scale;
- motion amount;
- backdrop blur and dimming;
- poster aspect behavior.

A live preview is shown. Invalid contrast combinations produce warnings and may be blocked for critical text.

## 17. Responsive behavior

Although desktop-first, layouts adapt from compact laptop windows to large televisions:

- navigation collapses;
- grid column count changes;
- details layout stacks;
- player controls enlarge in ten-foot mode;
- no required horizontal window width above the documented minimum.

Minimum supported window size must be defined during UI prototyping and validated through automated visual tests.

## 18. Built-in extension UI in 1.0

Settings may display approved built-in metadata extensions and their health, attribution, and configuration. Public 1.0 does not expose Install Package, Browse Plugins, or third-party `.mhpkg` file handling. Those actions are introduced only with the hardened external-plugin milestone.


## Revision 1.1 backend integration note

Cloud-connected status and actions must use the same local-first UX language: local content remains usable, synchronization state is non-blocking, errors are actionable, and no screen presents backend availability as equivalent to media-library availability. Plugin and UI contracts must consume backend-neutral services rather than Supabase SDK objects.

---

## Document control

This document is part of the MediaHub revised baseline specification v1.2. Changes that affect architecture, security, data compatibility, plugin contracts, or user-visible behavior must be recorded in the Architecture Decision Record volume and reflected in the traceability matrix.
