# Helpdesk: the workshop app

A modular monolith on .NET 10, built one lab at a time. One Tickets module runs through four
layers (Domain, Application, Infrastructure, Presentation) over a hand-rolled `Result` type
and a hand-rolled CQRS pipeline, so nothing here hides behind a framework you cannot read.
The layer rules are fitness tests rather than a diagram, the app ships through CI and a
release workflow as a container image, Aspire orchestrates the local run, and the app hosts
its own MCP server so an agent can call the same handlers the HTTP endpoints call. On top of
that, a second module (Notifications) reacts to ticket events through an outbox and an
in-process bus, and the same Tickets module also runs as its own service behind a YARP
gateway: that is the extraction recorded in the clip.

## Run it

```sh
docker compose up -d          # SQL Server 2022 on localhost:14330

dotnet tool restore           # dotnet ef is a local tool

dotnet ef database update -p src/Modules/Tickets/Helpdesk.Tickets.Infrastructure -s src/Helpdesk.Host --context TicketsDbContext
dotnet ef database update -p src/Modules/Notifications/Helpdesk.Notifications.Infrastructure -s src/Helpdesk.Host --context NotificationsDbContext

dotnet run --project src/Helpdesk.Host    # both modules, one process, port 5000
```

```sh
curl -s -X POST http://localhost:5000/tickets -H "content-type: application/json" -d "{\"title\":\"Printer on fire\"}"
curl -s -X POST http://localhost:5000/tickets/<id>/close
curl -s http://localhost:5000/tickets/<id>
```

Or let Aspire start the database, migrate it and run the app, with a dashboard for traces,
logs and metrics:

```sh
aspire run --apphost src/Helpdesk.AppHost
```

## Test it

```sh
dotnet test Helpdesk.slnx
```

The API tests boot the real host against the SQL container on 14330, so `docker compose up -d`
has to have run first. They use databases of their own and never touch development data.

## The extraction (three terminals)

The extraction is a deployment topology, not a rewrite. `src/Helpdesk.Tickets.Host` runs the
SAME `TicketsModule` alone on port 5101, and `src/Helpdesk.Gateway` (YARP) sits on port 5200
routing `/tickets/**` to it and everything else to the main host on 5000.

The main host keeps both modules, so the demo flips its module list live: open
`src/Helpdesk.Host/Program.cs` and comment out `typeof(TicketsModule).Assembly` from the
`moduleAssemblies` array. That one line is the whole extraction on the monolith side.
Restore it afterwards.

```sh
dotnet run --project src/Helpdesk.Gateway        # port 5200
dotnet run --project src/Helpdesk.Host           # port 5000
dotnet run --project src/Helpdesk.Tickets.Host   # port 5101
```

Then the lab 3 curl, unchanged except for the port, against the gateway:

```sh
curl -s -X POST http://localhost:5200/tickets -H "content-type: application/json" -d "{\"title\":\"Extracted!\"}"
curl -s -X POST http://localhost:5200/tickets/<id>/close
```

The one honest TODO: the in-process bus does not cross processes. In the extracted host the
bus is `LoggingEventBus`, which logs where a broker transport (RabbitMQ, Azure Service Bus,
Redis) plugs in. Every handler and every outbox row survives that swap unchanged: one class,
one DI line.

## Docs

- [docs/architecture-rules.md](docs/architecture-rules.md): the layer rules the fitness tests enforce.
- [docs/running.md](docs/running.md): the orchestrated run, the dashboard, and the two health endpoints.
- [docs/shipping.md](docs/shipping.md): the container image, CI, branch protection and the release tag.
- [docs/mcp.md](docs/mcp.md): the MCP server the app hosts, its tools, and what to watch for.

## Branches

One branch per lab, plus the two intentionally broken demo branches and the two historical
snapshots: [docs/branches.md](docs/branches.md).
