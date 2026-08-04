# Data Model

Reflects the entities in `Game.Backend/Entities` and DTOs in `Game.Shared/Dtos`. Keep this in
sync as the actual EF Core entities and DTOs evolve — don't let this drift into
aspirational-only documentation.

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
- **Known open item**: `PlayerEntity` (below) currently exists side-by-side with `UserEntity`
  rather than being unified with it — a legacy of the game-domain and auth work landing on
  parallel branches. Reconciling "the Identity user" and "the Player controlling a fleet" into
  one relationship (likely `PlayerEntity.UserId` referencing `UserEntity.Id`) is a needed
  follow-up, not yet done; don't build new features that assume they're already the same row.

## Core game-domain entities

- `Player` — currently its own row (`PlayerEntity`); see the reconciliation note above regarding
  its relationship to `UserEntity`. Controls one or more companies/fleets.
- `Faction` — a playable faction identity: narrative flavour, research-tree gating, and (via
  `PaletteId`) which palette LUT row re-skins its ships/stations. See
  [ADR 0006](../02-decisions/0006-procedural-indexed-palette-art-pipeline.md).
- `ShipType` — a catalog entry (data, not code) for one concrete ship design. ~20 per
  `ShipClass` (`LightHauler` / `MediumHauler` / `HeavyHauler`), each gated to a `Faction` and an
  `Epoch`, carrying `LoadCapacity` / `Speed` / `Acceleration` / `HopDistance` stats. Its `Id`
  also matches the art genome that renders it (see
  [ADR 0006](../02-decisions/0006-procedural-indexed-palette-art-pipeline.md)) — game math
  and art share one key.
- `Ship` — an instance of a `ShipType` owned by a `Player`, currently travelling between two
  `Node`s (`FromNodeId`/`ToNodeId`) with a `ProgressPercent` along the current hop. There is no
  `Route`/track entity — hop distance is a `ShipType` stat, not something players lay down.
- `Node` — a map location with a `NodeKind`:
  - `Station` — consumption-driven; levels up over time as delivered resources satisfy its
    consumption target (the "town" analogue).
  - `Planet` — may host a `Station` and/or one or more `ProductionSite`s.
  - `DysonSphere` — late-epoch, faction-specific mega-structure.
  - `ProductionSite` — finite output; `StressLevel` (0.0–1.0) rises under overload and is
    relieved by investing upgrade materials, which raises `Level` and the production ceiling.
- `EconomyTick` (server-side simulation step record, if persisted) — not yet a stored entity;
  currently only exists as the `EconomyTickDto` pushed over SignalR.

## Conventions

- EF Core entities live in `Game.Backend.Entities`; they are **not** the same types as the DTOs
  in `Game.Shared` — map explicitly between them rather than exposing EF entities directly over
  the wire (see `Program.cs`'s `/api/game-state` endpoint for the mapping pattern).
- **Exception, deliberately narrow**: closed, low-churn value enums (`NodeKind`, `ShipClass`)
  are reused directly from `Game.Shared.Dtos` inside entities instead of being re-declared per
  layer. These aren't evolving wire-contract shapes the way a DTO's field set is — duplicating
  them would only invite drift. Don't extend this exception to records/classes.
- `ShipType` and `Faction` are catalog/reference data (seeded, rarely mutated at runtime), while
  `Node`, `Ship`, and `Player` are live game-state rows. Keep that distinction in mind when
  designing migrations or caching — catalog data is a much safer thing to cache client-side.
- The dev seeder (see [ADR 0001](../02-decisions/0001-use-net-aspire-for-orchestration.md) and
  [local dev setup](../04-workflows/local-dev-setup.md)) should be updated whenever a new core
  entity is introduced, so a fresh `dotnet run` always yields a playable slice of the game.
- The first EF Core migration (`InitialCreate`, `Data/Migrations/`) covers the Identity tables
  plus whatever game entities existed at the time it was generated. Add a new migration
  (`dotnet ef migrations add <Name> --project src/Game.Backend --output-dir Data/Migrations`)
  whenever the model changes — don't hand-edit generated migration files.

## Progression loop (design intent, not yet simulated server-side)

1. Ships haul resources into a `Station`, satisfying its time-based consumption target →
   `Station.Level` increases → higher demand/rewards unlock.
2. Ships haul resources out of a `ProductionSite`; pushing it past sustainable throughput raises
   `StressLevel`. Investing upgrade materials relieves stress and raises `Level`, which raises
   the production ceiling — a deliberate two-sided pressure loop (tracked for implementation in
   the economy-tick phase; see the backend module doc once it lands).
3. New `ShipType`s and `NodeKind.DysonSphere` mega-structures unlock via a `Faction`'s research
   tree, gated by `Epoch` and (for some nodes) a specific one-off goal rather than raw currency —
   this is intentionally not implemented as a resource sink alone.

See [Glossary](../00-overview/glossary.md) for term definitions.
