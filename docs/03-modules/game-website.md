# Game.Website

A fast-loading, traditional web portal (Blazor Server) — separate from the game client itself.

## Responsibilities

- User registration and account management.
- Leaderboards and forums.
- The "Play" entry point: once logged in, clicking Play loads the Unity WebGL canvas and
  passes the user's auth token directly into the game (see
  [system architecture](../01-architecture/system-architecture.md)).
- On mobile web, redirects visitors to the native App Store/Play Store apps instead of
  loading the WebGL build (see [ADR 0002](../02-decisions/0002-unity-as-client-engine.md) for
  why).

## Design system

The site implements the "Void & Ember" design language — see
[ADR 0004](../02-decisions/0004-design-language-and-ui-foundation.md) and
[docs/05-design](../05-design/design-language.md). In short: Bootstrap 5.3 stays as the
layout/behaviour engine, but its `--bs-*` variables are re-pointed at tokens mirrored from
`Game.Shared/Design/*.cs` so the website and (eventually) the Unity client share one visual
identity. Stylesheet load order matters — see the comment in `Components/App.razor`. New pages
should be built from the `.vk-*` primitives in `wwwroot/css/voidkargo.css`
(`Components/Pages/Home.razor` is the reference implementation); see
[component inventory](../05-design/component-inventory.md) for what's still needed.

## Depends on

- `Game.Shared` (design tokens; DTOs where applicable — e.g. leaderboard DTOs), `Game.Backend`
  REST API (auth, account, leaderboards).

## Run / test

```bash
dotnet run --project src/Game.Website
```

Prefer running via Aspire for the full stack — see
[local dev setup](../04-workflows/local-dev-setup.md).
