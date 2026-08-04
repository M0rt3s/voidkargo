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

- **`VkButton`** (optional wrapper, or just document Bootstrap classes directly): confirm
  `.btn-primary` / `.btn-secondary` / `.btn-outline-primary` / `.btn-danger` cover every case
  before building a wrapper component. Likely no wrapper needed — Bootstrap's own `<button
  class="btn ...">` markup is already themed via `bootstrap-bridge.css`.
- **Auth forms** (login / register): use `.vk-panel` as the card, Bootstrap `.form-control` /
  `.form-label` (already themed) for fields. Centre the panel in a max-width column (~420px) —
  don't stretch auth forms to `--vk-content-max`.
- **`VkAlert` / inline form errors**: a `.vk-panel` variant with a left accent bar in
  `--vk-danger` / `--vk-warning` / `--vk-success` / `--vk-info` (2–3px `border-left`, using the
  existing hairline as the base and just recolouring the left edge — don't invent a new shadow
  or radius for this).
- **Empty states**: centred `.vk-label` + short copy + one action button, inside a
  `.vk-panel--inset`. Used for "no leaderboard data yet", "no forum posts yet", etc.

## Priority 2 — leaderboards & forums (the stated `Game.Website` responsibilities)

- **`VkTable`** or just themed `<table class="table">`: Bootstrap's table is already retokenised
  in `bootstrap-bridge.css` (tabular-nums, uppercase label-style headers). Check it renders well
  with real leaderboard data (rank, player, score columns) before adding anything custom — likely
  only needs a `.vk-numeric` class on numeric `<td>`s.
- **Rank badge**: top-3 leaderboard rows get a small badge using `.vk-label--accent` for #1 and
  `.vk-text-muted` for #2/#3. Do not invent gold/silver/bronze colours — that breaks the
  one-accent rule; distinguish rank #1 by weight/accent only.
- **Pagination**: Bootstrap's `.pagination` component, retokenise it in
  `bootstrap-bridge.css` the same way `.btn`/`.card` were done (it isn't covered yet — check
  `--bs-pagination-*` variables).
- **Forum thread list / thread view**: `.vk-panel` per thread row in a list; `.vk-stack` for
  post bodies in a thread view. Post metadata (author, timestamp) is `.vk-label vk-label--dim`.
- **User avatar/identity chip**: circular image (this is the one place `border-radius: 50%` is
  correct — avatars are a deliberate exception to the sharp-corner rule) + username in
  `--vk-text-strong` + optional rank/role tag as a small `.badge`.

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
