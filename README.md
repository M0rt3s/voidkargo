# voidkargo

A cross-platform management and logistics strategy game (similar to *Rail Nation*) — browser
as the primary client, native iOS/Android as secondary clients, built entirely in C#/.NET.

## Quickstart

```bash
dotnet restore
dotnet build
dotnet test

# Run the full local stack (API + Website + PostgreSQL + seeded dev data) via .NET Aspire:
dotnet run --project src/Game.AppHost
```

Prerequisites and details: [docs/04-workflows/local-dev-setup.md](docs/04-workflows/local-dev-setup.md).

## Repo map

| Path | What it is |
|---|---|
| `src/Game.Backend` | ASP.NET Core Web API + SignalR — the authoritative game server. |
| `src/Game.Shared` | Shared DTOs/game math/validation, referenced by both Backend and the Unity client. |
| `src/Game.Website` | Blazor portal: registration, leaderboards, "Play" entry point. |
| `src/Game.AppHost` / `Game.ServiceDefaults` | .NET Aspire local orchestration. |
| `client/Game.Client` | Unity client (WebGL/iOS/Android) — placeholder, see its [README](client/Game.Client/README.md). |
| `tests/` | xUnit test projects. |
| `docs/` | The documentation vault — start here. |

## Documentation

This repo's docs live with the code, under [`docs/`](docs/README.md) — a plain-Markdown
vault, readable on GitHub and openable directly as an Obsidian vault. Start at
[docs/README.md](docs/README.md) for the full map of content, including architecture,
decision records (ADRs), per-module docs, and workflows.

For AI coding agents working in this repo, see [AGENTS.md](AGENTS.md).
For human contributor workflow, see [CONTRIBUTING.md](CONTRIBUTING.md).
