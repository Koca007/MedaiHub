# Volume 30 — File Naming and Parsing Rulebook

**Project:** MediaHub  
**Document set:** MediaHub Project Bible  
**Status:** Revised baseline specification v1.2  
**Language:** English  
**Target platform:** Windows desktop first  
**Last updated:** 2026-08-08  

---

## 1. Parsing objective

Filename parsing produces structured evidence. It must not assume release-scene naming is always correct, and it must not silently force low-confidence mappings.

## 2. Normalization stages

1. preserve original filename;
2. remove extension;
3. Unicode normalize;
4. replace common separators with token boundaries;
5. preserve apostrophes and meaningful punctuation where possible;
6. identify bracketed groups;
7. identify technical suffixes;
8. identify year tokens;
9. identify episode tokens;
10. reconstruct candidate title.

## 3. Technical tokens

Examples:

- resolution: `720p`, `1080p`, `2160p`, `4K`;
- source: `BluRay`, `WEB-DL`, `WEBRip`, `HDTV`;
- codec: `x264`, `x265`, `HEVC`, `AV1`;
- HDR: `HDR10`, `HDR10+`, `Dolby Vision`, `DV`;
- audio: `AAC`, `AC3`, `EAC3`, `DTS`, `TrueHD`;
- channels: `2.0`, `5.1`, `7.1`;
- language: `HUN`, `ENG`, `JPN`, language names;
- edition: `Extended`, `Director's Cut`, `Remastered`;
- release group: usually final suffix.

Technical tokens are evidence and should not remove title words that genuinely match them without context.

## 4. Movie patterns

```text
Title.Year.Quality.Source.Codec.ext
Title (Year) Edition.ext
Title.Year.Language.Quality.ext
```

Examples:

```text
Dune.2021.2160p.UHD.BluRay.HDR.x265.mkv
King Kong (2005) Extended Cut.mkv
1917.2019.1080p.BluRay.mkv
```

The parser must distinguish title `1917` from year evidence by context.

## 5. Episode patterns

Supported:

```text
S01E02
S1E2
1x02
Season 1 Episode 2
Episode 12
E12
[12]
```

Absolute numbering is used only with series/folder context or explicit parser mode.

## 6. Multi-episode files

Examples:

```text
S01E01E02
S01E01-E02
S01E01-02
1x01-1x02
```

Required representation:

- create exactly one physical `MediaFile`;
- link it to every canonical episode in the detected ordered range;
- store one `MediaFileEpisodeSegment` per episode;
- use validated chapter markers as the preferred boundaries;
- otherwise use canonical runtimes only when their sum matches file duration within tolerance;
- allow explicit manual boundaries;
- store Unknown boundaries when none are trustworthy.

Unknown boundaries produce one combined range item such as `S01E01–E02`. Partial progress remains shared and is not divided by an arbitrary percentage. Natural completion marks all linked episodes watched. The review queue confirms ambiguous ranges before mappings are committed.

## 7. Specials

Tokens:

```text
S00E01
Special
OVA
OAD
SP
Bonus
```

Special classification depends on series metadata. Unknown special files enter review instead of being assigned to regular season.

## 8. Anime numbering

Anime files may use absolute episode numbers:

```text
Series Name - 012 [1080p].mkv
[Group] Series Name - 12 [WEB 1080p].mkv
```

Rules:

- require a series root/folder context;
- preserve release group separately;
- map absolute number through provider metadata when available;
- do not assume `012` is year;
- support cour/season aliases later.

## 9. Folders as evidence

Strong folder evidence:

```text
Series Name/
  Season 02/
    Series Name S02E03.mkv
```

Movie folder:

```text
Movie Name (2025)/
  Movie Name 2160p.mkv
```

Folder title and year may override a noisy filename only with confidence.

## 10. Language parsing

Language tokens are technical metadata, not necessarily title language.

A file may have:

```text
MULTi
DUAL
HUN
ENG
JPN
```

The scanner later verifies actual audio/subtitle tracks with FFprobe. Filename token is marked unverified.

## 11. Quality parsing

Quality in filename is validated against probe dimensions. If filename says 2160p but width/height indicates 1080p, technical probe wins and a mismatch warning is recorded.

## 12. Edition parsing

Edition tokens should remain attached to the media file/edition, not create a separate logical movie unless required.

Examples:

- Theatrical
- Extended
- Director's Cut
- Final Cut
- Remastered
- IMAX
- Unrated

## 13. Samples and extras

Default exclusion patterns:

```text
sample
trailer
featurette
behind the scenes
deleted scenes
extras
```

Future extras support may import these into a separate content type. A feature film named “Sample” must not be excluded solely by title; folder and size context apply.

## 14. Confidence output

Parser result example:

```json
{
  "candidateTitle": "Breaking Bad",
  "year": null,
  "season": 3,
  "episodes": [7],
  "quality": "1080p",
  "source": "BluRay",
  "codec": "H.265",
  "languageTokens": ["hu"],
  "confidence": 0.94,
  "evidence": [
    "explicit S03E07 token",
    "series root",
    "matching parent folder"
  ]
}
```

## 15. Ambiguity examples

### Same title, no year

```text
King Kong.mkv
```

Candidates 1933 and 2005: review required unless runtime/folder/provider IDs resolve.

### Numeric title

```text
1917.1080p.mkv
```

`1917` is title, not necessarily year. The absence of another title token and technical suffix context matters.

### Title containing season-like text

```text
Summer of 84.2018.mkv
```

Do not parse `84` as episode.

## 16. User rules

A matching decision may create:

- exact filename rule;
- parent-folder rule;
- regex-like safe pattern generated by host;
- series absolute-number mapping;
- edition naming rule.

User rules are previewed and can be removed.

## 17. Parser testing

Every fixed bug adds a regression case with:

- input path;
- root type;
- expected tokens;
- expected confidence;
- expected classification;
- expected review behavior.

## 18. Acceptance criteria

- Common movie/episode patterns parse.
- Numeric titles remain valid.
- Same-title ambiguous films review.
- Technical tokens are verified by probe.
- Absolute anime numbering requires context.
- Multi-episode file is not duplicated.
- Unknown multi-episode boundaries remain combined rather than generating guessed per-episode completion.
- User correction affects future equivalent files.

---

## Document control

This document is part of the MediaHub revised baseline specification v1.2. Changes that affect architecture, security, data compatibility, plugin contracts, or user-visible behavior must be recorded in the Architecture Decision Record volume and reflected in the traceability matrix.
