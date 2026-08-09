# Helpdesk (ADC 2026 pre-conference reference solution)

The finished reference solution for the ADC 2026 full-day workshop: a modular monolith on
.NET 10 (Domain / Application / Infrastructure / Presentation per module, Result-based error
handling, an outbox for cross-module events, executable layer rules, and a live extraction of
one module into its own service behind a YARP gateway).

Every commit on `main` corresponds to one lab or session. If an attendee falls behind, they
check out the matching snapshot branch and keep going.

## Branch map

| Branch | State |
|--------|-------|
| `lab1-end` | Walking skeleton: eight projects, the dependency rule wired into project references, `/health`. |
| `lab2-end` | The Ticket aggregate: Result / Error / Entity, the close rule, four domain tests. |
| `lab3-end` | Vertical slices: CQRS pipeline with decorators, EF Core persistence, POST /tickets, POST /tickets/{id}/close, GET /tickets/{id}, the `Initial` migration. |
| `session4-end` | The outbox: OutboxMessage, SaveChangesAsync interception, the relay BackgroundService, the in-process event bus, the `Outbox` migration. |
| `lab4-end` | Fitness tests: five NetArchTest rules in `Helpdesk.ArchitectureTests` plus `docs/architecture-rules.md`. |
| `session6-end` | The payoff: Tickets.Contracts, the Notifications module, IModule discovery, the extracted Tickets host, the YARP gateway, the `InitialNotifications` migration. |

`main` equals `session6-end`.

## How to run

```bash
docker compose up -d          # SQL Server 2022 on localhost:14330

dotnet ef database update -p src/Modules/Tickets/Helpdesk.Tickets.Infrastructure       -s src/Helpdesk.Host --context TicketsDbContext
dotnet ef database update -p src/Modules/Notifications/Helpdesk.Notifications.Infrastructure -s src/Helpdesk.Host --context NotificationsDbContext

dotnet run --project src/Helpdesk.Host        # both modules, one process, port 5100
```

`dotnet ef` is a local tool: run `dotnet tool restore` once after cloning.

```bash
curl -s -X POST localhost:5100/tickets -H "content-type: application/json" -d '{"title":"Printer on fire"}'
curl -s -X POST localhost:5100/tickets/<id>/close
curl -s localhost:5100/tickets/<id>
```

Build and test:

```bash
dotnet build Helpdesk.sln
dotnet test Helpdesk.sln
```

## The extraction demo (three terminals)

The extraction is a deployment topology, not a rewrite. `src/Helpdesk.Tickets.Host` runs the
SAME `TicketsModule` alone on port 5101, and `src/Helpdesk.Gateway` (YARP) sits on port 5000
routing `/tickets/**` to it and everything else to the main host on 5100.

The main host deliberately keeps BOTH modules in this repo, so the demo flips its module list
live: open `src/Helpdesk.Host/Program.cs` and comment out `typeof(TicketsModule).Assembly`
from the `moduleAssemblies` array. That one line is the whole "extraction" on the monolith
side. Restore it afterwards.

Three terminals:

```bash
dotnet run --project src/Helpdesk.Gateway        # port 5000
dotnet run --project src/Helpdesk.Host           # port 5100 (Notifications, and Tickets unless commented out)
dotnet run --project src/Helpdesk.Tickets.Host   # port 5101 (Tickets only)
```

Then the Session 3 curl, unchanged, still against port 5000:

```bash
curl -s -X POST localhost:5000/tickets -H "content-type: application/json" -d '{"title":"Extracted!"}'
curl -s -X POST localhost:5000/tickets/<id>/close
```

The one honest TODO: the in-process bus does not cross processes. In the extracted host the
bus is `LoggingEventBus`, which logs where a broker transport (RabbitMQ, Azure Service Bus,
Redis) plugs in. Every handler and every outbox line survives that swap unchanged: one class,
one DI line.
