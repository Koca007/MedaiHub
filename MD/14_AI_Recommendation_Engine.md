# Volume 14 — AI Recommendation Engine

**Project:** MediaHub  
**Document set:** MediaHub Project Bible  
**Status:** Revised baseline specification v1.2  
**Language:** English  
**Target platform:** Windows desktop first  
**Last updated:** 2026-08-08  

---

## 1. Purpose

The recommendation engine is a post-1.0 module. It helps the user discover titles already present in the library and may later discover externally known titles through configured metadata providers. Its learned profile is deliberately narrow:

- learn genre preferences;
- learn preferred playback quality;
- use explicit ratings and watch behavior only as weighting signals for genre and quality;
- never learn a persistent director, actor, cast, or free-text affinity profile.

Similar-title and same-director rows may be deterministic metadata features. They do not expand the learned AI profile.

The system is local-first, explainable, resettable, and never required for core playback.

## 2. What “offline” and “online” mean

### Offline recommendation

All feature extraction and ranking occur on the user's device using library metadata, ratings, and watch history. No viewing data leaves the machine. This is the post-1.0 default.

### Online-assisted recommendation

A future optional feature may query a remote model or recommendation service. It requires explicit consent, a clear disclosure of the fields sent, and an option to disable or delete associated remote data. Core recommendations must continue locally when this mode is disabled.

## 3. Recommendation stages

```text
Candidate generation
→ eligibility filtering
→ feature extraction
→ scoring
→ diversity adjustment
→ explanation generation
→ row construction
→ feedback capture
```

## 4. Candidate generation

Local candidates include:

- unwatched titles;
- partially watched series;
- highly related titles;
- deterministic titles by the director of a currently selected or highly rated title;
- titles in preferred genres;
- titles available in preferred quality.

The director- and similarity-based candidates above are generated from catalog metadata and the currently selected or explicitly named source title. They do not contribute person, keyword, franchise, or latent-factor fields to the user's learned profile.

Candidates exclude:

- hidden titles;
- explicitly dismissed recommendations;
- missing titles when the row requires immediate playback;
- titles below parental or content policy if a future control exists;
- duplicate editions represented as separate recommendations.

## 5. User preference profile

The profile is derived, not treated as immutable truth.

Genre profile fields:

- weighted watch time;
- completion count;
- average rating;
- recency;
- sample confidence;
- explicit negative feedback.

Quality profile fields:

- selected file resolution;
- HDR selection;
- codec/device compatibility;
- whether the user chose a lower-quality local file over a higher-quality network version;
- playback failures by quality.

No director or cast affinity vector is stored.

## 6. Signal weights

Initial deterministic model:

| Signal | Allowed influence |
|---|---:|
| Explicit five-star rating | strongly increases weights for that title's genres and chosen quality |
| Explicit one-star rating | strongly decreases weights for that title's genres; does not create a title/person aversion vector |
| Completion | increases genre/quality evidence |
| Abandoned early repeatedly | decreases genre/quality evidence with low confidence |
| Rewatch | strongly increases genre/quality evidence |
| Recent genre interest | moderately increases that genre weight |
| Preferred quality available | slightly increases quality fit |
| Missing/unreachable | deterministic eligibility exclusion |
| Already completed | deterministic row adjustment unless this is a rewatch row |
| User dismissed recommendation | suppresses that concrete candidate; it does not create a new learned feature |

Weights are configuration constants behind a versioned scoring model. Any later tuning may change coefficients, but the learned feature space remains limited to genre and quality.

## 7. Genre scoring

For each genre:

```text
genre score =
  rating component
+ completion component
+ normalized active watch time
+ recency component
- abandonment component
```

The score includes Bayesian smoothing or a minimum sample size so one title does not dominate.

## 8. Quality learning

The system learns preference from actual choices rather than assuming “higher is always better.”

Examples:

- user repeatedly chooses 1080p local versions instead of 4K files on a slow NAS;
- user chooses HDR on a compatible display;
- 4K versions fail on the device and should not be favored;
- user manually marks one edition preferred.

Quality preference influences which file to play and which recommendation cards receive a “Best available quality” badge. It must not hide titles simply because preferred quality is absent.

## 9. Similar title recommendations

Similarity can use:

- genre overlap;
- director;
- cast, lower weight;
- keywords and themes from metadata;
- release period;
- runtime;
- collection/franchise;
- user rating patterns.

Baseline can be content-based. Later versions may use embeddings, provided they can run locally or under explicit online consent.

Any embedding is content-to-content only. It cannot create a user-affinity dimension outside genre and quality, and ratings or viewing history cannot train or weight director, cast, keyword, theme, period, runtime, or franchise affinity.

## 10. Deterministic same-director suggestions

Same-director suggestions do not create or update a learned director profile. They may be generated from the currently viewed title or a specific highly rated title and must name that concrete reason, for example: “Another film directed by [Director].” Private note content is never inspected.

## 11. Diversity

Without diversity correction, recommendations can become repetitive. The ranking stage enforces:

- maximum items from one franchise;
- genre variety;
- a mix of familiar and exploratory candidates;
- no duplicate title;
- no multiple editions;
- controlled repetition across sessions.

## 12. Explanations

Every recommendation should have one concise reason:

- Because you watch science fiction.
- Similar to a film you rated five stars.
- Another film from the director of a specific title.
- Available in your preferred quality.
- Continue this series.
- Recently added to your library.

When multiple signals exist, use the strongest truthful reason.

## 13. Feedback

Actions:

- Not interested
- Hide this title
- Show fewer like this
- Show more from this genre
- Reset recommendation profile

Feedback is stored locally and is reversible through recommendation settings.

## 14. Cold start

Before enough data exists:

- use recent additions;
- use explicit favorites only to weight the genres and selected quality represented by those titles;
- ask no invasive onboarding questions;
- allow optional genre selection;
- use library popularity only if a provider supplies it;
- display “Recommendations improve as you watch and rate titles.”

## 15. Local model options

The post-1.0 recommendation module does not require a large language model. It can use:

- weighted feature vectors;
- cosine similarity;
- nearest-neighbor search;
- rule-based diversity;
- bounded genre/quality coefficient tuning.

A future semantic search model may run through ONNX or another local inference runtime. Model downloads are optional, signed, versioned, and size-disclosed.

## 16. Data boundaries

Allowed for the learned preference profile:

- title IDs;
- genres;
- ratings;
- completion;
- quality choice;
- timestamps for recency.

Allowed only for deterministic content-to-content candidate generation and explanation, never for learned user affinity:

- director and cast metadata;
- keywords and themes;
- release period, runtime, collection, and franchise metadata.

Not used by default:

- note text;
- raw screenshots;
- subtitle content;
- media bytes;
- unrelated file names;
- OS account information.

## 17. Evaluation

Offline evaluation metrics:

- precision at K based on subsequent plays;
- dismissal rate;
- diversity;
- novelty;
- explanation accuracy;
- coverage;
- repeated recommendation rate.

Product evaluation must not optimize only clicks; completions, ratings, and dismissals provide more meaningful signals.

## 18. Model versioning

Recommendation outputs record:

- model version;
- feature schema version;
- generation timestamp;
- reason code.

When the model changes, derived profiles can be rebuilt from retained events. The user can reset all derived recommendation data without deleting watch history.

## 19. Failure behavior

If the recommendation engine fails:

- hide or replace the recommendation row;
- do not block Home;
- log a sanitized diagnostic;
- allow profile rebuild;
- local playback remains unaffected.

## 20. Acceptance criteria

- A user with no history receives sensible non-personal rows.
- Ratings influence recommendations.
- Preferred genres emerge only after sufficient evidence.
- Preferred quality influences file choice without excluding titles.
- No learned director/cast affinity is produced.
- Every recommendation has an accurate explanation.
- User can dismiss and reset recommendations.
- Notes and media content are not used.
- Engine failure does not affect core features.


## 21. Supabase-backed development mode

After the recommendation milestone begins, Development/Test builds may synchronize narrow inputs to Supabase to validate future multi-device and server-generated recommendations. Allowed events include:

- genre viewed/completed;
- selected quality class;
- explicit star rating;
- recommendation shown;
- recommendation opened;
- recommendation dismissed;

Disallowed payloads include full local paths, network-share names, note text, subtitle content, screenshots, or original media files.

The local recommendation engine remains the fallback and must remain capable of producing recommendations without cloud access. Server-generated recommendations are cached as optional candidates with an explanation, generation version, expiry, and source. A remote result cannot overwrite user ratings or collections or expand the allowed learned feature set beyond genre and quality.

Public Windows 1.0 does not show a personalized recommendation row, collect recommendation feedback, or upload recommendation events.

Privileged calls to external model APIs occur only through Edge Functions or a later MediaHub backend. API keys are never embedded in the desktop client.

---

## Document control

This document is part of the MediaHub revised baseline specification v1.2. Changes that affect architecture, security, data compatibility, plugin contracts, or user-visible behavior must be recorded in the Architecture Decision Record volume and reflected in the traceability matrix.
