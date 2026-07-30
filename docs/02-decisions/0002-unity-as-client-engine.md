# ADR 0002 — Unity as the Client Engine

- **Status**: Accepted
- **Date**: 2026-07-30

## Context

The game needs a real-time, smoothly-animated map/canvas (trains moving, economy updates)
across browser, iOS, and Android, while keeping the entire team (a solo C#/.NET developer)
inside one language and ecosystem for maximum code reuse and velocity.

Blazor WebAssembly, MAUI, and Three.js were all considered first, since they'd keep things
closer to "just the web".

## Decision

Use **Unity** as the client engine, written entirely in C#, exported three ways from a single
codebase: WebGL (browser), iOS native, and Android native. UI is built with Unity's **UI
Toolkit** (UXML/USS), which is deliberately close to HTML/CSS and easy to pick up for anyone
with web front-end experience.

## Consequences

- **Easier**: one client codebase for all three targets; native performance for the real-time
  map instead of DOM/Canvas manipulation via interop; UI authoring feels familiar (HTML/CSS-like).
- **Harder**: Unity WebGL has a larger initial download payload (10-30MB) than a pure HTML/JS
  game. Mobile web browsers (especially Safari) heavily restrict memory for WebGL content —
  the plan is to detect mobile web visitors and redirect them to the native App Store /
  Play Store apps instead of running WebGL on mobile browsers.
- Unity licensing/tooling becomes a dependency for anyone working on the client.

## Alternatives considered

- **Blazor WebAssembly** for the game canvas: WASM cannot talk to the DOM/WebGL directly, so
  updating a canvas game map at game-loop frequency would require blasting thousands of JS
  interop calls per second — causing stuttering and killing frame rate. Rejected for the game
  client (still used for `Game.Website`, the non-realtime portal).
- **MAUI**: good for native mobile shells, but doesn't solve the WebGL/browser rendering
  problem, and doesn't offer a unified high-performance canvas/game-loop story.
- **Three.js**: would break "stay 100% in C#" and require a second tech stack/skillset.
- **Godot 4**: ruled out because it does not currently support exporting C# projects to WebGL.
