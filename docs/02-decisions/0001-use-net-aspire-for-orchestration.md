# ADR 0001 — Use .NET Aspire for Local Orchestration

- **Status**: Accepted
- **Date**: 2026-07-30

## Context

The backend consists of multiple pieces that need to run together for a good local dev
experience: `Game.Backend` (API + SignalR), `Game.Website` (portal), and PostgreSQL. As a
solo developer, spinning these up manually (docker-compose scripts, manual connection
strings, manual seeding) is friction that eats into actual feature time and is easy to let
rot.

## Decision

Use **.NET Aspire** (`Game.AppHost` + `Game.ServiceDefaults`) to orchestrate local
development. A single `dotnet run --project src/Game.AppHost` starts the backend, website,
and a PostgreSQL container, wires up service discovery/connection strings automatically, and
in the `Development` environment triggers an EF Core data seeder that populates a fresh
database with a playable slice of the game (map, dummy players, resource nodes).

## Consequences

- **Easier**: one command to run the full stack locally; consistent environment for whoever
  (human or AI agent) picks up the project next; built-in dashboard for logs/traces during
  dev.
- **Harder**: Aspire is still a relatively young ecosystem; production deployment topology
  needs separate consideration (Aspire is primarily a local-dev/orchestration story, not
  necessarily how it ships to production).
- Requires the .NET Aspire workload/SDK components to be installed for contributors.

## Alternatives considered

- **docker-compose**: viable, but doesn't give the same tight C#-native dev-loop integration
  (health checks, service discovery, dashboard) with as little config.
- **Manual scripts / README instructions**: highest friction, most likely to bit-rot and be
  skipped, worst fit for "test locally with ease" goal.
