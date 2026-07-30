# Game.Backend

ASP.NET Core Web API — the authoritative server for the game loop, economy, player actions,
and validation.

## Responsibilities

- Owns game state; validates and applies all player actions.
- Hosts SignalR hubs that push real-time updates (train movement, economy ticks, chat) — see
  [networking strategy](../01-architecture/networking-strategy.md).
- Exposes REST endpoints for client-initiated, state-heavy actions (initial state load,
  transactions, history/leaderboards).
- Persists to PostgreSQL via EF Core.
- In `Development`, runs an EF Core data seeder on startup to populate a fresh database with
  a playable slice of the game.

## Key folders (as they're introduced)

- `Hubs/` — SignalR hub classes.
- `Endpoints/` or `Controllers/` — REST API surface.
- `Data/` — EF Core `DbContext`, entity configurations, migrations.
- `Data/Seed/` — the dev-only seeder.
- `GameLoop/` — tick/simulation logic.

## Depends on

- `Game.Shared` for DTOs, validation, and game math — do not duplicate types that belong there.

## Run / test

```bash
dotnet run --project src/Game.Backend         # standalone (no Aspire orchestration)
dotnet test tests/Game.Backend.Tests
```

For the full local stack (recommended), use Aspire — see
[local dev setup](../04-workflows/local-dev-setup.md).
