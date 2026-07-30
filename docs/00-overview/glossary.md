# Glossary

Domain terms used across `Game.Shared`, `Game.Backend`, and (eventually) `Game.Client`.
Keep names in code consistent with the terms here — if you introduce a new core concept,
add it here in the same PR.

This is a starter stub; expand it as domain modeling happens (see
[data model](../01-architecture/data-model.md)).

| Term | Meaning |
|---|---|
| **Player** | A registered user controlling one or more companies/fleets in the game world. |
| **Node** | A point on the game map that can produce, consume, or transfer resources (e.g., a town or resource site). |
| **Route** | A connection between two Nodes that a Train travels along. |
| **Train** | A unit that moves resources between Nodes along a Route; the primary "moving piece" pushed to clients over SignalR. |
| **Tick** | A discrete server-side simulation step where the economy, production, and train positions advance. |
| **Economy tick** | A Tick specifically concerned with price/resource updates, pushed to clients via SignalR. |
| **DTO** | Data Transfer Object defined in `Game.Shared`; the wire contract between `Game.Backend` and `Game.Client`. |
| **Seeder** | The EF Core dev-only component that populates a fresh database with a playable slice of the game on startup. |
