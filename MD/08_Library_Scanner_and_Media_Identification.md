# Volume 08 — Library Scanner and Media Identification

**Project:** MediaHub  
**Document set:** MediaHub Project Bible  
**Status:** Revised baseline specification v1.2  
**Language:** English  
**Target platform:** Windows desktop first  
**Last updated:** 2026-08-08  

---

## 1. Scanner goals

The scanner transforms raw files into a reliable logical library without blocking the UI. It combines real-time notifications, periodic reconciliation, filename parsing, technical probing, fingerprints, metadata matching, and user corrections.

## 2. Pipeline

```text
File-system signal
→ debounce and coalesce
→ path validation
→ stability check
→ extension allow-list
→ existing record lookup
→ filename parsing
→ quick fingerprint
→ ffprobe analysis
→ candidate classification
→ match scoring
→ commit or review queue
→ artwork/metadata jobs
→ read-model refresh
```

## 3. Supported inputs

Baseline video extensions:

```text
.mkv .mp4 .m4v .avi .mov .webm .ts .m2ts
```

Extensions are configurable but remain allow-listed. Sidecar files include common subtitle, image, and local metadata formats.

Ignored by default:

- sample folders;
- hidden temporary files;
- incomplete download suffixes;
- recycle/system folders;
- files below configurable duration or size;
- known extras unless extras support is enabled.

## 4. Real-time monitoring

Local roots use `FileSystemWatcher` or an equivalent abstraction. Watcher events are:

- debounced;
- coalesced by normalized path;
- delayed until file size and timestamp stabilize;
- retried for sharing violations;
- converted into scanner jobs.

Watcher overflow triggers a reconciliation scan. Watchers are not assumed reliable across all network shares.

## 5. Reconciliation

Reconciliation compares database and file system:

- enumerate current files;
- detect new paths;
- detect missing paths;
- detect changed size/timestamp;
- detect renames using identity or fingerprint;
- verify previously unreachable files;
- clean stale transient state.

Schedule:

- at application startup after shell is interactive;
- after root reconnect;
- after watcher overflow;
- periodic low-priority interval;
- manual user request.

## 6. Filename parsing

The parser extracts:

- title tokens;
- year;
- season and episode;
- absolute episode;
- multi-episode ranges;
- resolution;
- source;
- codec;
- audio language;
- subtitle language markers;
- release group;
- edition;
- part number;
- special/OVA markers where relevant.
- ordered multi-episode ranges such as `S01E01E02`.

Examples:

```text
Breaking.Bad.S03E07.1080p.BluRay.x265.mkv
Movie.Title.2025.Directors.Cut.2160p.HDR.mkv
Series Name - 2x04 - Episode Title.mp4
Anime.Title.012.1080p.WEB-DL.mkv
```

Parsing produces evidence with confidence, not a final truth.

## 7. Classification

A candidate is classified as movie, episode, or unknown using:

- root classification;
- season/episode tokens;
- folder structure;
- runtime;
- sibling-file patterns;
- local metadata;
- prior decisions.

Folder patterns may include:

```text
Series/Season 01/Series.S01E01.mkv
Movies/Movie Title (2025)/Movie Title.mkv
```

No one naming convention is required.

## 8. Multi-episode mapping

The scanner creates one `MediaFile` for a combined file and an ordered mapping for each detected canonical episode. It attempts to resolve segment boundaries from chapters first and from validated known episode runtimes second. Low-confidence boundaries remain Unknown; they are never converted into guessed individual progress. The review queue can confirm the episode range and optionally provide manual boundaries.

## 9. Technical probing

FFprobe or an equivalent tool extracts:

- duration;
- container;
- video streams;
- codec profile;
- dimensions;
- frame rate;
- bit depth;
- HDR metadata;
- audio streams;
- subtitle streams;
- default and forced flags;
- chapters;
- attachment streams.

Probe output is parsed from machine-readable JSON. External-process execution uses:

- argument lists, never shell concatenation;
- timeout;
- cancellation;
- restricted working directory;
- bounded output;
- exit-code validation.

## 10. Fingerprints

Three levels:

1. **Path signature:** fastest, least durable.
2. **Quick fingerprint:** file size plus sampled byte regions and technical properties.
3. **Full cryptographic hash:** optional, expensive, used for exact duplicate verification.

Fingerprinting is scheduled to avoid saturating disks. Network shares default to conservative sampling.

## 11. Match scoring

Illustrative weights:

| Evidence | Weight |
|---|---:|
| Exact user matching rule | 100 |
| Stable file identity | 95 |
| Full hash | 95 |
| Series + season + episode exact | 90 |
| Normalized title + year | 75 |
| Runtime within tolerance | 15 |
| Folder context | 10 |
| Quality/source tokens | 5 |
| Conflicting year | -40 |
| Conflicting episode | -100 |

Thresholds:

- high confidence: auto-commit;
- medium confidence: commit provisionally only if no competing candidate and user setting permits;
- low confidence: review queue.

## 12. Duplicate detection

Duplicates are categorized:

- exact duplicate bytes;
- same title and edition, different encoding;
- alternate quality;
- alternate audio;
- different cut;
- accidental duplicate path record.

MediaHub should preserve legitimate versions. It selects a preferred version but does not delete duplicates automatically.

Quality score may consider:

- resolution;
- HDR;
- bitrate;
- codec efficiency;
- audio layout;
- preferred language metadata;
- local versus network availability.

## 13. Ambiguity workflow

When two films share a title and the filename cannot disambiguate by year or other evidence, the scanner asks the user through the review queue.

The review card shows:

- path and filename;
- generated frame;
- runtime;
- technical quality;
- candidate artwork and year;
- confidence evidence;
- impact of the choice.

A decision can be applied to one file, a folder pattern, or future equivalent names.

## 14. Missing and disconnected files

A missing path is not immediately treated as deleted.

State progression:

```text
Reachable
→ TemporarilyUnavailable
→ MissingVerified
```

For network roots, state remains temporarily unavailable until the root itself is reachable and the path is verified absent.

## 15. Rename and move detection

Within a root, use stable identity where supported. Across roots:

- compare fingerprint;
- compare size;
- compare runtime and stream signature;
- compare title evidence.

If matched, update path while preserving media file ID. If uncertain, create a new file record and retain the old record as missing until review.

## 16. Scan performance

- bounded directory enumeration;
- batch database commits;
- reuse unchanged probe results;
- prioritize recently changed paths;
- postpone artwork generation during heavy playback;
- avoid full hashes during initial scan unless needed;
- cache parser normalization;
- report throughput and estimated remaining count without promising exact completion time.

## 17. Scanner safety

- never follow reparse points outside configured roots unless enabled;
- detect recursive directory loops;
- normalize paths before comparison;
- prevent path traversal in sidecar references;
- do not open executable content;
- limit image dimensions and decompression;
- treat malformed metadata as untrusted.

## 18. Acceptance criteria

- A newly copied stable media file appears without manual refresh.
- A file copied in chunks is not indexed until stable.
- A disconnected NAS does not remove library items.
- Rename preserves progress and rating.
- Same-title films produce a review when evidence is insufficient.
- Deleting through MediaHub moves local files to Recycle Bin and retains the title.
- Reintroducing the same file reattaches history.
- Watcher overflow is repaired by reconciliation.
- A combined episode-range file produces one physical record and multiple ordered canonical episode mappings.
- Unknown multi-episode boundaries remain combined rather than generating false per-episode progress.

---

## Document control

This document is part of the MediaHub revised baseline specification v1.2. Changes that affect architecture, security, data compatibility, plugin contracts, or user-visible behavior must be recorded in the Architecture Decision Record volume and reflected in the traceability matrix.
