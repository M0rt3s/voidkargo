# Game.Shared

A plain C# class library referenced **verbatim** by both `Game.Backend` and the Unity
`Game.Client`. This is the single source of truth for the wire contract and core game rules.

## Contains

- DTOs (SignalR/REST payloads).
- Game math (economy formulas, movement/timing calculations).
- Grid/map logic shared between server simulation and client rendering.
- Validation rules that both server (authoritative) and client (optimistic UI) need to agree on.
- `Design/` — canonical UI design tokens (colour, typography, spacing/motion metrics). The
  single source of truth for the "Void & Ember" visual identity, mirrored by the website's
  `wwwroot/css/tokens.css` and consumed directly by Unity once its UI is built. See
  [ADR 0004](../02-decisions/0004-design-language-and-ui-foundation.md) and
  [docs/05-design](../05-design/design-language.md).
- `Art/` — the deterministic, genome-driven procedural pixel-art generator for ships and
  stations. No Unity/ASP.NET Core dependency, so both `Game.Backend` tooling and the Unity
  Editor's future bake step ("Foundry", tracked separately) can call it. See
  [ADR 0006](../02-decisions/0006-procedural-indexed-palette-art-pipeline.md) for the full
  design; the module currently contains:
  - `Rng/Pcg32.cs` — the dependency-free deterministic PRNG (PCG XSH-RR) every other pass uses
    instead of `System.Random`, plus a per-pixel order-independent hash (`HashToUnit`).
  - `Json/JsonValue.cs` — a minimal, dependency-free JSON model/parser/writer (no
    `System.Text.Json`, which isn't inbox in `netstandard2.1`) used to (de)serialize genomes.
  - `Genome/` — `ShipGenome` and its component records (canvas, silhouette/spine, modules,
    greebles, palette zone map), `GenomeJson` for JSON (de)serialization, and `GenomeValidator`,
    the style linter (class-specific thickness ranges, module-vs-epoch gating, zone-map
    completeness, canvas-size constraint).
  - `Palette/` — `Palette` (a 16-colour indexed row), `PaletteJson` for JSON (de)serialization
    (mirrors `GenomeJson`), `PaletteValidator`, which checks contrast against the game's dark
    backdrop and pairwise separability under simulated protanopia/deuteranopia/tritanopia (a
    deliberately pragmatic, not clinically exact, approximation — see the accessibility goal in
    the top-level plan), and `PaletteLutBaker`, which lays out multiple palettes into a single
    row-per-palette RGBA image — the shared LUT texture a future URP shader samples so a
    faction/cosmetic skin is a data change (a LUT row), not a new texture asset.
  - `Canvas/IndexedCanvas.cs` — the palette-index/glow/alpha pixel-plane buffer sprites are
    rendered into, plus the nearest-neighbour integer upscale used to reach the final texture size.
  - `Rendering/ShipRenderer.cs` — the full deterministic pass pipeline (spine/mass stamping,
    module placement, mirroring, morphological cleanup, distance-transform shading with ordered
    dithering, greebles, emissive marking, outline, wear) and `ValidationResult`, the shared
    result type both validators return.
  - `Encoding/PngEncoder.cs` (+ `Crc32`/`Adler32` helpers) — a minimal PNG writer with no
    `System.Drawing`/ImageSharp dependency, so the same code runs under Unity's C# 9 toolchain
    and the `dotnet` SDK. Supports both indexed-canvas sprites (`Encode`) and arbitrary raw RGBA
    buffers (`EncodeRgba`, used by `PaletteLutBaker`).

  Content files (genomes, palettes) live outside this project, in the repo-root
  [`content/`](../../content/) directory — see "Content and the Foundry editor tool" below.

  Not yet built: the `PixelPalette` URP shader that samples the index/glow planes against a
  palette LUT at render time, and the bitmap hull-number decal font (the wear pass currently
  covers the "worn" look on its own — see the MVP-scope remark on `ShipRenderer`).

## Content and the Foundry editor tool

Ship genomes and palettes are authored as plain JSON files in the repo-root `content/` directory,
**not** inside Unity's `Assets/`, since both `Game.Backend` tooling and the Unity Editor need to
read the same files and raw JSON has no reason to be bundled into a Unity build:

- [`content/ship-genomes/*.json`](../../content/ship-genomes/) — one file per `ShipGenome`,
  file name (minus `.json`) matching both `ShipGenome.Id` and the corresponding
  `ShipTypeDto.Id`/`ShipTypeEntity.Id` in `Game.Backend`.
- [`content/palettes/*.json`](../../content/palettes/) — one file per `Palette`, file name
  matching both `Palette.Id` and the owning faction's `FactionDto.PaletteId`.

Every file here is exercised by `tests/Game.Shared.Tests/Art/ContentTests.cs`, which discovers
all genome/palette files, parses, validates, renders, and encodes each of them — the
"regenerating test" AGENTS.md requires for any generated art committed to the repo. A genome or
palette that fails validation fails `dotnet test`, without needing Unity installed.

The Unity-side "Foundry" editor window
(`client/Game.Client/Assets/Scripts/Editor/Foundry/FoundryWindow.cs`, menu item
**VoidKargo > Foundry**) reads the same `content/` files via plain `System.IO` (resolving the
repo root by walking up from `Application.dataPath` until it finds `voidkargo.slnx`), lists every
genome/palette with pass/fail validation status, previews a genome rendered against any loaded
palette, and bakes:

- Ship sprites to `Assets/Art/Generated/Ships/<genome id>.png`.
- The combined palette LUT to `Assets/Art/Generated/PaletteLut.png` (one row per valid palette).

Baked textures are configured (point filter, uncompressed, no mipmaps, `sRGBTexture = false`)
since they encode data (palette index / glow / alpha), not colour to display directly, and are
committed to git alongside their source genome/palette and the `ContentTests` coverage.

**Verification note:** `FoundryWindow.cs` was authored without a local Unity install available in
this environment (no editor version was installed under Unity Hub) — its `EditorWindow`/
`AssetDatabase`/`TextureImporter` usage could only be reviewed, not compiled. Open the project in
Unity 6000.5.6f1 and check the Editor Console after pulling this change; if anything doesn't
compile, that's expected to need a follow-up fix.

## Rules

- **No dependencies on ASP.NET Core, EF Core, or Unity-specific APIs.** This project must
  compile and run unmodified inside a Unity project — keep it to plain .NET/C#.
- **Targets `net10.0;netstandard2.1`.** The `netstandard2.1` leg exists because Unity is
  referenced as a [local package](https://docs.unity3d.com/Manual/upm-ui-local.html) pointing
  directly at this folder's source (see
  [`client/Game.Client/README.md`](../../client/Game.Client/README.md)) — Unity ignores this
  project's `.csproj` and compiles the `.cs` files itself with its own compiler, currently
  pinned to **C# 9.0**. Avoid C# 10+-only syntax here (file-scoped namespaces, implicit
  usings) — use explicit `using` statements and block-scoped namespaces so the same files
  compile under both the `dotnet` SDK and Unity.
- Treat public types here as a **contract**: changing a DTO shape affects `Game.Backend` and
  the Unity client simultaneously. Check both before merging, and update
  [data model](../01-architecture/data-model.md) / [glossary](../00-overview/glossary.md) if
  you introduce new domain concepts.

## Run / test

```bash
dotnet test tests/Game.Shared.Tests
```
