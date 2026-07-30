# Game.Client (Unity)

The game client itself: all visuals, client-side logic, and UI, written in C# using Unity.
See [ADR 0002](../02-decisions/0002-unity-as-client-engine.md) for why Unity was chosen.

## Status

**Not yet scaffolded.** This is a placeholder — a Unity project can't be generated headlessly
via the .NET CLI; it needs the Unity Editor installed locally. See
[`client/Game.Client/README.md`](../../client/Game.Client/README.md) for setup steps.

## Responsibilities (once scaffolded)

- All rendering/visuals for the game map, trains, and UI.
- UI built with Unity **UI Toolkit** (UXML/USS) — deliberately close to HTML/CSS.
- Consumes `Game.Shared` DTOs directly, both for REST responses and for deserializing SignalR
  messages (via the official `Microsoft.AspNetCore.SignalR.Client` package, which works in
  Unity).
- Exports to three targets from one codebase: WebGL, iOS, Android.

## Networking

Same split as the rest of the system — see
[networking strategy](../01-architecture/networking-strategy.md).

## Constraints to remember

- Unity WebGL builds have a larger initial payload (10-30MB) than pure HTML/JS. Mobile web
  browsers heavily restrict memory — mobile web visitors should be redirected to native
  App Store/Play Store builds rather than served the WebGL build (handled in `Game.Website`).
