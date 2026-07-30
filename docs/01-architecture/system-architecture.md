# System Architecture

## Diagram

```
                         ┌─────────────────────────┐
                         │        Players           │
                         └────────────┬─────────────┘
                                      │
        ┌─────────────────────────────┴──────────────────────────────┐
        │                                                             │
        ▼                                                             ▼
┌───────────────────┐                                     ┌───────────────────────┐
│   Game.Website     │  registration / leaderboards /      │   Game.Client (Unity) │
│ (Blazor Server)    │  forums / "Play" launches Unity ───▶│  WebGL / iOS / Android│
└─────────┬──────────┘                                     └────────────┬──────────┘
          │  REST (auth, account)                     REST │            │ SignalR
          ▼                                                 ▼            ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                              Game.Backend (ASP.NET Core)                    │
│   REST API (client-initiated actions)      SignalR Hubs (server pushes)     │
│   Game loop · Economy · Validation · Auth                                   │
└───────────────────────────────┬───────────────────────────────────────────--┘
                                 │  EF Core
                                 ▼
                         ┌───────────────┐
                         │  PostgreSQL   │
                         └───────────────┘

        Game.Shared (DTOs, math, grid logic, validation)
        referenced by both Game.Backend and Game.Client
```

## Local orchestration (.NET Aspire)

`Game.AppHost` is the single entry point for local development: `dotnet run` from it starts
`Game.Backend`, `Game.Website`, and a PostgreSQL container, wires up connection strings and
service discovery, and (in `Development`) triggers an EF Core seeder so there's a playable
slice of the game (map, dummy players, resource nodes) immediately. See
[local dev setup](../04-workflows/local-dev-setup.md) and
[ADR 0001](../02-decisions/0001-use-net-aspire-for-orchestration.md).

## Why Unity for the client

WebAssembly can't talk to the DOM/WebGL directly without heavy JS interop, which would kill
frame rate for a real-time map. Unity was chosen to stay 100% in C# while hitting native
performance, compiling to WebGL, iOS, and Android from one codebase. Full rationale in
[ADR 0002](../02-decisions/0002-unity-as-client-engine.md).

## Networking

See [networking strategy](networking-strategy.md) for the SignalR vs REST split, and
[ADR 0003](../02-decisions/0003-signalr-plus-rest-hybrid-networking.md) for why a hybrid
approach was chosen over pure REST polling or pure SignalR.

## Per-project detail

Each project has its own short doc under [`docs/03-modules`](../03-modules/game-backend.md).
