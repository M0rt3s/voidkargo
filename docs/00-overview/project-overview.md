# Project Overview

`voidkargo` is a cross-platform management and logistics strategy game, similar in spirit
to *Rail Nation*. Players manage transport networks, economy, and resources over time.

## Goals

- **Browser-first**: the primary client runs in the browser (via Unity WebGL).
- **Secondary native clients**: iOS and Android, built from the same Unity codebase.
- **Maximize code reuse**: one shared C# codebase (`Game.Shared`) between server and client.
- **Stay entirely in C#/.NET**: no separate frontend stack to maintain as a solo/small team.
- **High performance**: the game map/canvas must stay smooth — this drove the client engine
  choice (see [ADR 0002](../02-decisions/0002-unity-as-client-engine.md)).

## The pieces, at a glance

| Piece | Role |
|---|---|
| `Game.Backend` | Authoritative server: game loop, economy, player actions, validation. PostgreSQL-backed. |
| `Game.Shared` | DTOs, game math, grid logic, validation — shared verbatim by Backend and Unity. |
| `Game.Website` | Fast-loading web portal: registration, leaderboards, forums, launches the game. |
| `Game.Client` (Unity) | All visuals/UI/client logic. Exports to WebGL, iOS, Android from one codebase. |
| Networking | SignalR pushes real-time server-initiated data; REST handles client-initiated heavy actions. |
| .NET Aspire | Orchestrates local dev: spins up Backend + Website + Postgres, seeds dev data automatically. |

For how these connect, see [System architecture](../01-architecture/system-architecture.md).
For terminology used throughout the codebase, see [Glossary](glossary.md).
