# Contributing

`voidkargo` is currently developed by a single indie developer, with AI coding agents
assisting throughout. This doc covers the human workflow; AI agents should read
[AGENTS.md](AGENTS.md) instead (or in addition).

## Prerequisites

See [`docs/04-workflows/local-dev-setup.md`](docs/04-workflows/local-dev-setup.md).

## Workflow

1. **Branch**: `feature/<short-description>`, `fix/<short-description>`, or
   `docs/<short-description>` off `main`.
2. **Commits**: prefer [Conventional Commits](https://www.conventionalcommits.org/)
   (`feat:`, `fix:`, `docs:`, `chore:`, `refactor:`, `test:`) — keeps history scannable by
   both humans and AI agents summarizing changes later.
3. **Before opening a PR**, run:
   ```bash
   dotnet build
   dotnet test
   ```
4. **PR checklist**: filled in automatically from
   `.github/PULL_REQUEST_TEMPLATE.md` — see
   [`docs/04-workflows/definition-of-done.md`](docs/04-workflows/definition-of-done.md).
5. **Docs**: if you touch behavior, contracts, or how something is run/tested, update the
   matching doc under `docs/03-modules/` (and add an ADR under `docs/02-decisions/` for any
   decision that would be expensive to reverse).

## Repo structure

See the table in [AGENTS.md](AGENTS.md#repo-map) or start browsing at
[`docs/README.md`](docs/README.md).

## Using an Obsidian vault

`docs/` is a plain-Markdown, relative-link vault — open the `docs/` folder directly as an
Obsidian vault if you want backlinks/graph view, or just browse it on GitHub. No Obsidian
plugins are required.
