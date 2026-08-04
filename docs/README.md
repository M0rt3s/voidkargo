# voidkargo Docs Vault

This is the documentation home for `voidkargo`. It's a plain-Markdown vault: open this
`docs/` folder directly in Obsidian for backlinks/graph view, or just browse it on GitHub —
every link below works in both.

Docs here are kept short and targeted on purpose: enough for a human or an AI agent to know
exactly where to look and what they need, without wading through a huge unmaintained wiki.

## Map of content

- **[00-overview](00-overview/project-overview.md)** — what the game is, at a glance.
  - [Project overview](00-overview/project-overview.md)
  - [Glossary](00-overview/glossary.md)
- **[01-architecture](01-architecture/system-architecture.md)** — how the pieces fit together.
  - [System architecture](01-architecture/system-architecture.md)
  - [Networking strategy (SignalR + REST)](01-architecture/networking-strategy.md)
  - [Data model](01-architecture/data-model.md)
- **[02-decisions](02-decisions/adr-template.md)** — why the big, hard-to-reverse choices were made.
  - [ADR template](02-decisions/adr-template.md)
  - [0001 — Use .NET Aspire for orchestration](02-decisions/0001-use-net-aspire-for-orchestration.md)
  - [0002 — Unity as the client engine](02-decisions/0002-unity-as-client-engine.md)
  - [0003 — SignalR + REST hybrid networking](02-decisions/0003-signalr-plus-rest-hybrid-networking.md)
  - [0004 — Design language and UI foundation](02-decisions/0004-design-language-and-ui-foundation.md)
- **[03-modules](03-modules/game-backend.md)** — one short doc per project.
  - [Game.Backend](03-modules/game-backend.md)
  - [Game.Shared](03-modules/game-shared.md)
  - [Game.Website](03-modules/game-website.md)
  - [Game.AppHost](03-modules/game-apphost.md)
  - [Game.Client (Unity)](03-modules/game-client-unity.md)
- **[04-workflows](04-workflows/ai-workflow.md)** — how work actually gets done here.
  - [AI workflow](04-workflows/ai-workflow.md)
  - [Local dev setup](04-workflows/local-dev-setup.md)
  - [Definition of done](04-workflows/definition-of-done.md)
- **[05-design](05-design/design-language.md)** — the "Void & Ember" visual identity, shared by
  the website and (eventually) the game client.
  - [Design language](05-design/design-language.md)
  - [Design tokens reference](05-design/design-tokens.md)
  - [Component inventory](05-design/component-inventory.md)

## Conventions for this vault

- Use standard Markdown relative links (`[text](../path.md)`), not Obsidian-only `[[wikilinks]]`,
  so everything renders correctly on GitHub too.
- Keep each doc short (roughly one screen). If a doc is growing long, split it.
- When you make a decision that would be expensive to reverse, write an ADR instead of burying
  the rationale in a PR description or a chat log.
