# Game.Client (Unity)

This folder is a placeholder. The Unity project itself is **not scaffolded here** — Unity
projects can't be generated headlessly from the CLI in this environment; they need the Unity
Editor installed locally.

See [ADR 0002](../../docs/02-decisions/0002-unity-as-client-engine.md) and
[docs/03-modules/game-client-unity.md](../../docs/03-modules/game-client-unity.md) for the
full rationale and responsibilities.

## Setup (when you're ready to start client work)

1. Install the Unity Editor (use a recent LTS release with WebGL, iOS, and Android build
   support modules installed).
2. Create a new Unity project (3D, URP or built-in — decide based on performance testing) at
   this path: `client/Game.Client/`.
3. Add the Unity-specific `.gitignore`/`.gitattributes` entries already prepared at the repo
   root (see `/.gitignore` and `/.gitattributes` — Unity sections are scoped to this folder).
4. Reference `Game.Shared` from the Unity project:
   - Simplest: add `src/Game.Shared/Game.Shared.csproj`'s compiled output, or better, use a
     [Unity local package](https://docs.unity3d.com/Manual/upm-ui-local.html) pointing at the
     `Game.Shared` source so it compiles as part of the Unity project (keeps it a true shared
     source-of-truth rather than a stale copied DLL).
5. Add the `Microsoft.AspNetCore.SignalR.Client` NuGet package to the Unity project (via
   NuGetForUnity or manual DLL install) for real-time server pushes.
6. Set up UI Toolkit (UXML/USS) as the UI framework.
7. Configure build targets: WebGL, iOS, Android.

## Networking

Follow [docs/01-architecture/networking-strategy.md](../../docs/01-architecture/networking-strategy.md):
SignalR for server-initiated real-time updates, REST for client-initiated actions.

## Once scaffolded

Update this README and
[docs/03-modules/game-client-unity.md](../../docs/03-modules/game-client-unity.md) to reflect
the actual folder layout, Unity version pinned, and any project-specific conventions.
