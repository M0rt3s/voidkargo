# Design Tokens Reference

Canonical values live in two places that must stay in sync (see
[ADR 0004](../02-decisions/0004-design-language-and-ui-foundation.md)):

- **C#**: `src/Game.Shared/Design/ColorRgb.cs`, `VoidKargoPalette.cs`, `VoidKargoTypography.cs`,
  `VoidKargoMetrics.cs` — used by Backend/Website and (eventually) Unity.
- **CSS**: `src/Game.Website/wwwroot/css/tokens.css` (`--vk-*` custom properties) — used by the
  website, and re-pointed onto Bootstrap's own variables in `bootstrap-bridge.css`.

**Rule:** components consume semantic tokens (`--vk-surface`, `--vk-text`, `--vk-accent`...).
Raw ramp values (`--vk-void-700`, `--vk-ember-500`...) are only referenced when *defining* a new
semantic token, never directly in a component or page.

## Colour

### Ramps

| Ramp | Steps | Use |
|---|---|---|
| **Void** | 900 (bg) → 500 | Backgrounds, from deepest to most-raised surface. |
| **Steel** | 400 → 100 | Borders/hairlines (400/300) and de-emphasised text (200/100). |
| **Bone** | 100, 000 | Body text (100) and emphasis text/headings (000). Never pure white. |
| **Ember** | 600 → 300 | The one accent: pressed → default → hover → focus-ring. |
| Signals | Frost / Moss / Sulfur / Rushnyk | Info / success / warning / danger. Meaning only — never decorative. |

### Semantic aliases

| Token | Resolves to | Use |
|---|---|---|
| `--vk-bg` | Void 900 | Page background. |
| `--vk-surface` | Void 700 | Standard panel fill. |
| `--vk-surface-raised` | Void 600 | Elevated/floating panel fill (dropdowns, popovers). |
| `--vk-surface-inset` | Void 800 | Recessed wells: inputs, code blocks, log viewers. |
| `--vk-border` / `--vk-border-strong` | Steel 400 / 300 | Default hairline / emphasised hairline. |
| `--vk-text` / `--vk-text-strong` | Bone 100 / 000 | Body text / headings & emphasis. |
| `--vk-text-muted` / `--vk-text-dim` | Steel 100 / 200 | Secondary text / tertiary (labels, timestamps). |
| `--vk-accent` / `-hover` / `-pressed` | Ember 500 / 400 / 600 | The one accent — primary actions, active states. |
| `--vk-focus` | Ember 300 | Focus ring colour only. |
| `--vk-info` / `--vk-success` / `--vk-warning` / `--vk-danger` | Frost / Moss / Sulfur / Rushnyk 500 | Status meaning. |

Never write a raw hex value in a `.razor` or `.razor.css` file — every colour need should be
expressible as one of the above.

## Typography

| Token | Value | Use |
|---|---|---|
| `--vk-font-display` | Unbounded | Headings and the wordmark only. |
| `--vk-font-ui` | Inter | Everything read as prose or a control label. |
| `--vk-font-mono` | JetBrains Mono | Every number, ID, coordinate, timer. Always with `.vk-mono`/`.vk-numeric` for tabular figures. |

Scale steps `--vk-fs-100` … `--vk-fs-900` follow a ~1.25 ratio; 400 (1rem/16px) is body, steps
below are UI chrome, steps above are display sizes. Letter-spacing: `--vk-tracking-display`
(-0.02em, tight — for large display type), `--vk-tracking-label` (0.14em, wide — for uppercase
eyebrow labels), `--vk-tracking-normal` (0, everything else).

Fonts are self-hosted (`wwwroot/fonts/`, loaded via `wwwroot/css/fonts.css`) as latin + latin-ext
+ cyrillic + cyrillic-ext woff2 subsets — no third-party font CDN request.

## Spacing

4px base grid: `--vk-space-1` (4px) through `--vk-space-8` (64px). Nothing in the UI should sit
off this grid — if a gap doesn't fit a step, the layout is probably wrong, not the scale.

## Shape, borders, elevation

- **Radius**: `--vk-radius-none` (0), `--vk-radius-sharp` (2px, the default for panels/buttons/
  inputs), `--vk-radius-soft` (4px, hard ceiling — pill badges only).
- **Border**: always `--vk-border-width` (1px). `--vk-hairline` / `--vk-hairline-strong` are the
  ready-made `border` shorthand values.
- **Elevation and glow**: there is no drop-shadow scale. `--vk-shadow-seam` (an inset top
  highlight) is the standard "this is a panel" cue; `--vk-shadow-lift` is reserved for genuinely
  floating layers (modals, the elevated panel variant). `--vk-glow-accent` and `--vk-focus-ring`
  are *state* indicators (live/focus) — never apply a glow as a static style choice.

## Motion

`--vk-duration-instant` (90ms, micro-interactions like checkbox toggles), `-fast` (140ms, hovers/
button states), `-slow` (200ms, panel/overlay transitions — the ceiling). Easing:
`--vk-ease-out` for entrances, `--vk-ease-standard` for everything else. Respects
`prefers-reduced-motion` globally (handled once in `voidkargo.css`; don't re-implement per
component).

## Layout

`--vk-topbar-height` (56px), `--vk-sidebar-width` (260px), `--vk-content-max` (1320px) — shared
fixed metrics so the website chrome and (later) the in-game HUD frame agree on proportions.

## Ornament

`--vk-grid-line` / `--vk-grid-size` drive the near-invisible background survey grid (applied
once, on `body`, in `voidkargo.css`). `--vk-rhombus-line` / `--vk-rhombus-size` drive
`.vk-divider--stitch`. Both are backdrop textures — don't reuse them as foreground decoration.
