# Contributing to MediaHub

MediaHub currently follows a short-lived feature-branch workflow with pull-request review.

## Before changing code

1. Read the relevant volumes under `MD/`.
2. Link the change to a stable requirement or backlog identifier.
3. Identify architecture, security, migration, localization, and accessibility impact.
4. Keep public version 1.0 scope gates intact.

## Quality expectations

- Enable nullable analysis and keep warnings at zero.
- Preserve the documented project dependency direction.
- Add automated tests for expected and failure behavior.
- Keep user-facing strings in localization resources.
- Never log notes, credentials, tokens, or sensitive media paths.
- Include local and remote migration plans when synchronized entities change.

Run the commands in `README.md` before requesting review. Database changes must be immutable after shared deployment, and RLS changes require policy tests in the same pull request.

Commits should be focused and use imperative summaries. Staging, committing, pushing, and pull-request creation remain separate repository actions.
