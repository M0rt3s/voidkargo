# Game.Client (Unity)

The game client itself: all visuals, client-side logic, and UI, written in C# using Unity.
See [ADR 0002](../02-decisions/0002-unity-as-client-engine.md) for why Unity was chosen.

## Status

**Scaffolded.** Unity 6000.5.6f1 (URP) project lives directly in
[`client/Game.Client/`](../../client/Game.Client/). See
[`client/Game.Client/README.md`](../../client/Game.Client/README.md) for how `Game.Shared` is
referenced as a Unity local package and its C# compatibility constraints.

## Responsibilities

- All rendering/visuals for the game map, ships, stations/planets, and UI.
- UI built with Unity **UI Toolkit** (UXML/USS) — deliberately close to HTML/CSS.
- Consumes `Game.Shared` DTOs directly, both for REST responses and for deserializing SignalR
  messages (via the official `Microsoft.AspNetCore.SignalR.Client` package, which works in
  Unity).
- Exports to three targets from one codebase: WebGL, iOS, Android.

## Editor tooling: Foundry

`Assets/Scripts/Editor/Foundry/FoundryWindow.cs` (menu item **VoidKargo > Foundry**) is the
Editor-only window for the procedural pixel-art pipeline described in
[ADR 0006](../02-decisions/0006-procedural-indexed-palette-art-pipeline.md): it loads ship
genomes and palettes from the repo-root [`content/`](../../content/) directory, previews a genome
rendered against any loaded palette, and bakes ship sprites / the combined palette LUT into
`Assets/Art/Generated/...` with the correct (point-filtered, uncompressed, non-sRGB) import
settings for an indexed data texture. See
[`docs/03-modules/game-shared.md`](game-shared.md#content-and-the-foundry-editor-tool) for the
full content-file/testing story — this file only covers the Unity-side tool itself.

It lives directly under `Assets/Scripts/Editor/` (no dedicated `.asmdef`) so it automatically
picks up `Game.Shared`'s `autoReferenced: true` local-package assembly, and only compiles into
the Editor, never a player build, by virtue of Unity's `Editor`-folder convention.

## Networking

Same split as the rest of the system — see
[networking strategy](../01-architecture/networking-strategy.md).

## Constraints to remember

- Unity WebGL builds have a larger initial payload (10-30MB) than pure HTML/JS. Mobile web
  browsers heavily restrict memory — mobile web visitors should be redirected to native
  App Store/Play Store builds rather than served the WebGL build (handled in `Game.Website`).
