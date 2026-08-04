# Glossary

Domain terms used across `Game.Shared`, `Game.Backend`, and (eventually) `Game.Client`.
Keep names in code consistent with the terms here — if you introduce a new core concept,
add it here in the same PR.

This is a starter stub; expand it as domain modeling happens (see
[data model](../01-architecture/data-model.md)).

| Term | Meaning |
|---|---|
| **Player** | A registered user controlling one or more companies/fleets in the game world. |
| **Node** | A point on the game map that can produce, consume, or transfer resources — a Station, Planet, Dyson Sphere, or Production Site (see `NodeKind`). |
| **NodeKind** | The kind of Node: `Station` (consumption-driven, levels up — the "town" equivalent), `Planet`, `DysonSphere` (late-epoch mega-structure), or `ProductionSite` (finite output, has Stress). |
| **Hop** | A single leg a Ship travels between two Nodes. There is no track/route to lay — a ship's max hop distance is one of its stats (see `ShipTypeDto.HopDistance`); the game is about fleet assignment, not rail-laying. |
| **Ship** | A unit that moves resources between Nodes over one or more Hops; the primary "moving piece" pushed to clients over SignalR. An instance of a `ShipType`. |
| **ShipClass** | The broad performance archetype of a Ship: `LightHauler` (fast, low load, long hop), `MediumHauler` (versatile load/speed/acceleration/hop trade-off, only some epochs add new types), or `HeavyHauler` (slow, high load, short hop). |
| **ShipType** | A catalog entry (data, not code) describing one concrete ship design — roughly 20 per `ShipClass`. Drives both game math and the procedural art genome (matched by id). Gated by `Faction` and `Epoch`. |
| **Faction** | A playable faction identity (narrative, research tree, and art palette). Data-driven via `FactionDto`, not a fixed enum, so new factions don't require a contract change. |
| **Epoch** | A research-tree "etape"/era. Unlocking a `ShipType` or mega-structure gated to an Epoch typically requires completing a faction-specific special goal, not just accruing currency. |
| **Level** | A Node's progression tier. Stations level up by satisfying time-based consumption targets; Production Sites level up via investment + upgrade materials (see **Stress**). |
| **Stress** (`StressLevel`) | A 0.0–1.0 pressure value on a Node (chiefly `ProductionSite`s) that rises under heavy/overloaded throughput and is relieved by investing materials; sustained overload without relief caps or degrades production until addressed. |
| **Tick** | A discrete server-side simulation step where the economy, production, consumption, and ship positions advance. |
| **Economy tick** | A Tick specifically concerned with price/resource updates, pushed to clients via SignalR. |
| **Genome** | A validated, symbolic JSON description of a ship/station's procedural pixel art (silhouette, modules, palette zones) — the LLM-authorable input to the deterministic art generator. See [ADR 0006](../02-decisions/0006-procedural-indexed-palette-art-pipeline.md). |
| **Palette** | A named 16-colour row in the shared palette LUT texture. Swapping a Node/Ship's palette row re-skins it (faction identity, cosmetics) with no new art assets. See [ADR 0006](../02-decisions/0006-procedural-indexed-palette-art-pipeline.md). |
| **DTO** | Data Transfer Object defined in `Game.Shared`; the wire contract between `Game.Backend` and `Game.Client`. |
| **Seeder** | The EF Core dev-only component that populates a fresh database with a playable slice of the game on startup. |
