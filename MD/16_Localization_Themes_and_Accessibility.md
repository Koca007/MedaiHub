# Volume 16 — Localization, Themes and Accessibility

**Project:** MediaHub  
**Document set:** MediaHub Project Bible  
**Status:** Revised baseline specification v1.2  
**Language:** English  
**Target platform:** Windows desktop first  
**Last updated:** 2026-08-08  

---

## 1. Hungarian-first localization architecture

The supported Windows 1.0 user-interface language is Hungarian (`hu-HU`). The architecture remains fully localization-ready: no user-facing string may be hardcoded in view models, services, or extensions, and later language packs require resource additions rather than code changes.

Resource key convention:

```text
Area.Feature.Element.State
```

Examples:

```text
Player.Controls.Play
Library.Scan.Status.Indexing
Deletion.Dialog.RetainedData
Update.Banner.Available
```

## 2. Locale behavior

Version 1.0 determines locale from:

1. the installed supported locale, `hu-HU`;
2. fallback Hungarian resource `hu`;
3. internal English safety fallback if a resource is missing.

A user-facing language selector appears only when more than one complete UI language is shipped.

Locale affects:

- UI strings;
- date/time formatting;
- number formatting;
- file size formatting;
- sorting and collation;
- pluralization;
- metadata language preference, separately configurable.

## 3. Fallback chain

Example:

```text
hu-HU → hu → en
```

English remains a safety fallback and source-resource language, not an advertised complete Windows 1.0 UI locale. Missing translation is logged in development builds and falls back safely. Resource keys must never appear to end users in production unless all fallbacks fail.

## 4. Translation workflow

- source strings authored in English;
- stable semantic keys;
- context and screenshots where needed;
- placeholders named rather than positional where supported;
- ICU-style pluralization;
- automated validation for missing keys and placeholder mismatch;
- pseudolocalization build;
- translator notes;
- language pack version compatibility.

## 5. Plugin localization

Plugins ship resource bundles declared in the manifest. Host renders plugin-provided declarative UI using plugin resources. Missing plugin translation falls back to plugin English, then shows a safe label.

## 6. Metadata language

Interface language and content metadata language are separate. Version 1.0 metadata resolution order is fixed to Hungarian, original language, then English. Original titles may be shown alongside localized titles. A configurable order may be added later.

## 7. Theme engine

Themes are token-based, not arbitrary executable code.

Token groups:

- colors;
- typography;
- spacing;
- radii;
- shadows;
- motion;
- artwork treatment;
- player overlay;
- focus visuals.

Example theme schema:

```json
{
  "schemaVersion": 1,
  "id": "mediahub.default.dark",
  "name": "MediaHub Dark",
  "colors": {
    "background.primary": "#101114",
    "surface.primary": "#1A1C21",
    "text.primary": "#FFFFFF",
    "accent.primary": "#6A8DFF",
    "focus": "#FFFFFF"
  },
  "motion": {
    "duration.fast": 120,
    "duration.normal": 220
  }
}
```

Colors above are examples, not implementation mandates.

## 8. User customization

User can configure:

- base theme;
- accent;
- light/dark mode;
- background intensity;
- card size and density;
- corner radius;
- font scale;
- motion amount;
- artwork blur;
- player control opacity;
- focus indicator intensity.

Advanced customization remains constrained by accessibility validation.

## 9. Theme packages

Theme packages contain:

- manifest;
- token JSON;
- optional static assets;
- preview image;
- license;
- locales.

No assemblies or scripts are allowed in a pure theme package.

## 10. Accessibility requirements

- keyboard-complete navigation;
- screen-reader labels;
- accessible role/state/value;
- visible focus;
- focus restoration after dialogs;
- logical reading order;
- scalable text;
- high-contrast compatibility;
- reduced motion;
- captions not obscured by controls;
- minimum target sizes;
- non-color status indicators.

## 11. Keyboard navigation

Content rows:

- Left/Right moves cards.
- Up/Down moves rows.
- Enter opens or activates.
- Context-menu key opens actions.
- Home/End move to row edges where appropriate.
- Focus returns to originating card after closing details.

Player keyboard behavior is defined in Volume 07 and must remain operable with controls hidden.

## 12. Screen readers

Cards expose:

- title;
- media type;
- year;
- watched state;
- progress;
- availability;
- rating;
- primary action.

Progress is announced semantically, for example: “42 percent watched,” not as a raw visual bar.

## 13. Motion

Animations must:

- communicate hierarchy;
- remain short;
- not delay input;
- be disabled or reduced according to setting;
- avoid continuous decorative movement;
- avoid large parallax in reduced-motion mode.

## 14. Contrast

Critical text and controls meet recognized contrast targets. Theme editor validates text/surface pairs. Artwork overlays use gradients or scrims to maintain readability.

## 15. High DPI and display changes

- per-monitor DPI awareness;
- vector icons;
- correct relayout when moving between displays;
- artwork size variants;
- no bitmap text;
- fullscreen adapts to target monitor;
- persisted window bounds validated against current displays.

## 16. Right-to-left readiness

Even if RTL languages are not in the first translation set, layouts should avoid assumptions that make future RTL impossible. Icon directionality and media timeline behavior are explicitly reviewed.

## 17. Acceptance criteria

- Adding or changing a later installed language pack does not require reinstall.
- Missing translation falls back to English.
- The 1.0 Hungarian interface applies metadata fallback in the order Hungarian → original → English.
- Theme customization cannot make critical controls unreadable without warning.
- Primary workflows are keyboard-complete.
- Reduced-motion setting removes major transitions.
- Screen reader announces progress and watched state.


## Revision 1.1 backend integration note

Cloud-connected status and actions must use the same local-first UX language: local content remains usable, synchronization state is non-blocking, errors are actionable, and no screen presents backend availability as equivalent to media-library availability. Plugin and UI contracts must consume backend-neutral services rather than Supabase SDK objects.

---

## Document control

This document is part of the MediaHub revised baseline specification v1.2. Changes that affect architecture, security, data compatibility, plugin contracts, or user-visible behavior must be recorded in the Architecture Decision Record volume and reflected in the traceability matrix.
