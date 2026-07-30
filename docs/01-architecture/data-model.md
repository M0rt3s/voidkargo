# Data Model

Stub — to be filled in as domain modeling happens. Keep this in sync with the actual EF Core
entities in `Game.Backend` and DTOs in `Game.Shared` once they exist; don't let this drift
into aspirational-only documentation.

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
