# Game.Client (Unity)

The Unity project scaffolded directly in this folder (`client/Game.Client/`), targeting Unity
**6000.5.6f1** (URP). Unity-generated folders (`Library/`, `Temp/`, `Logs/`, `UserSettings/`,
etc.) are gitignored — see `/.gitignore` (Unity sections scoped to this folder).

See [ADR 0002](../../docs/02-decisions/0002-unity-as-client-engine.md) and
[docs/03-modules/game-client-unity.md](../../docs/03-modules/game-client-unity.md) for the
full rationale and responsibilities.

## Referencing Game.Shared

`Game.Shared` is consumed as a [Unity local package](https://docs.unity3d.com/Manual/upm-ui-local.html)
pointing directly at its source, so it compiles as part of this Unity project — a true
shared source-of-truth rather than a stale copied DLL:

- `Packages/manifest.json` declares `"com.voidkargo.shared": "file:../../../src/Game.Shared"`.
- `src/Game.Shared/package.json` and `src/Game.Shared/Game.Shared.asmdef` make that folder a
  valid Unity package/assembly.
- `src/Game.Shared/Game.Shared.csproj` multi-targets `net10.0;netstandard2.1` (Unity's max API
  compatibility level) so the `dotnet build`/`dotnet test` toolchain keeps working unchanged.
- **Important:** Unity compiles the `.cs` files in `Game.Shared` directly with its own bundled
  compiler — it does **not** read `Game.Shared.csproj` at all, and its compiler is currently
  pinned to **C# 9.0** (visible via `-langversion:9.0` in Unity's Editor log/Bee build args).
  Because of this, source in `Game.Shared` must avoid C# 10+-only syntax (e.g. file-scoped
  namespaces, implicit usings) — use explicit `using` statements and block-scoped namespaces
  instead so the same files compile under both toolchains.
- `bin`/`obj` build output for `Game.Shared` is redirected to `artifacts/Game.Shared/` (via
  `src/Game.Shared/Directory.Build.props`) so Unity's asset importer doesn't pick up stray
  compiled DLLs sitting inside the package folder.

If you add new types to `Game.Shared`, verify they compile cleanly in the Unity Console (no
errors) in addition to `dotnet build` — Unity won't fail loudly the way `dotnet build` does if a
`.csproj` is misconfigured; it just reports the compile errors in its own Console/Editor log
(`client/Game.Client/Logs/Editor.log`).

## Setup notes

- Add the `Microsoft.AspNetCore.SignalR.Client` NuGet package to the Unity project (via
  NuGetForUnity or manual DLL install) for real-time server pushes.
- UI Toolkit (UXML/USS) is the intended UI framework.
- Build targets: WebGL, iOS, Android.

## Networking

Follow [docs/01-architecture/networking-strategy.md](../../docs/01-architecture/networking-strategy.md):
SignalR for server-initiated real-time updates, REST for client-initiated actions.
