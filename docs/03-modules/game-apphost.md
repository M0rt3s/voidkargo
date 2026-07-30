# Game.AppHost

The .NET Aspire orchestration entry point for local development. See
[ADR 0001](../02-decisions/0001-use-net-aspire-for-orchestration.md) for why Aspire was chosen.

## Responsibilities

- Declares and wires up the local resource graph: PostgreSQL, `Game.Backend`, `Game.Website`.
- Provides service discovery and connection strings to the referenced projects automatically.
- Surfaces the Aspire dashboard (logs, traces, resource status) during local dev.

## Related: Game.ServiceDefaults

`Game.ServiceDefaults` is referenced by `Game.Backend` and `Game.Website`; it centralizes
standard Aspire-recommended defaults (OpenTelemetry, health checks, resilience/retry
policies) so those concerns aren't duplicated per project.

## Run

```bash
dotnet run --project src/Game.AppHost
```

See [local dev setup](../04-workflows/local-dev-setup.md) for ports, prerequisites, and what
gets seeded automatically.
