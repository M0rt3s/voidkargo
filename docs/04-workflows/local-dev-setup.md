# Local Dev Setup

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/) — version pinned in [`global.json`](../../global.json).
- [Docker](https://www.docker.com/) (or another OCI-compatible container runtime) — .NET
  Aspire uses it to run PostgreSQL locally.
- (Later, for client work) Unity Editor — see
  [`client/Game.Client/README.md`](../../client/Game.Client/README.md).

## Running the full stack

```bash
dotnet run --project src/Game.AppHost
```

This starts:
- A PostgreSQL container.
- `Game.Backend` (API + SignalR), connected to that Postgres instance.
- `Game.Website` (portal).
- The Aspire dashboard, with links/ports for each resource, plus logs and traces.

In the `Development` environment, an EF Core seeder runs automatically against the fresh
database so there's a playable slice of the game (map, dummy players, resource nodes)
immediately — no manual setup needed. See
[data model](../01-architecture/data-model.md) and
[ADR 0001](../02-decisions/0001-use-net-aspire-for-orchestration.md).

## Running pieces individually

Useful when iterating on just one project:

```bash
dotnet run --project src/Game.Backend
dotnet run --project src/Game.Website
```

Note: without Aspire, you'll need to supply your own PostgreSQL connection string (e.g., via
`appsettings.Development.json` or environment variables) and the dev seeder won't run
automatically.

## Building and testing everything

```bash
dotnet build
dotnet test
```
