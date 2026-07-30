# AI Workflow

How AI coding agents (GitHub Copilot, Cursor, Claude, Aider, etc.) should operate in this
repo. The canonical machine-readable contract is [`AGENTS.md`](../../AGENTS.md) at the repo
root — this doc gives the human-facing rationale and a few extra habits worth keeping.

## Why this setup

As a solo indie developer, AI agents are a force multiplier — but only if they have fast,
targeted access to the *right* context instead of needing to re-derive it (or guess) every
session. This repo is structured so that:

- **`AGENTS.md`** gives any agent the essentials in one file: repo map, build/test commands,
  conventions, dos/don'ts.
- **ADRs** (`docs/02-decisions/`) mean an agent never has to re-litigate or accidentally
  reverse a decision it wasn't there for — the reasoning is written down.
- **Per-module docs** (`docs/03-modules/`) keep context small and targeted: an agent working
  on `Game.Backend` doesn't need to load the whole repo's history to understand it.
- **`.editorconfig`** + enforced analyzers make style deterministic rather than something the
  agent has to infer from surrounding code.
- **Definition of Done** ([definition-of-done.md](definition-of-done.md)) gives a concrete,
  checkable bar for "finished" that both the developer and an agent can apply consistently.

## Habits worth keeping as the project grows

- When you ask an agent for a non-trivial architectural change, ask it to draft an ADR first.
- After any AI-assisted change that alters behavior or a contract, do a quick pass: did the
  matching `docs/03-modules/*.md` get updated? If not, ask the agent to do it before merging.
- Keep `AGENTS.md` itself short. If it starts accumulating detail that only applies to one
  module, move that detail into the module's own doc instead and link to it.
- Periodically ask an agent to check `AGENTS.md` and the vault against the actual code and
  flag drift — cheap insurance against docs rotting.
