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

## 1. Create the repository and push

Branch protection is free on public repositories only, so create it public.

```sh
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
compiles cleanly. Push it and open a pull request to watch CI go red on the fitness
test rather than on the compiler.

```sh
git push -u origin lab5-violation && gh pr create --base main --head lab5-violation --title "Call the handler straight from the endpoint" --body "Deliberate layer violation. CI should reject this."
```

The failing check is `Architecture (fitness) tests`, and the message names the offender:
`Public handlers found: Helpdesk.Tickets.Application.Tickets.CreateTicketHandler`.

## 4. Fix it, then merge

Push a fix to the same branch, wait for `build-and-test` to go green, and squash it in.

```sh
gh pr merge --squash --delete-branch
```

## 5. Tag a release

Pushing the tag is what triggers `release.yml`. The tag name becomes the image tag.

```sh
git checkout main && git pull && git tag v0.1.0 && git push origin v0.1.0
```

## 6. Pull the published image

Wait for the release run to finish, then:

```sh
docker pull ghcr.io/<owner>/helpdesk:v0.1.0
```

The package starts out private. Make it public from the package settings page if you
want somebody else to pull it.

## Day to day

CI restores in locked mode, which fails if a package resolves to anything the
committed `packages.lock.json` files do not already list. So after you change a
package version, regenerate the lock files and commit them:

```sh
dotnet restore Helpdesk.sln --force-evaluate
```

`NuGetAuditMode` is set to `all` and `NuGetAuditLevel` to `low`, so a known
vulnerability in a direct or a transitive package breaks the build. Bump the package
rather than suppressing the warning.
