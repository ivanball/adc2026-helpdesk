# Running it

One command starts the database, the API and a dashboard that shows you what they did.
Everything below runs from the repository root.

## 1. Start the orchestrated run

```sh
aspire run --apphost src/Helpdesk.AppHost
```

This needs the Aspire CLI 13.6 or newer, and the AppHost project must be on 13.6 too:
`Aspire.AppHost.Sdk/13.6.0`, `Aspire.Hosting.SqlServer` 13.6.0 and
`<AspireUseCliBundle>true</AspireUseCliBundle>`, as committed here. A 13.0 AppHost still
runs under the 13.6 CLI, but `aspire ps` says "No running AppHost found" and the MCP
server in section 4 sees nothing. The CLI gives no warning.

The console prints a dashboard URL that looks like `https://localhost:17288/login?t=...`.
The token is in the URL, so open the link the console printed rather than typing the host
name by hand. Leave the command running; stopping it stops the resources it started.

The AppHost starts its own SQL Server container, named `helpdesk-aspire-sql`, on a port
Aspire picks. The `docker compose` container on port 14330 that labs 1 to 5 use is a
different container and keeps running. Nothing you did in the earlier labs breaks.

The orchestrated run is the only configuration that migrates the database on startup. The
AppHost sets `Migrations__ApplyOnStartup=true`; everywhere else the setting is absent and the
host leaves the schema alone, which is what `dotnet ef database update` is for.

## 2. Read one trace

Create a ticket so there is something to look at:

```sh
curl -i -X POST http://localhost:5000/tickets -H "content-type: application/json" -d "{\"title\":\"Printer on fire\"}"
```

Use the port the dashboard shows for the `api` resource, not necessarily 5000.

Open **Traces** in the dashboard and click the newest one. One request, two levels:

1. `POST /tickets` is the ASP.NET Core span. Its duration is what the caller waited.
2. Directly under it, an `INSERT` span from the SQL client. That span is why the service
   defaults project turns on SQL instrumentation. Without it the database time is invisible
   and shows up as unexplained gap inside the HTTP span.

Click any span to see its attributes, then use **Structured logs** to see the log lines
written during that same request. The dashboard keeps them joined by trace id, so you can
go from a slow request to the exact log line without grepping anything.

## 3. The two health endpoints

```sh
curl -i http://localhost:5000/alive
curl -i http://localhost:5000/health
```

`/alive` is liveness: it answers yes while the process is running and checks nothing else.
`/health` is readiness: it answers yes only when every dependency this instance needs,
including the database, is reachable. The rule that follows from those two definitions is
that startup gates on liveness and never on readiness: the AppHost uses `WaitFor(tickets)`
to hold the API until SQL Server reports healthy, but nothing waits on the API's own
`/health`, because readiness only goes green after the schema is in place and the schema
only arrives after the API starts.

## 4. Point your coding agent at the dashboard

The Aspire CLI can expose the running application to a coding agent over MCP, so the agent
can list resources, read logs and read traces from the run you just started.

```sh
aspire agent --help
aspire agent init --mcp
```

`aspire agent init --mcp` writes the MCP server entry for the agent you pick (without
`--mcp` it installs skills only and leaves MCP unconfigured). The entry for Claude Code
lives in `.mcp.json` at the repository root and is already committed here:

```json
{ "mcpServers": { "aspire": { "command": "aspire", "args": ["agent", "mcp"] } } }
```

VS Code reads `.vscode/mcp.json` instead, in a different shape: a top-level `servers` key
and `"type": "stdio"` on the entry.

Check `aspire agent --help` first. The command arrived in 13.2, and a CLI that predates it
answers `Unrecognized command or argument 'agent'`. If yours does, update the CLI before the
next section, because the whole exercise is the agent reading live telemetry. Restart the
agent after writing the file so it picks the server up, and confirm the `aspire` server is
listed and connected.

## 5. The exercise: a bug the client cannot see

Branch `lab6-fault` ships one planted fault in the close-ticket path. The API answers
`204 No Content` and the ticket stays open. Nothing in the response says so.

1. Stop the run, check the branch out, and start it again:

   ```sh
   git checkout lab6-fault
   aspire run --apphost src/Helpdesk.AppHost
   ```

2. Create a ticket, close it, then read it back:

   ```sh
   curl -s -X POST http://localhost:5000/tickets -H "content-type: application/json" -d "{\"title\":\"Badge reader dead\"}"
   curl -i -X POST http://localhost:5000/tickets/<id>/close
   curl -s http://localhost:5000/tickets/<id>
   ```

   The close returns `204`. The read back says `"status":"Open"`.

3. Look at the dashboard, not at the code. Open **Structured logs** and filter to **Error**.
   There is one entry from the close handler, `Background close for ticket ... did not
   persist`, with an `ObjectDisposedException` attached. Now look at **Traces**: the close
   request is there, green, answered `204`, with a `SELECT` under it and no `UPDATE`. The
   Error log sits on that same trace, written after the request's span had already ended.
   An Error log on a request whose trace already ended green is the tell. The write ran
   after the request was already answered.

4. Now ask your agent, without telling it where the bug is:

   > Tickets do not close. The close endpoint returns 204 but the ticket reads back as open.
   > Use the aspire MCP server to read the logs and traces from the running app, find out
   > why from the running application rather than by reading the code first, and fix it.

   Watch it read the error log, follow the exception to the handler, and propose the fix.
   That loop, run the app, read its telemetry, change the code, is the point of the lab.

5. Confirm the fix the way CI would. Stop the run first: while the AppHost runs, the API
   process locks its own `bin` folder and the test build fails with `MSB3027`.

   ```sh
   dotnet test tests/Helpdesk.ApiTests
   ```

   `CloseTicketTests` fails on `lab6-fault` and passes once the fault is gone. The five
   architecture tests pass on both branches, because a swallowed exception is a behaviour
   bug and no layer rule can see it. Tests catch what rules cannot, and telemetry catches
   what tests were never written for.

6. Go back when you are done: `git checkout lab6-end`.

## What the instructor deploys

Demo only. Do not run this part on workshop wifi.

The same AppHost graph is what gets deployed. Add a deployment target to the AppHost, then:

```sh
aspire deploy --apphost src/Helpdesk.AppHost
```

`aspire deploy` reads the deployment targets the AppHost declares, so an Azure Container
Apps run needs the Azure Container Apps environment added to `AppHost.cs` first; with no
target declared the command has nothing to deploy. The older pair `azd init` followed by
`azd up` does the same job from the same AppHost and is the fallback.

What ships is the graph, not a folder of YAML you maintain by hand: one container app per
project resource, SQL Server as one more container app (`AddSqlServer` deploys as a SQL
Server container app; `AddAzureSqlServer` is the managed Azure SQL option), and the
connection strings wired between them the same way `WithReference` wires them on your
laptop.
