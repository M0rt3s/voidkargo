# Design Language — "Void & Ember"

The visual identity shared by the website and (eventually) the in-game HUD. If you're building
a new screen or component and aren't sure whether something "fits", check it against the
principles below before checking it against the token list.

See also: [ADR 0004](../02-decisions/0004-design-language-and-ui-foundation.md) for *why* this
exists and how it's implemented; [design-tokens.md](design-tokens.md) for the token reference;
[component-inventory.md](component-inventory.md) for what to build next.

## The brief, in one sentence

**High-tech Slavic logistics command centre** — instrumentation, not decoration. Think Dune's
Ornithopter cockpits, Elite Dangerous's ship HUD, a well-run rail dispatch office: sleek,
technical, quiet, expensive-feeling because of restraint, not shine.

## Five rules

1. **One accent, spent sparingly.** Ember amber is the only "loud" colour in the system. If a
   screen has more than one or two ember elements fighting for attention, something's wrong.
   Everything else is void, steel, and bone.
2. **Depth from hairlines and value steps, not shadows or gradients.** A panel sits on the page
   because it's a slightly different shade of dark and has a 1px border — not because it's
   floating with a drop shadow. `--vk-shadow-lift` exists for genuinely floating layers (modals)
   only.
3. **Numbers are always monospace, tabular.** Any figure a player reads repeatedly — cargo
   tonnage, a countdown, a coordinate, an ID — uses `.vk-mono`/`.vk-numeric` and
   `font-variant-numeric: tabular-nums`. This one habit does more to make the UI feel like
   instrumentation than any visual flourish.
4. **Eyebrow labels are the connective tissue.** Small, uppercase, wide-tracked labels
   (`.vk-label`) sit above stats, panels and sections. Use them generously for structure; never
   use them for body copy.
5. **Motion is short and functional.** 90–200ms, ease-out. Nothing loops except a genuinely
   "live" status dot (`.vk-status--live`). No decorative motion, ever.

## The Slavic thread

The "Slavic" half of the brief isn't kitsch (no onion domes, no folk-costume clichés). It shows
up in two restrained, structural ways:

- **Typography.** `Unbounded` is a geometric display face that renders Cyrillic natively and
  well — the brand should look equally at home in Latin or Cyrillic script without needing a
  second typeface.
- **Ornament.** Rushnyk embroidery and cross-stitch patterns are built from a strict rhombus/
  diamond grid. `.vk-divider--stitch` and the background survey grid (`--vk-grid-line`) borrow
  that geometry, reduced to near-invisible hairlines — a texture you feel more than see.

Anything more literal (colour blocking from folk textiles, script-like flourishes) is
deliberately out of scope — it would tip the look from "sleek" into "themed", which the brief
explicitly wants to avoid ("not too shiny, just luxurious").

## Reference points and how they map

| Reference | What we borrowed |
|---|---|
| **Dune** (2021 sequences, Ornithopter HUD) | Warm, low-saturation amber against near-black; hard-edged, minimal chrome. |
| **Elite Dangerous** | HUD corner brackets (`.vk-hud`), stat blocks, "systems nominal" status language. |
| **Tron (Legacy)** | Thin glowing lines used only as *signal* (focus rings, live status) — never as ambient decoration. |
| **Jarvis (Iron Man UI)** | Dense, legible data density without visual noise; monospace readouts. |

## What "not too shiny" means in practice

- Radius ceiling of 4px (`--vk-radius-soft`) — reserved for pill badges. Everything else is
  0–2px. The language is machined, not soft.
- No decorative gradients. The only gradient-like effect in the system is the background
  survey-grid texture, which is a repeating hairline pattern, not a colour gradient.
  ["Glow" is a state (focus, live), never a style choice.](design-tokens.md#elevation-and-glow)
- Pure black/white are avoided everywhere (`--vk-void-900` not `#000`, `--vk-bone-000` not
  `#FFF`) — flat pure values look cheap on screens; a few percent of colour keeps things feeling
  considered.

## Reference implementation

`Components/Layout/MainLayout.razor`, `NavMenu.razor` and `Components/Pages/Home.razor` in
`Game.Website` are built entirely from tokens and the `.vk-*` primitives — no bespoke CSS beyond
layout grids. Copy their patterns rather than inventing new ones.
