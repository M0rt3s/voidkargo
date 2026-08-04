# ADR 0004 — Design Language and UI Foundation

- **Status**: Accepted
- **Date**: 2026-08-04

## Context

The website was still the stock Blazor Server template (default Bootstrap theme, "Hello,
world!" home page). We need a real visual identity — one that:

- Reads as "high-tech Slavic logistics command", not a generic sci-fi skin.
- Feels the same on the website and, eventually, in the Unity client's in-game UI (HUD,
  menus). A player should never feel like they left the brand when they click "Play".
- Is maintainable by a solo/small team: no bespoke design system to hand-roll and keep in
  sync by memory, no fighting a framework that actively disagrees with the target look.

`Game.Website` already depends on Bootstrap 5.3 (see its default project template). The
question was whether to keep it, replace it, or something in between.

## Decision

**Keep Bootstrap 5.3 as the layout/behaviour engine. Move all visual identity into a token
layer that re-points Bootstrap's own CSS custom properties.**

Concretely:

1. **Canonical tokens live in `Game.Shared/Design/`** (`ColorRgb`, `VoidKargoPalette`,
   `VoidKargoTypography`, `VoidKargoMetrics`) — plain C# constants/structs with no framework
   dependency. This is the single source of truth for colour, type and spacing, consumable by
   both `net10.0` (Backend/Website) and `netstandard2.1` (Unity), matching how `Game.Shared`
   already works for DTOs and game math.
2. **The web mirrors those tokens as CSS custom properties** in
   `wwwroot/css/tokens.css` (`--vk-*` namespace). Bootstrap 5.3 already exposes its entire
   palette/shape/typography as `--bs-*` variables, so `wwwroot/css/bootstrap-bridge.css`
   re-points those at `--vk-*` instead of overriding Bootstrap's compiled component CSS. Bootstrap
   itself is never modified or rebuilt from Sass.
3. **Everything Bootstrap doesn't provide** (base element styling, the type ramp, and a small
   set of HUD primitives — `.vk-panel`, `.vk-label`, `.vk-stat`, `.vk-status`, `.vk-hud` corner
   brackets, `.vk-divider--stitch`) lives in `wwwroot/css/voidkargo.css`.
4. The design language itself — "Void & Ember" — is documented in
   [`docs/05-design/design-language.md`](../05-design/design-language.md); the full token
   reference is in [`docs/05-design/design-tokens.md`](../05-design/design-tokens.md).

When the Unity client is scaffolded, it reads `VoidKargoPalette`/`VoidKargoTypography`/
`VoidKargoMetrics` directly from `Game.Shared` (e.g. `new Color32(p.R, p.G, p.B, p.A)` from
`ColorRgb`) and expresses the same primitives as UI Toolkit USS, generated or hand-written from
the same constants. There is intentionally no shared *CSS* file between web and Unity — USS and
CSS are similar but not compatible — but there is exactly one shared *source of truth*.

## Consequences

- **Easier**: the web keeps Bootstrap's grid, breakpoints, focus/ARIA states, and interactive
  components (dropdowns, modals, offcanvas) for free — no reason to reinvent responsive layout.
  Changing the brand (e.g. retuning the ember hex) is a one-line change in `tokens.css` (web)
  and the matching constant in `Game.Shared` — both surfaces move together.
- **Harder**: two Bootstrap quirks require workarounds, now documented inline in
  `bootstrap-bridge.css`:
  - Button variants (`.btn-primary`, etc.) resolve colour from per-variant `--bs-btn-*`
    variables baked in at Sass-compile time, not from `--bs-primary` — so each variant is
    re-declared explicitly rather than inherited automatically.
  - Some newer Bootstrap variables (`--bs-border-radius-xl/2xl`) don't exist in 5.3's default
    build; they're set anyway for forward-compatibility and currently have no effect.
- Two token stores (`Game.Shared/Design/*.cs` and `tokens.css`) must be changed together. This
  is an accepted, documented trade-off rather than a build-time codegen step, to avoid adding a
  code-generation pipeline before there's a second consumer (Unity) to justify it. Revisit if
  drift becomes a real problem.
- No dependency was added — Bootstrap was already present, and this is a smaller footprint than
  most alternatives.

## Alternatives considered

- **Replace Bootstrap with a hand-rolled CSS layer.** Rejected as premature: the requester is
  primarily a backend developer, and a fully custom system means hand-implementing responsive
  grid, focus management and all interactive components — exactly the "reinventing the wheel"
  outcome we were asked to avoid.
- **Adopt a heavier design-system/component library** (e.g. a full Tailwind rebuild, a
  commercial admin template). Rejected: adds a build toolchain (PostCSS/Tailwind CLI) and a
  dependency footprint disproportionate to a solo project, and fights the "not too shiny,
  minimalistic" brief harder than plain CSS variables do.
- **Duplicate design tokens directly in Unity (no shared C# source).** Rejected: violates the
  existing "`Game.Shared` is a contract" convention (see `AGENTS.md`) and guarantees the two
  clients drift apart over time.
