# AGENTS.md

Instructions for any AI coding agent (GitHub Copilot, Cursor, Claude, Aider, etc.)
working in this repository. Read this before making changes. Humans: see
[CONTRIBUTING.md](CONTRIBUTING.md) instead (or too).

## What this project is

`voidkargo` is a cross-platform management/logistics strategy game (think Rail Nation).
Browser is the primary client; iOS/Android are secondary. The whole stack — backend,
shared logic, web portal, and game client — is C#/.NET. Full architecture context lives
in [`docs/01-architecture/system-architecture.md`](docs/01-architecture/system-architecture.md).

## Repo map

| Path | What it is |
|---|---|
| `src/Game.Backend` | ASP.NET Core Web API + SignalR hubs. Game loop, economy, validation, EF Core/PostgreSQL. |
| `src/Game.Shared` | Class library: DTOs, game math, grid logic, validation. Referenced by **both** Backend and the Unity client — treat changes here as a public contract. |
| `src/Game.Website` | Blazor Server portal: registration, leaderboards, forums, "Play" entry point. |
| `src/Game.AppHost` | .NET Aspire orchestration entry point for local dev (`dotnet run` here starts everything). |
| `src/Game.ServiceDefaults` | Shared Aspire service defaults (telemetry, health checks, resilience). |
| `tests/` | xUnit test projects, one per `src/` project that has logic worth testing. |
| `content/` | Shared JSON content: ship genomes (`ship-genomes/`) and palettes (`palettes/`) consumed by `Game.Shared`'s art pipeline (ADR 0006), the Unity "Foundry" editor tool, and `Game.Shared.Tests/Art/ContentTests.cs`. |
| `client/Game.Client` | Unity project (WebGL + iOS + Android). Not scaffolded via CLI — see its own README. |
| `docs/` | The documentation vault. Start at [`docs/README.md`](docs/README.md). |

## How to build and test

```bash
dotnet restore
dotnet build
dotnet test
```

To run the full local stack (API + Website + Postgres + seeded dev data) via Aspire:

```bash
dotnet run --project src/Game.AppHost
```

See [`docs/04-workflows/local-dev-setup.md`](docs/04-workflows/local-dev-setup.md) for details,
ports, and prerequisites.

## Conventions

- **C# style**: follow `.editorconfig` (enforced). Nullable reference types enabled project-wide.
- **`Game.Shared` is a contract**: if you change a DTO or public type there, check both
  `Game.Backend` usages and note it in the module doc — the Unity client depends on the same file.
- **SignalR vs REST**: server-initiated/real-time data (moves, ticks, chat) goes over SignalR;
  client-initiated heavy actions (buy ship, initial state load, history/leaderboards) go over
  REST. Don't blur this — see
  [`docs/01-architecture/networking-strategy.md`](docs/01-architecture/networking-strategy.md).
- **Big/expensive-to-reverse decisions are recorded as ADRs** in `docs/02-decisions/`. Read the
  relevant ADR before proposing to change engine choice, orchestration, or networking strategy.
  If you believe a decision should change, propose a new ADR rather than silently deviating.
- **Keep docs in sync**: if a change alters a module's behavior, contract, or how to run/test it,
  update the matching file in `docs/03-modules/`. Small, targeted doc updates — not rewrites.

## Definition of done

Before considering any task complete, check
[`docs/04-workflows/definition-of-done.md`](docs/04-workflows/definition-of-done.md). Summary:
solution builds, tests pass (new tests added for new logic), relevant docs updated, no unrelated
changes bundled in.

## What NOT to do

- Don't add new frameworks/services (queues, caches, extra databases) without an ADR — this is a
  solo-dev project; keep the stack minimal and justified.
- Don't hand-edit generated Unity meta files or `Library/`/`Temp/` folders.
- Don't hand-edit Unity scene (`.unity`) or prefab (`.prefab`) YAML directly — these formats are
  easy to corrupt by hand. Create/modify scene and prefab content through Unity Editor scripts
  (e.g. the "Foundry" tooling described in
  [ADR 0006](docs/02-decisions/0006-procedural-indexed-palette-art-pipeline.md)) instead.
- Don't bypass `Game.Shared` by duplicating DTOs/logic in Backend or (eventually) Unity.
- Don't commit generated ship/station sprite art without its source genome and a test that
  regenerates it. Art is data: a genome + the deterministic renderer is the source of truth: see
  [ADR 0006](docs/02-decisions/0006-procedural-indexed-palette-art-pipeline.md).
