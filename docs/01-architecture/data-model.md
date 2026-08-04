# Data Model

Stub — to be filled in as domain modeling happens. Keep this in sync with the actual EF Core
entities in `Game.Backend` and DTOs in `Game.Shared` once they exist; don't let this drift
into aspirational-only documentation.

## Identity / auth entities (built)

- `UserEntity : IdentityUser<Guid>` (`Game.Backend/Entities/UserEntity.cs`) — ASP.NET Core
  Identity user with a required `DisplayName`. `GameDbContext` is an
  `IdentityDbContext<UserEntity, IdentityRole<Guid>, Guid>`, so the standard Identity tables
  (`AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, etc.) exist alongside the game's own tables.
- Roles are the fixed set in `Game.Shared/Auth/GameRoles.cs`: `Admin`, `Player`, `GameMaster`.
  Public self-registration (`POST /api/auth/register`) always grants `Player` only — `Admin`
  and `GameMaster` are only assigned via the Development seeder today (or a future admin-only
  management surface).
- See [ADR 0005](../02-decisions/0005-jwt-plus-cookie-hybrid-auth.md) for the auth architecture,
  [game-backend.md](../03-modules/game-backend.md) for the REST surface, and
  [local-dev-setup.md](../04-workflows/local-dev-setup.md) for the seeded dev accounts.

## Expected core entities (placeholder)

- `Player`
- `Node` (map location: town / resource site)
- `Route` (connects two Nodes)
- `Train` (moves along a Route, carries resources)
- `ResourceType` / `ResourceStock`
- `EconomyTick` (server-side simulation step record, if persisted)

See [Glossary](../00-overview/glossary.md) for definitions of these terms.

## Conventions once entities exist

- EF Core entities live in `Game.Backend` (or a `Game.Backend.Data` sub-namespace); they are
  **not** the same types as the DTOs in `Game.Shared` — map explicitly between them rather than
  exposing EF entities directly over the wire.
- The dev seeder (see [ADR 0001](../02-decisions/0001-use-net-aspire-for-orchestration.md) and
  [local dev setup](../04-workflows/local-dev-setup.md)) should be updated whenever a new core
  entity is introduced, so a fresh `dotnet run` always yields a playable slice of the game.
- The first EF Core migration (`InitialCreate`, `Data/Migrations/`) covers the Identity tables
  plus whatever game entities existed at the time it was generated. Add a new migration
  (`dotnet ef migrations add <Name> --project src/Game.Backend --output-dir Data/Migrations`)
  whenever the model changes — don't hand-edit generated migration files.
