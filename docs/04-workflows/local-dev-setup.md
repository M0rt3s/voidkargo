# Local Dev Setup

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/) — version pinned in [`global.json`](../../global.json).
- [Docker](https://www.docker.com/) (or another OCI-compatible container runtime) — .NET
  Aspire uses it to run PostgreSQL locally.
- (Later, for client work) Unity Editor — see
  [`client/Game.Client/README.md`](../../client/Game.Client/README.md).

## First-time setup: JWT signing key (Development secret)

`Game.Backend` needs a `Jwt:Key` config value to sign tokens (see [ADR
0005](../02-decisions/0005-jwt-plus-cookie-hybrid-auth.md)). This is a secret, so it's **not**
committed in `appsettings.Development.json` — set it once via the .NET Secret Manager, which
stores it outside the repo (`~/.microsoft/usersecrets/`, gitignored by the tool itself, not
this repo's `.gitignore`):

```bash
dotnet user-secrets set "Jwt:Key" "$(openssl rand -base64 48)" --project src/Game.Backend
```

Any sufficiently long random string works — it only needs to be stable across your own local
runs (and ≥32 characters; see `JwtOptionsValidation`). Without this, `Game.Backend` fails fast
on startup with a clear error rather than silently running with a weak/missing key.

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

## Test accounts for auth (Development only)

The same `Development`-only seeder also creates the `Admin`, `Player`, and `GameMaster` roles
and one test account per role, so you can log in at `/login` (via `Game.Website`) and exercise
each authorization level immediately:

| Username     | Password       | Role         |
|--------------|----------------|--------------|
| `admin`      | `admin`        | `Admin`      |
| `player`     | `player`       | `Player`     |
| `gamemaster` | `gamemaster`   | `GameMaster` |

These accounts and their weak passwords **only exist/work in `Development`**:
`DevelopmentDataSeeder` (`Game.Backend/Data/Seed/`) is only registered as a hosted service when
`IsDevelopment()`, and the Identity password policy is only relaxed (allowing short,
no-digit/uppercase/symbol passwords) inside that same `IsDevelopment()` check in
`Game.Backend/Program.cs`. Outside Development, `admin`/`admin` etc. would simply fail the real
password policy — there is no hardcoded backdoor. See [ADR
0005](../02-decisions/0005-jwt-plus-cookie-hybrid-auth.md) for the full auth architecture and
`Game.Backend/Auth/JwtOptionsValidation.cs` for the matching fail-fast guard on `Jwt:Key` (the
app refuses to start outside Development if it's still the development placeholder value or
too short).

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

## Troubleshooting: Postgres `password authentication failed`

If `Game.Backend` crashes on startup with `Npgsql.PostgresException: 28P01: password
authentication failed for user "postgres"`, the Postgres **container**'s data volume is out of
sync with the **password** Aspire is currently generating/using. This happens because
`Game.AppHost` uses `.WithDataVolume()` for Postgres (a named Docker volume that persists across
runs), but Postgres only applies `POSTGRES_PASSWORD` the *first* time it initializes an empty
data directory — if the volume already has an initialized database from an earlier run (e.g.
with a different `Parameters:postgres-password` from `dotnet user-secrets` in
`src/Game.AppHost`, perhaps after a fresh clone or machine change), the container keeps the old
password and rejects the new one.

Fix: stop the AppHost, then remove the stale volume so Postgres reinitializes fresh (all seeded
data, including the [test accounts](#test-accounts-for-auth-development-only), is regenerated
automatically on next start — nothing is lost):

```bash
docker volume ls | grep postgres-data   # find the game.apphost-*-postgres-data volume(s)
docker volume rm <volume-name>
dotnet run --project src/Game.AppHost
```
