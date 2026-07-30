# Definition of Done

A checklist for both humans and AI agents to apply before calling any task/PR complete.
Kept intentionally short — this should be quick to run through every time, not a burden.

- [ ] `dotnet build` succeeds with no new warnings introduced.
- [ ] `dotnet test` passes, including new tests for new/changed logic (especially in
      `Game.Shared` and `Game.Backend`).
- [ ] If a DTO or public type in `Game.Shared` changed, both `Game.Backend` usages were
      checked and the change is noted (Unity isn't scaffolded yet, but treat it as a
      consumer regardless).
- [ ] If behavior, a contract, or how to run/test a project changed, the matching doc in
      `docs/03-modules/` was updated.
- [ ] If a decision was made that would be expensive to reverse (new dependency, changed
      architecture, changed networking approach), an ADR was added under `docs/02-decisions/`.
- [ ] No unrelated changes bundled into the same PR.
- [ ] Commit messages follow [Conventional Commits](https://www.conventionalcommits.org/)
      (see [CONTRIBUTING.md](../../CONTRIBUTING.md)).

If any box can't be checked, either fix it before finishing, or explicitly note in the PR
description why it's deferred and to what follow-up.
