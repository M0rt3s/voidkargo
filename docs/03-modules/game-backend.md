# Game.Backend

ASP.NET Core Web API — the authoritative server for the game loop, economy, player actions,
and validation.

## Responsibilities

- Owns game state; validates and applies all player actions.
- Hosts SignalR hubs that push real-time updates (ship movement, economy ticks, chat) — see
  [networking strategy](../01-architecture/networking-strategy.md).
- Exposes REST endpoints for client-initiated, state-heavy actions (initial state load,
  transactions, history/leaderboards).
- Owns authentication and authorization: ASP.NET Core Identity (`Guid` keys) issues JWT bearer
  tokens consumed both by `Game.Website` (server-to-server) and, eventually, the Unity client.
  See [ADR 0005](../02-decisions/0005-jwt-plus-cookie-hybrid-auth.md) and
  [data model](../01-architecture/data-model.md).
- Persists to PostgreSQL via EF Core.
- In `Development`, runs an EF Core data seeder on startup to populate a fresh database with
  a playable slice of the game, plus the `Admin`/`Player`/`GameMaster` roles and three
  matching test accounts (`admin`/`admin`, `player`/`player`, `gamemaster`/`gamemaster`) — see
  [local dev setup](../04-workflows/local-dev-setup.md) for how to use them. This seeding, and
  the relaxed Identity password policy it depends on, is gated behind `IsDevelopment()` and
  never runs outside local development.

## Key folders (as they're introduced)

- `Hubs/` — SignalR hub classes.
- `Auth/` — JWT issuance (`JwtTokenService`), options + fail-fast validation (`JwtOptions`,
  `JwtOptionsValidation`), and the `/api/auth/*` minimal API endpoints (`AuthEndpoints`).
- `Entities/` — EF Core entities, including `UserEntity : IdentityUser<Guid>`.
- `Endpoints/` or `Controllers/` — REST API surface.
- `Data/` — EF Core `DbContext`, entity configurations, migrations.
- `Data/Seed/` — the dev-only seeder.
- `GameLoop/` — tick/simulation logic.

## Auth REST surface

- `POST /api/auth/register` — creates a user and always grants the `Player` role only
  (never derived from client input).
- `POST /api/auth/login` — validates credentials via `UserManager.CheckPasswordAsync`, returns
  a signed JWT + expiry + profile (including roles).
- `GET /api/auth/me` — `[RequireAuthorization]`; returns the caller's profile from their token
  claims.

## Depends on

- `Game.Shared` for DTOs, validation, and game math — do not duplicate types that belong there.
  Auth DTOs (`RegisterRequestDto`, `LoginRequestDto`, `AuthResponseDto`, `UserProfileDto`) and
  role constants (`GameRoles`) live there so `Game.Website` (and eventually the Unity client)
  share the same contract.

## Run / test

```bash
dotnet run --project src/Game.Backend         # standalone (no Aspire orchestration)
dotnet test tests/Game.Backend.Tests
```

For the full local stack (recommended), use Aspire — see
[local dev setup](../04-workflows/local-dev-setup.md).
