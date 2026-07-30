# Game.Shared

A plain C# class library referenced **verbatim** by both `Game.Backend` and the Unity
`Game.Client`. This is the single source of truth for the wire contract and core game rules.

## Contains

- DTOs (SignalR/REST payloads).
- Game math (economy formulas, movement/timing calculations).
- Grid/map logic shared between server simulation and client rendering.
- Validation rules that both server (authoritative) and client (optimistic UI) need to agree on.

## Rules

- **No dependencies on ASP.NET Core, EF Core, or Unity-specific APIs.** This project must
  compile and run unmodified inside a Unity project — keep it to plain .NET/C#.
- Treat public types here as a **contract**: changing a DTO shape affects `Game.Backend` and
  the Unity client simultaneously. Check both before merging, and update
  [data model](../01-architecture/data-model.md) / [glossary](../00-overview/glossary.md) if
  you introduce new domain concepts.

## Run / test

```bash
dotnet test tests/Game.Shared.Tests
```
