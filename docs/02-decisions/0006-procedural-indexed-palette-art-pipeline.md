# ADR 0006 — Procedural Indexed-Palette Art Pipeline for Ships and Stations

- **Status**: Accepted
- **Date**: 2026-08-04

## Context

The game's design calls for roughly 20 unique ship types per `ShipClass` (`LightHauler`,
`MediumHauler`, `HeavyHauler`) across multiple factions and research-tree epochs, plus
station/planet/Dyson-sphere art per faction, plus a cosmetics economy where players can later
buy or unlock repaints of the same hull. Hand-drawing that volume of pixel art (dozens of base
designs × several factions × an open-ended number of cosmetic recolours) is not viable for a
solo/small team, but the visual bar is high: dark, "Slavic-punk/high-tech" (Expanse-adjacent),
readable at a glance, with class silhouettes that communicate role (fast/light vs.
versatile/medium vs. slow/heavy) even before a player reads a tooltip.

LLM agents are good at generating and varying *symbolic, structured* content (JSON, C#) but are
not a reliable source of pixel-perfect, style-consistent image data — asking a model to emit
raw pixels (or to call an image-generation API per ship) gives inconsistent style, no
reproducibility, no palette-swap story, and a runtime/licensing dependency this project doesn't
want. Cosmetics ("repaint parts of it using palettes handed out or sold") also need to be cheap
to produce and cheap to ship (WebGL payload size matters — see
[ADR 0002](0002-unity-as-client-engine.md)) — a naive "one texture per skin" approach doesn't
scale to a real cosmetics catalog.

Accessibility is also a stated goal (colour-blind players, contrast, reduced motion), which is
far cheaper to bake into a generator's validation rules than to retrofit onto hand-authored art
later.

## Decision

Build a **deterministic, genome-driven procedural art pipeline**, living in `Game.Shared/Art`
(no Unity or ASP.NET Core dependency, usable by both `Game.Backend` tooling and the Unity
client/editor):

1. **LLM agents author a *genome*, never pixels.** A genome is a small, schema-validated JSON
   document describing a ship or station symbolically: silhouette spine/thickness, a list of
   modules (engine, cargo, sensor, radiator, …) with anchors and sizes, greeble density, palette
   *zone* assignments (hull/trim/accent/glass/emissive/outline — roles, not colours), wear, and
   a seed. `ShipTypeDto.Id` (see `Game.Shared/Dtos/GameDtos.cs`) is the same key used to look up
   a ship's genome, so game balance data and its art share one identity.
2. **A deterministic C# renderer turns a genome into pixels — same genome + seed always
   produces byte-identical output.** Work at a 64×64 logical grid (spine/mass stamping →
   module placement → mirror with a small deliberate asymmetry budget → morphological
   close/prune → distance-transform-based shading quantized to a palette ramp with ordered
   (Bayer) dithering → greebles → emissive mask → outline → wear/decals → validation), then
   integer-upscale ×4 to a 256×256 texture. A minimal custom PNG encoder ships inside
   `Game.Shared` (no `System.Drawing`/ImageSharp dependency) so the same code runs identically
   under Unity's C# 9 toolchain and the `dotnet` SDK.
3. **Textures are palette-indexed, not pre-baked RGB.** The rendered texture stores a palette
   *index* (0–15) per pixel plus alpha/coverage and a separate emissive/glow plane — it does not
   store final colours. A **palette** is a 16-colour row in a shared LUT texture; a URP shader
   samples the index texture and looks up the LUT row at render time (see the palette shader
   phase, tracked separately). Re-skinning a hull for a different faction or selling a cosmetic
   is then a ~64-byte palette row, not a new texture — this is also what makes "repaint parts of
   it" (zone-scoped recolouring) fall out for free instead of needing extra mask textures.
4. **Art is baked at edit time, not generated at runtime.** An in-editor tool (tracked
   separately as the "Foundry" editor window) loads genomes, renders and validates them, and
   bakes an atlas + palette LUT that ships with the build. The client never runs the generator
   live — this keeps WebGL payload/memory and determinism predictable and avoids giving players
   any way to influence generation at runtime.
5. **Validation is enforced as part of the pipeline, including accessibility.** Every genome is
   checked against a style linter (per-class thickness ranges, module legality per epoch) before
   it renders, and every palette is checked for adjacent-ramp contrast and separability under
   simulated protanopia/deuteranopia/tritanopia before it's accepted. These are unit tests in
   `Game.Shared.Tests`, not manual review steps.
6. **Determinism rules**: the renderer uses a dependency-free deterministic PRNG (not
   `System.Random`, which is not guaranteed stable across runtimes) and avoids
   culture-dependent parsing/formatting, so the same genome produces the same PNG bytes on the
   server, in Unity, and in CI.

## Consequences

- **Easier**: ~20 ship types × multiple factions × an open-ended cosmetics catalog becomes
  tractable for a solo/small team — new content is a JSON genome or a 16-colour palette, not a
  new hand-painted asset. Faction identity, cosmetics, and "repaint on click" are the same
  mechanism (swap a palette row), which keeps that whole feature surface simple. Accessibility
  (contrast, colour-blind safety) is enforced automatically instead of relying on manual review.
  The generator, its validation, and its tests are pure C# in `Game.Shared` — squarely in the
  part of the stack LLM coding agents are strongest at, and reusable by both `Game.Backend` and
  Unity without duplication.
- **Harder**: the renderer itself (silhouette generation, module placement, shading/dithering,
  PNG encoding) is nontrivial code that has to be built and tuned before any art appears, and
  "does this look good" is still a human/aesthetic judgement call no amount of validation
  automates away — the linter/contrast checks catch *broken* output, not *mediocre* output.
  Committed art now depends on committed genomes: a sprite without a genome + a regenerating
  test is effectively an orphaned, unreproducible asset (see the corresponding rule added to
  `AGENTS.md`). The custom PNG encoder is one more piece of low-level code to maintain instead
  of depending on a battle-tested imaging library — accepted deliberately to avoid a
  `System.Drawing`/ImageSharp dependency that Unity's C# 9 toolchain and WebGL target can't
  cleanly share.

## Alternatives considered

- **LLM generates raw pixels or calls an external image-generation API per ship/skin**:
  rejected — inconsistent style across generations, no reproducibility (can't regenerate the
  same asset later), no natural palette-swap/cosmetics story, and adds a runtime API dependency,
  cost, and licensing question this project doesn't want.
- **Hand-drawn pixel art for all ship types/factions/cosmetics**: rejected as a scope reality —
  not viable for a solo/small team at the stated content volume (~20 types × 3 classes ×
  multiple factions × an open-ended cosmetics catalog), and doesn't solve the "let players
  repaint parts of it" requirement without a palette-indexed approach anyway.
- **Full-colour (non-indexed) generated textures, one bake per faction/cosmetic skin**: works
  visually but doesn't scale a cosmetics economy — every skin becomes a full texture (storage
  and WebGL download cost) instead of a few bytes, and "repaint parts of it" would need extra
  per-zone mask textures that indexed rendering gives away for free.
- **Runtime generation on the client**: rejected — adds WebGL CPU/memory cost, reintroduces
  cross-platform determinism risk (must match server-baked assets used for e.g. anti-cheat or
  shared IDs), and gives up the ability to hand-curate/approve genomes before they ship.
