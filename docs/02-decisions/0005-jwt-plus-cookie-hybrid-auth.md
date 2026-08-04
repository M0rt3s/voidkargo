# ADR 0005 — JWT (Backend) + Cookie (Website) Hybrid Authentication

- **Status**: Accepted
- **Date**: 2026-08-04

## Context

The game needed its first authentication/authorization layer: `Game.Backend` had no Identity,
no login/register endpoints, and no roles; `Game.Website` had no session mechanism at all. Two
different consumers need to authenticate against `Game.Backend`:

1. **`Game.Website`** (Blazor Server) — a traditional server-rendered portal that needs its own
   session so `[Authorize]` pages, `AuthorizeView`, and server-side redirects work the normal
   ASP.NET Core way.
2. **The Unity client** (eventually) — a native/WebGL client with no browser cookie jar of its
   own, which the documented "Play" flow says receives an auth token directly (see [system
   architecture](../01-architecture/system-architecture.md)).

A single mechanism doesn't cleanly serve both: raw JWT-in-browser (e.g. `localStorage`) is
awkward and risky for a server-rendered Blazor app (XSS exposure, manual attach-to-every-request
plumbing), while a pure cookie session doesn't make sense for the Unity client to carry around.

The project also had **zero EF Core migrations** anywhere before this — `DevelopmentDataSeeder`
was already calling `Database.MigrateAsync()`, but with no migrations that only creates the
`__EFMigrationsHistory` table, not the actual schema. This was a latent gap independent of auth,
fixed as necessary infrastructure alongside this work (see `Data/Migrations/InitialCreate`).

The user requirement was also explicit: on local `Development`, `admin/admin`, `player/player`,
`gamemaster/gamemaster` test accounts must work out of the box (via seeding, not hardcoding) so
authorization levels can be exercised, without weakening anything outside `Development`.

## Decision

- **`Game.Backend`** owns identity and issues tokens: ASP.NET Core Identity with `Guid` keys
  (`UserEntity : IdentityUser<Guid>`, `IdentityRole<Guid>`), three fixed roles (`Admin`,
  `Player`, `GameMaster` — `Game.Shared/Auth/GameRoles.cs`), and a REST surface
  (`Auth/AuthEndpoints.cs`): `POST /api/auth/register` (always grants `Player` only — never
  derived from client input), `POST /api/auth/login` (returns a signed JWT + expiry + profile),
  `GET /api/auth/me` (`[RequireAuthorization]`). Tokens are HMAC-SHA256 signed
  (`Auth/JwtTokenService.cs`) and validated via `AddJwtBearer`.
- **`Game.Website`** keeps its own cookie session (`CookieAuthenticationDefaults`, cookie name
  `voidkargo.auth`) rather than storing the raw JWT client-side. Non-interactive minimal API
  endpoints under `/account/*` (`Endpoints/AccountEndpoints.cs`) call the backend's REST auth
  API, then sign in a cookie principal whose claims include the roles plus the backend JWT
  itself (stashed as a `backend_jwt` claim, never sent to the browser as anything but this
  server-side cookie) — this is what will get forwarded to Unity for the "Play" flow. Cookie
  expiry tracks the JWT's expiry; there is no refresh-token flow yet (a known, intentional gap
  — logging in again is the fallback once a session expires).
- **Why non-interactive endpoints, not Blazor event handlers**: signing in requires writing a
  `Set-Cookie` response header, which can't happen once an interactive Blazor Server circuit's
  response has already started streaming. `Login.razor` / `Register.razor` / `Profile.razor`
  (logout) are plain `<form method="post">` + `<AntiforgeryToken />`, matching the same pattern
  the official ASP.NET Core Identity UI uses for Blazor Server.
- **Fail-fast, environment-gated config** (`Auth/JwtOptionsValidation.cs`,
  `IValidateOptions<JwtOptions>` + `ValidateOnStart()`): the app refuses to start outside
  `Development` if `Jwt:Key` is missing, shorter than 32 characters, or still equals the
  well-known development placeholder value. The Identity password policy is only relaxed inside
  `if (builder.Environment.IsDevelopment())` in `Program.cs`, mirroring the existing pattern
  where `DevelopmentDataSeeder` itself is only registered as a hosted service in `Development`.
  This is the mechanism that makes `admin/admin` etc. work locally without weakening anything
  in a real deployment — there is no hardcoded credential or bypass reachable outside
  `Development`.
- **Seeding, not hardcoding**: `DevelopmentDataSeeder` (already `Development`-gated) creates the
  three roles and three matching test accounts (`admin`/`admin` → `Admin`, `player`/`player` →
  `Player`, `gamemaster`/`gamemaster` → `GameMaster`) idempotently via `RoleManager`/
  `UserManager`, rather than the application code containing any special-cased credential.

## Consequences

- **Easier**: `[Authorize]`/`AuthorizeView`/cookie-based redirects on the Website "just work"
  with standard ASP.NET Core primitives; the Unity client (once it exists) gets a portable JWT
  it can carry itself, matching the already-documented "Play forwards token" design; testing
  all three authorization levels locally requires zero manual setup — `dotnet run --project
  src/Game.AppHost` is enough.
- **Harder**: two different auth representations to reason about (cookie vs. JWT) instead of
  one; no refresh-token flow yet means a website session silently stops being able to make
  authenticated backend calls once the underlying JWT expires (mitigated by matching cookie
  expiry to JWT expiry, but not eliminated) — revisit if the JWT expiry window becomes a real
  usability problem.
- Generating the first-ever EF Core migration (`InitialCreate`) was a prerequisite for the
  seeder to actually create schema, not scope creep — this was a latent pre-existing gap.
- Public self-registration is deliberately limited to the `Player` role; granting `Admin`/
  `GameMaster` requires either the `Development` seeder or a future admin-only management
  surface (not built yet).

## Alternatives considered

- **Cookie auth all the way through to the Unity client** — rejected: WebGL/native clients
  don't have a normal browser cookie jar to rely on, and it would fight the documented
  SignalR/REST split and "Play passes token to Unity" design instead of fitting it.
- **Raw JWT stored client-side in the Website (e.g. `localStorage`, then attached manually to
  every request)** — rejected: meaningfully larger XSS blast radius for a server-rendered app
  that doesn't need it, plus loses the "just works" ASP.NET Core cookie-auth ergonomics
  (`[Authorize]`, `AuthorizeView`, automatic login redirects) for no benefit on this side.
- **A single shared session store (e.g. server-side session cache keyed by a cookie, checked by
  both Website and Backend)** — rejected as unnecessary infrastructure for a solo-dev project
  (would need a new shared cache/store — see `AGENTS.md`'s "no new services without an ADR"
  guidance) when the JWT-issued-by-Backend model already covers the Unity requirement cleanly.
