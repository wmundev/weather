---
name: commit-changes
description: How changes are committed in this repo - straight onto the trunk branch `main`, never on a feature branch, and never pushed, so the author reviews every commit before it leaves the machine (a push to `main` runs CI and then deploys to AWS). Covers the checks to run first, staging only what the change touched, keeping unrelated work and the api.yaml pre-commit hook out of the commit, the message shape, and what to report afterwards. Use whenever committing, when asked to "commit", "checkpoint" or "save progress", before creating any branch, and when work was started on some other branch.
---

# Committing changes

## Commit to `main`, never push

- **The trunk is `main`** (tracking `origin/main`). Commit directly onto it. Do
  **not** create a feature branch or open a PR unless the user asks for one in
  so many words - this overrides any general habit of branching before
  committing on a default branch.
- **Never push.** Not `git push`, not `gh pr create`, not a push to any other
  branch or remote. A push to `main` runs `.github/workflows/ci.yml`, and a
  green CI run triggers `deploy.yml`, which deploys to AWS Elastic Beanstalk. The author
  reviews the commits locally and pushes them themselves.
- **Work already on another branch** (an older session, a worktree) goes onto
  `main` by fast-forward: `git checkout main && git merge --ff-only <branch>`,
  then `git branch -d <branch>`. If it will not fast-forward, stop and ask rather
  than rebasing or merging on your own. Leave the old `feature/*` branches whose
  upstream is gone alone unless asked - they predate this workflow.

## Before each commit

1. **Run the gate.** Commit only on green.
   - `dotnet build --configuration Release`
   - The unit suites per the `dotnet-test` skill (`weather-test`,
     `Weather.CLI.UnitTests`).
   - A change to a controller, middleware, `Program.cs` / `Hosting/`, or anything else on the
     request path also needs `Weather.API.IntegrationTests`. If those fail on
     missing AWS / ConfigCat credentials rather than on assertions, say so when
     reporting - don't call the change verified.
   - `dotnet format weather.sln --verify-no-changes --include <your files>` -
     scoped to the files you touched (see step 3).
2. **An endpoint change is not committable on its own.** A changed route, verb,
   parameter, or request body ships with its Bruno update (`bruno-collection-sync`)
   and its regenerated `api.yaml` in the **same** commit - see `add-api-endpoint`.
3. **Stage only the paths the change touched** - list them; never `git add -A`,
   `git add .`, `git add -u`, or `git commit -a`. The working copy is CRLF under
   `core.autocrlf` with `* text=auto` in `.gitattributes`, so running
   `dotnet format` or a ReSharper cleanup over the whole solution can mark
   untouched files modified. Format your own files only. If it happened anyway,
   `git diff --stat` shows nothing for those files; clear them from `git status`
   with `git add --pathspec-from-file=<list>`, which records the same blob and
   changes nothing.
4. **Keep work that isn't yours out of the commit.**
   - Check `git diff --cached --name-only` *before* staging. Anything already
     staged that this change did not touch is the author's work in progress - do
     not commit it and do not unstage it; stop and ask.
   - Unstaged edits and untracked files that aren't part of this change stay
     exactly as they are - don't stage, revert, or delete them.
5. **Watch the `api.yaml` hook.** `.githooks/pre-commit` (enabled through
   `core.hooksPath`) publishes `weather-backend` **from the working tree, not the
   index**, regenerates `api.yaml`, and stages it. So:
   - If unrelated, uncommitted changes under `weather-backend/`,
     `weather-application/`, `weather-repository/`, or `weather-domain/` touch a
     route, parameter, `[ProducesResponseType]`, or XML doc comment, the
     regenerated spec describes *their* surface too, and the hook folds it into
     your commit. Set them aside first:
     `git stash push --keep-index --include-untracked -m "commit-changes: unrelated work"`,
     commit, then `git stash pop`. If the pop conflicts, stop and tell the user -
     never drop the stash.
   - The hook also stages an `api.yaml` the author had modified by hand.
   - If generation fails (usually a build error) the hook blocks the commit. Fix
     the build; never bypass it with `--no-verify`.
6. **Check the result is exactly the change you meant**: `git diff --cached --stat`
   before committing, and `git show --stat HEAD` after - an `api.yaml` change in
   a commit that touched no endpoint means the hook picked up something it
   shouldn't have.

## The commit

- **Message**: a conventional prefix and a lowercase summary -
  `feat: cache weather lookups by zip code`, `fix: …`, `ci: …`, `chore: …`,
  `docs: …`, `test: …` - then a short body saying what changed and why. No
  `Co-Authored-By` or other attribution trailer, even when an agent wrote it.
  `build(deps): …` is Dependabot's; don't use it for hand-made commits.
- **One commit per shippable feature, always** - not only when asked for
  checkpoints. A request that bundles several features ("add X, then Y, and
  also Z") lands as several commits, one per feature, made as each is finished
  rather than in a single sweep at the end. Each commit builds, passes the gate
  on its own, and leaves the API deployable if it were pushed alone - remember
  every green push to `main` deploys. Don't split a feature into half-steps that
  could not ship on their own (a service with no caller, an action with no
  Bruno entry), and don't fold an unrelated fix or cleanup into a feature's
  commit - give it its own.
- **Let the pre-commit hook run**; never skip it with `--no-verify`. It is slow
  (it runs `dotnet publish`). If it fails, fix the cause and make a new commit -
  don't amend a commit you did not just make.

## Afterwards

Tell the user what is waiting for them to push: `git log --oneline origin/main..main`
and the branch state from `git status -sb` (e.g. `main...origin/main [ahead 3]`).
Note that pushing deploys, and call out anything that needs to happen first -
a new SSM parameter in `weather-infra`, a ConfigCat flag, a DynamoDB table.
Mention any stash you created and whether it was popped cleanly.
