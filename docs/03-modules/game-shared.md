# Game.Shared

A plain C# class library referenced **verbatim** by both `Game.Backend` and the Unity
`Game.Client`. This is the single source of truth for the wire contract and core game rules.

## Contains

- DTOs (SignalR/REST payloads).
- Game math (economy formulas, movement/timing calculations).
- Grid/map logic shared between server simulation and client rendering.
- Validation rules that both server (authoritative) and client (optimistic UI) need to agree on.
- `Design/` — canonical UI design tokens (colour, typography, spacing/motion metrics). The
  single source of truth for the "Void & Ember" visual identity, mirrored by the website's
  `wwwroot/css/tokens.css` and consumed directly by Unity once its UI is built. See
  [ADR 0004](../02-decisions/0004-design-language-and-ui-foundation.md) and
  [docs/05-design](../05-design/design-language.md).
- `Art/` (planned) — the deterministic, genome-driven procedural pixel-art generator for ships
  and stations (silhouette/module generation, indexed-canvas rendering, palette validation, PNG
  encoding). No Unity/ASP.NET Core dependency, so both `Game.Backend` tooling and the Unity
  Editor's bake step can call it. See
  [ADR 0006](../02-decisions/0006-procedural-indexed-palette-art-pipeline.md).

## Rules

- **No dependencies on ASP.NET Core, EF Core, or Unity-specific APIs.** This project must
  compile and run unmodified inside a Unity project — keep it to plain .NET/C#.
- **Targets `net10.0;netstandard2.1`.** The `netstandard2.1` leg exists because Unity is
  referenced as a [local package](https://docs.unity3d.com/Manual/upm-ui-local.html) pointing
  directly at this folder's source (see
  [`client/Game.Client/README.md`](../../client/Game.Client/README.md)) — Unity ignores this
  project's `.csproj` and compiles the `.cs` files itself with its own compiler, currently
  pinned to **C# 9.0**. Avoid C# 10+-only syntax here (file-scoped namespaces, implicit
  usings) — use explicit `using` statements and block-scoped namespaces so the same files
  compile under both the `dotnet` SDK and Unity.
- Treat public types here as a **contract**: changing a DTO shape affects `Game.Backend` and
  the Unity client simultaneously. Check both before merging, and update
  [data model](../01-architecture/data-model.md) / [glossary](../00-overview/glossary.md) if
  you introduce new domain concepts.

## Run / test

```bash
dotnet test tests/Game.Shared.Tests
```
