# Branches

Every lab ends on a branch. If you fall behind, check out the branch for the lab you
just finished and keep going: `git checkout lab4-end`.

The test count is what `dotnet test Helpdesk.slnx` reports on that branch. The API tests
need the SQL container, so run `docker compose up -d` first.

| Branch | What it adds | Tests |
|--------|--------------|-------|
| `lab1-end` | Walking skeleton: eight projects, the dependency rule expressed as project references, `/health`, one placeholder test per test project. | 2 |
| `lab2-end` | `Result`, `Error` and `Entity` in the shared kernel, and the Ticket aggregate with its close rule. | 5 |
| `lab3-end` | Vertical slices: the CQRS abstractions and decorators, EF Core persistence and the `Initial` migration, the three ticket endpoints, the first API tests, and the `slice` skill that scaffolds a new one. | 7 |
| `lab4-end` | Executable layer rules: five NetArchTest fitness tests, `docs/architecture-rules.md`, and the two agent hooks that deny dangerous commands and confirm a merge. | 11 |
| `lab5-end` | Shipping: `Dockerfile`, `.dockerignore`, the CI, release and agent-review workflows, lock files and locked-mode restore. | 11 |
| `lab6-end` | Running it: the Aspire AppHost and service defaults, `/alive` and `/health`, the dashboard walkthrough in `docs/running.md`. | 13 |
| `lab7-end` | Extending it: the MCP server hosted by the app at `/mcp`, the ticket tools, the stdio fallback host, `docs/mcp.md`. | 16 |
| `main` | Everything above, plus the outbox and the in-process event bus, the Notifications module talking to Tickets only through `Helpdesk.Tickets.Contracts`, `IModule` discovery in the host, and the extracted `Helpdesk.Tickets.Host` behind the YARP gateway. This is the take-home branch. | 17 |
| `session4-end` | Historical snapshot (old lineage; the outbox and the second module now live on `main`). | |
| `session6-end` | Historical snapshot (old lineage; the outbox and the second module now live on `main`). | |
| `lab5-violation` | Demo: intentionally broken. The endpoint calls the handler directly and skips the pipeline, so the layer rules fail. | |
| `lab6-fault` | Demo: intentionally broken. The close handler saves in the background and answers early, so the write can be lost and the API tests catch it. | |
| `lab8-demo` | Demo: intentionally broken. The close handler is public, so rubric rule 3 scores 0 with a real citation and one fitness test fails. | |
