# MediaHub

MediaHub is a Windows-first, local-first media library and player. It remains useful without an account or internet connection while using a hosted Supabase development backend for explicit non-personal services and internal synchronization contract testing.

## Current status

The repository contains the first executable foundation:

- .NET 10 LTS solution with Clean Architecture boundaries;
- WPF prototype shell with Hungarian resources and theme tokens;
- behavior-rich media title invariants and automated tests;
- production build gates that keep personal synchronization disabled for version 1.0;
- hosted Supabase schema, RLS policies, synthetic seed data, and three non-personal Edge Functions;
- Deno-formatted, linted, and type-checked Edge Function source;
- CI definitions for Windows/.NET and Deno validation.

The full product specification is in [`MD/`](MD/00_Documentation_Index.md).

## Prerequisites

- Windows 10 or Windows 11 x64;
- .NET SDK 10.0.302 or a compatible latest patch selected by `global.json`;
- Node.js 24 or later;
- Deno 2.9.5 for Edge Function checks;
- Supabase CLI installed through the pinned npm development dependency.

## Build and test

```powershell
npm ci
dotnet build MediaHub.sln --configuration Release
dotnet test MediaHub.sln --configuration Release
npm run edge:fmt:check
npm run edge:lint
npm run edge:check
```

Run the desktop application:

```powershell
dotnet run --project src/MediaHub.App/MediaHub.App.csproj
```

Validated library roots are stored in
`%LOCALAPPDATA%\MediaHub\Data\mediahub.db`. Start with `--safe-mode` to
keep optional extension startup disabled while diagnosing the application.

## Hosted Supabase workflow

This checkout is linked through the Supabase CLI credential store. Link metadata under `supabase/.temp/` is deliberately ignored. A local Supabase stack is not required for the current project workflow.

```powershell
npx supabase login
npx supabase link --project-ref <project-ref>
npm run supabase:dry-run
npx supabase db push --linked --include-seed
npx supabase functions deploy --use-api
```

Do not place service-role keys, database passwords, access tokens, or provider secrets in tracked files. The desktop client may eventually receive only a publishable key appropriate for an untrusted public client.

## Architecture rules

- Domain has no UI, persistence, network, player-engine, or Supabase dependency.
- Application exposes MediaHub-owned interfaces and DTOs.
- Supabase-specific types remain in `MediaHub.Infrastructure.Supabase`.
- File paths, network credentials, original media, and private notes never enter public backend calls.
- Public version 1.0 exposes no sign-in or personal synchronization.

## Licensing

The distribution license has not yet been selected. Do not assume open-source redistribution rights until a `LICENSE` file is added through an explicit product decision.
