# ADR-032: WPF for the foundation prototype

- Status: Provisional
- Date: 2026-08-10

## Context

The specification leaves WPF versus WinUI 3 open until prototype evidence exists. Foundation work still requires a runnable Windows shell without allowing presentation technology to leak into Domain or Application.

## Decision

Use WPF on .NET 10 for the initial executable foundation. Keep all WPF references inside `MediaHub.App`, use resource dictionaries for Hungarian localization and theme tokens, and enforce boundaries with architecture tests.

## Consequences

The team can validate startup, accessibility, high-DPI behavior, cinematic rows, player embedding, and Windows 10/11 compatibility early. This is not the final production UI selection. A measured WinUI 3 comparison remains required before closing the platform prototype gate.

## Review trigger

Review after both UI prototypes have evidence for startup time, virtualization, keyboard navigation, accessibility, deployment, player-surface integration, and Windows 10/11 stability.
