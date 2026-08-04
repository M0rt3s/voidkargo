# Component Inventory

The foundation (tokens, primitives, layout shell, home page) is built. This is the punch list of
components still needed for a real website, in priority order. Each entry says what it is, which
existing primitives/tokens to build it from, and any behavioural notes. None of these need new
design decisions — they're assembly work on top of `docs/05-design/design-tokens.md` and
`design-language.md`.

Build new components as Blazor components under `Components/Ui/` (one `.razor` +
`.razor.css` pair each, matching the existing pattern in `Components/Layout/`). Reuse
`.vk-panel`, `.vk-label`, `.vk-stat`, `.vk-status`, `.vk-hud`, `.vk-mono` rather than writing new
CSS classes where one of these already fits.

## Priority 1 — needed for auth/account pages to exist at all

- **`VkButton`** — ✅ confirmed, no wrapper built. `.btn-primary` / `.btn-secondary` /
  `.btn-outline-primary` / `.btn-danger` cover every case via Bootstrap's own `<button
  class="btn ...">` markup, themed in `bootstrap-bridge.css`.
- **Auth forms** (login / register) — not built yet. There's no auth backend (no Identity, no
  login/register endpoints) to wire a real form to, so building the markup now would just be
  thrown away. When the backend lands: use `.vk-panel` as the card, Bootstrap `.form-control` /
  `.form-label` (already themed) for fields, centred in a max-width column (~420px) — don't
  stretch auth forms to `--vk-content-max`. `VkAlert` (below) is ready for the inline error case.
- **`VkAlert`** — ✅ built: `Components/Ui/VkAlert.razor` (+ `.razor.css`,
  `VkAlertVariant.cs`). A `.vk-panel` with a 3px `border-left` accent in
  `--vk-danger` / `--vk-warning` / `--vk-success` / `--vk-info`; `Variant`, optional `Title`,
  `ChildContent` parameters. See `/dev/styleguide` for a rendered example of every variant.
- **Empty states** — ✅ built: `Components/Ui/VkEmptyState.razor`. `.vk-panel--inset` well with
  `Eyebrow` (uppercase label), `Message` copy, and an optional `ActionContent` render fragment
  for the single call-to-action button.

## Priority 2 — leaderboards & forums (the stated `Game.Website` responsibilities)

- **`VkTable`** — ✅ no new component needed, confirmed. Themed `<table class="table">` covers
  leaderboard data as-is; numeric columns get a `.vk-numeric` class on both `<th>` and `<td>`,
  which now also right-aligns them (`bootstrap-bridge.css`).
- **Rank badge** — ✅ built: `Components/Ui/VkRankBadge.razor`. Plain mono numeral; `#1` gets
  `--vk-accent` + bold weight, `#2`/`#3` stay `--vk-text-muted`. No gold/silver/bronze colours.
- **Pagination** — ✅ retokenised in `bootstrap-bridge.css` (`--bs-pagination-*` variables,
  matching the `.btn`/`.card` treatment) and wrapped in `Components/Ui/VkPagination.razor`
  (`CurrentPage`/`TotalPages`/`WindowSize` params, `CurrentPageChanged` callback; renders a
  windowed page range rather than every page).
- **Forum thread list / thread view** — thread *list* row is built:
  `Components/Ui/VkThreadRow.razor` (`.vk-panel` per row, title as the primary link, metadata —
  author via `VkIdentityChip`, timestamp, reply count — as a single muted line). The full thread
  *view* (post-by-post `.vk-stack`) isn't built yet — no forum data model exists to shape it
  against; build it once posts/replies have a concrete DTO in `Game.Shared`.
- **User avatar/identity chip** — ✅ built: `Components/Ui/VkIdentityChip.razor`. Circular
  avatar (`border-radius: 50%`, the deliberate exception to the sharp-corner rule) +
  `--vk-text-strong` username + optional `Tag` rendered as a small `.badge`. Ships with a
  placeholder avatar at `wwwroot/img/avatar-placeholder.svg` for when no image URL is known yet.

All Priority 1/2 components above live under `Components/Ui/` (one `.razor` + `.razor.css` pair
each) and are rendered together at `/dev/styleguide` (not linked from `NavMenu`) for quick visual
QA — open it after touching any of these files.

## Priority 3 — the "Play" entry point

- **Play/launch panel**: replace the disabled placeholder buttons in `MainLayout.razor` /
  `Home.razor` (`title="The Unity client is not wired up yet"`) once the Unity WebGL build
  exists. Loading state should reuse `.vk-status--live` styling (a "spinning up" status) rather
  than a spinner graphic.
- **Mobile redirect screen** (per [ADR 0002](../02-decisions/0002-unity-as-client-engine.md)):
  a full `.vk-panel--elevated`, centred, explaining that mobile web visitors should use the
  native app, with store badges. Keep copy short — this is a redirect, not a landing page.

## Priority 4 — nice-to-have polish

- **Toasts/notifications**: Bootstrap's `.toast`, retokenised, positioned bottom-right,
  `--vk-duration-slow` transition in/out.
- **Breadcrumb**: `.vk-label` styling applied to Bootstrap's `.breadcrumb`, `/` separators
  restyled as thin chevrons or kept as-is if legible.
- **Command palette / search** (optional, very on-brand for "technical, Jarvis-like" but not a
  stated requirement): a `.vk-panel--elevated` modal with a mono input and `.vk-label` result
  groups. Don't build this speculatively — only if a real search/command need shows up.

## Explicitly out of scope for now

- Dark/light theme toggle. The product is dark-only by design (`data-bs-theme="dark"` is
  hardcoded in `App.razor`); a light mode is not part of the brief and would double the QA
  surface for no stated benefit. Revisit only if accessibility feedback demands it.
- Any Unity/USS work — `Game.Client` isn't scaffolded with UI yet. When it is, start from
  `Game.Shared/Design/*.cs` directly rather than re-deriving values from the CSS.
