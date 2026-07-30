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

## Depends on

- `Game.Shared` (where applicable — e.g., leaderboard DTOs), `Game.Backend` REST API (auth,
  account, leaderboards).

## Run / test

```bash
dotnet run --project src/Game.Website
```

Prefer running via Aspire for the full stack — see
[local dev setup](../04-workflows/local-dev-setup.md).
