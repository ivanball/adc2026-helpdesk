# Shipping it

Everything below runs from the repository root. Replace `<owner>` with your GitHub
account name, in lower case (container registry names must be lower case).

## What is in the box

- `Dockerfile` builds the host into a runtime image: SDK image restores in locked
  mode and publishes, `aspnet` image runs it as a non-root user on port 8080.
- `.github/workflows/ci.yml` runs on every push to `main` and every pull request.
  One job, `build-and-test`, with a SQL Server 2022 service container published on
  host port 14330, so the API tests use the same connection string they use locally.
- `.github/workflows/release.yml` runs when you push a tag starting with `v` and
  publishes the image to `ghcr.io/<owner>/helpdesk`.
- `.github/workflows/agent-review.yml` asks an agent to review each pull request.
  It needs an `ANTHROPIC_API_KEY` repository secret. Skip it if you do not have one.
  It is advisory: it is not in the protection body below, so it never blocks a merge.

## 1. Create the repository and push

Branch protection is free on public repositories only, so create it public. Name the
local branch `main` first: whatever you push becomes the default branch, and step 2
protects `main` by name.

```sh
git branch -M main
gh repo create <owner>/helpdesk --public --source=. --remote=origin --push
```

## 2. Protect main

The required check is the CI job name. The three test steps run inside that one
job, so there is one context to require, not three.

```sh
gh api -X PUT repos/<owner>/helpdesk/branches/main/protection --input - <<'JSON'
{
  "required_status_checks": { "strict": true, "contexts": ["build-and-test"] },
  "enforce_admins": false,
  "required_pull_request_reviews": null,
  "restrictions": null
}
JSON
```

`strict: true` means a branch has to be up to date with `main` before it can merge.
`required_pull_request_reviews: null` keeps you able to merge your own pull request,
which matters when you are the only person in the repository.

## 3. Open the pull request that breaks a rule

`lab5-violation` takes a shortcut the architecture tests forbid: the endpoint injects
the concrete command handler instead of the interface, which means the handler had to
become public and the request now skips the validation and logging decorators. It
compiles cleanly. Make the change on a branch (`git checkout -b lab5-violation`),
commit it, push it and open a pull request to watch CI go red on the fitness test
rather than on the compiler. If you fetched the `lab5-violation` branch instead, it is
already committed: skip the first line.

```sh
git commit -am "Call the handler straight from the endpoint"
git push -u origin lab5-violation
gh pr create --base main --head lab5-violation --title "Call the handler straight from the endpoint" --body "Deliberate layer violation. CI should reject this."
```

In the run's step list `Architecture (fitness) tests` has the red X and
`API integration tests` is skipped. The step log names the offender:

```text
  Failed Helpdesk.ArchitectureTests.LayerRulesTests.Rule4_Handlers_are_internal [72 ms]
  Error Message:
   Public handlers found: Helpdesk.Tickets.Application.Tickets.CreateTicketHandler
```

## 4. Fix it, then close the pull request

Put the interface back in the endpoint and `internal` back on the handler, then commit
and push to the same branch:

```sh
git commit -am "Put the interface back" && git push
```

Wait for `build-and-test` to go green, then close the pull request rather than
merging it:

```sh
gh pr close lab5-violation
```

The fix reverted both edits, so the branch now changes nothing, and squash-merging it
would put an empty commit named after the violation into `main`.

## 5. Land the image files, then tag a release

`Dockerfile`, `.dockerignore` and `.github/workflows/release.yml` reach `main` through
a pull request like everything else. Start from an up-to-date `main`:

```sh
git checkout main && git pull
git checkout -b ship-image && git add -A && git commit -m "Dockerfile and release workflow" && git push -u origin ship-image
gh pr create --base main --head ship-image --title "Dockerfile and release workflow" --body "Release on tag."
```

If the three files were already on `main` when you pushed in step 1, there is nothing
to commit here: skip this block and the merge, and go straight to the tag.

Wait for `build-and-test` to go green, then squash it in:

```sh
gh pr merge --squash --delete-branch
```

Type the merge yourself. If you ask the agent, the `confirm-merge.sh` hook refuses it.

The merge to `main` only runs CI. Pushing the tag is what triggers `release.yml`, and
the tag name becomes the image tag:

```sh
git checkout main && git pull && git tag v0.1.0 && git push origin v0.1.0
```

## 6. Pull the published image

Wait for the release run to finish, then:

```sh
docker pull ghcr.io/<owner>/helpdesk:v0.1.0
docker run --rm -p 8080:8080 ghcr.io/<owner>/helpdesk:v0.1.0
```

It starts without a database and answers `/health` on port 8080.

The package is linked to your public repository and inherits its visibility, so it is
public: anyone can `docker pull` it without logging in. In a private repository the
package is private too, and a teammate's pull is denied until you grant access on the
package settings page.

## Day to day

CI restores in locked mode, which fails if a package resolves to anything the
committed `packages.lock.json` files do not already list. So after you change a
package version, regenerate the lock files and commit them:

```sh
dotnet restore Helpdesk.slnx --force-evaluate
```

`NuGetAuditMode` is set to `all` and `NuGetAuditLevel` to `low`, so a known
vulnerability in a direct or a transitive package breaks the build. Bump the package
rather than suppressing the warning.
