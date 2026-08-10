# Volume 07 — Player Engine

**Project:** MediaHub  
**Document set:** MediaHub Project Bible  
**Status:** Revised baseline specification v1.2  
**Language:** English  
**Target platform:** Windows desktop first  
**Last updated:** 2026-08-08  

---

## 1. Objective

The player module provides reliable, hardware-accelerated playback without exposing the selected engine to the rest of the application. LibVLCSharp is the preferred initial engine, with MPV retained as an evaluated alternative. The final choice is recorded through an ADR after prototype benchmarks.

## 2. Player abstraction

```csharp
public interface IMediaPlayer : IAsyncDisposable
{
    PlayerState State { get; }
    TimeSpan Position { get; }
    TimeSpan Duration { get; }
    double Volume { get; }

    event EventHandler<PlayerStateChangedEventArgs>? StateChanged;
    event EventHandler<PositionChangedEventArgs>? PositionChanged;
    event EventHandler<TrackListChangedEventArgs>? TracksChanged;
    event EventHandler<PlayerErrorEventArgs>? ErrorOccurred;

    Task OpenAsync(
        PlaybackSource source,
        PlaybackOptions options,
        CancellationToken cancellationToken);

    Task PlayAsync(CancellationToken cancellationToken);
    Task PauseAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
    Task SeekAsync(TimeSpan position, CancellationToken cancellationToken);
    Task SetVolumeAsync(double volume, CancellationToken cancellationToken);
    Task SelectAudioTrackAsync(string trackId, CancellationToken cancellationToken);
    Task SelectSubtitleTrackAsync(string? trackId, CancellationToken cancellationToken);
}
```

The interface is intentionally task-based even when the underlying engine call is synchronous, because opening media, track enumeration, and network sources can block or fail asynchronously.

## 3. Playback source resolution

The application resolves a playable item to a preferred available file using:

1. explicit user-selected edition;
2. stored preferred media file;
3. highest quality score compatible with the device;
4. local file over slow network where configured;
5. reachable and verified state;
6. deterministic tie-breaker.

After 1.0, quality preference may be learned by the recommendation module but must remain user-overridable. Version 1.0 uses explicit/default quality selection only.

## 4. Session lifecycle

States:

```text
Created
Resolving
Opening
Buffering
Playing
Paused
Seeking
Ended
Failed
Stopped
Disposed
```

Transitions are validated. Duplicate Play or Pause commands are idempotent.

A `PlaybackSession` contains:

- session ID;
- playable item ID;
- media file ID;
- selected tracks;
- start reason;
- start position;
- checkpoint sequence;
- timestamps;
- completion result;
- termination reason.

## 5. Resume behavior

Resume is offered when:

- saved position exceeds the minimum threshold, default 60 seconds;
- saved position is below the completion boundary;
- duration remains sufficiently similar to avoid resuming into a different edition incorrectly.

If the preferred file changed and runtime differs materially, the application warns or maps progress proportionally only when a safe edition mapping exists.

Checkpoint policy:

- every 10–15 seconds while playing;
- immediately on pause;
- immediately before seek if enough time elapsed;
- on track changes;
- on player close;
- on application shutdown;
- final checkpoint on end.

Checkpoints are throttled and monotonic within a session.

## 6. Completion

Default completion threshold: 90%.

Exceptions:

- files shorter than configurable minimum;
- credits markers in a future version;
- manual watched/unwatched override;
- episode autoplay where the engine reaches natural end.

Natural end marks complete even when timestamp precision is below the percentage threshold.

## 7. Audio

Baseline behavior:

- select the container's default audio track;
- if no default exists, use engine default;
- remember an explicit user choice for the current title or series only when the user opts in;
- expose language, title, codec, channels, bitrate, and default flag;
- support audio delay where engine supports it;
- normalize volume only as an explicit setting.

No global language override is required by the baseline decision.

## 8. Subtitles

Supported sources:

- embedded streams;
- local sidecar files;
- manually imported files;
- downloaded provider files after the online-subtitle milestone;
- future user-defined subtitle folders.

When provider download is added after 1.0, downloaded subtitles are registered but disabled by default. Version 1.0 supports embedded, sidecar, and manual subtitle import.

Subtitle menu displays:

- language;
- title;
- source;
- hearing-impaired flag;
- forced flag;
- format;
- provider attribution where required.

User can adjust delay, size, vertical position, font family, outline, and background according to engine capability.

## 9. Seeking and controls

Default seek increment is ten seconds. Repeated key presses may accelerate only if this behavior is clear and configurable. Seeking clamps to valid duration and handles unknown-duration streams gracefully.

Timeline provides:

- buffered state where available;
- chapter markers;
- resume marker;
- optional intro/credits markers in future;
- hover preview thumbnails in a later milestone.

## 10. Fullscreen and cinematic mode

- Double-click toggles fullscreen.
- Escape exits fullscreen before closing player.
- Controls and cursor hide after inactivity.
- UI reappears on mouse movement, click, keyboard input, or focus change.
- System sleep is prevented while actively playing, according to setting.
- Screen saver behavior is configurable.
- The application restores previous window state after fullscreen.

## 11. Screenshot capture

Screenshots support:

- user-triggered gallery captures;
- generated artwork fallback;
- resume thumbnails.

Captures store:

- image asset ID;
- playable item ID;
- playback timestamp;
- source file fingerprint;
- creation reason;
- dimensions;
- privacy flag.

Gallery captures provide “Jump to scene” when the same compatible edition is available. The action verifies the source fingerprint and edition before seeking and falls back to opening the capture when the source changed. Generated artwork chooses a frame away from black frames, opening logos, and credits where possible.

## 12. Next episode

After an episode finishes:

- show next episode countdown if enabled;
- keep autoplay disabled by default;
- always expose a Next Episode button when a canonical successor is available;
- allow Cancel, Play Now, or Back to Series;
- skip unavailable episodes only with a visible message;
- never silently jump seasons if canonical ordering is uncertain;
- preserve autoplay preference.

When the current session originates from a playlist, the next target is the next playlist entry. Playlist order takes precedence over canonical episode order, film sequels, franchises, and other continuation rules.

## 13. Multi-episode playback

A file such as `S01E01E02.mkv` is opened once and represented by one physical `MediaFile`. Ordered episode-segment mappings identify the canonical episodes within it.

Boundary priority:

1. validated chapter markers;
2. validated known episode runtimes whose sum matches the file duration within tolerance;
3. explicit user mapping;
4. Unknown.

When boundaries are Unknown, MediaHub displays and tracks one combined episode-range playback item. It does not guess that 50% equals the end of episode one. Natural completion marks all linked episodes complete; the user may still apply manual watched overrides.

## 14. Error handling

Player errors capture:

- engine error code;
- file reachability;
- codec/container summary;
- hardware acceleration status;
- selected tracks;
- sanitized path;
- engine logs within size limits.

Recovery options:

- retry;
- reopen with hardware acceleration disabled;
- choose another media file;
- open diagnostics;
- mark file unsupported.

Fallback behavior must not repeatedly loop without user control.

## 15. Hardware acceleration

Auto mode is default. The application records the effective decoder and allows:

- Auto
- Enabled
- Disabled

Per-file compatibility overrides may be stored after a failure. Hardware capability detection must not prevent playback on unsupported devices.

## 16. Test requirements

The player test matrix includes:

- H.264/H.265/AV1 where supported;
- MKV/MP4/WebM;
- stereo and multichannel audio;
- multiple audio and subtitle streams;
- variable frame rate;
- HDR and SDR;
- very short and very long files;
- corrupt files;
- network interruption;
- sleep/resume;
- display change;
- fullscreen transitions;
- high DPI;
- abnormal application termination;
- edition switch and resume safety.
- Picture-in-Picture session reuse;
- screenshot gallery scene jump with matching and changed editions;
- playlist-origin continuation precedence;
- multi-episode exact, inferred, and unknown segment boundaries.

---

## Document control

This document is part of the MediaHub revised baseline specification v1.2. Changes that affect architecture, security, data compatibility, plugin contracts, or user-visible behavior must be recorded in the Architecture Decision Record volume and reflected in the traceability matrix.
