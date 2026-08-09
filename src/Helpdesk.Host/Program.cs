using Helpdesk.Host;
using Helpdesk.SharedKernel;
using Helpdesk.Tickets.Application.Tickets;
using Helpdesk.Tickets.Infrastructure;
using Helpdesk.Tickets.Presentation;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Telemetry, resilience, service discovery and the "self" liveness check.
builder.AddServiceDefaults();

builder.Services.AddTicketsModule(builder.Configuration);

// Events: the in-process bus, the outbox relay that feeds it, and today's throwaway consumer.
builder.Services.AddSingleton<IEventBus, InProcessEventBus>();
builder.Services.AddTicketsOutboxRelay();
builder.Services.AddScoped<IIntegrationEventHandler<TicketClosedIntegrationEvent>, TicketClosedLoggingHandler>();

// MCP is one more adapter at the API layer: the tools in the Tickets Presentation assembly
// call the same handlers the HTTP endpoints below call.
// Stateless because the 2026-07-28 specification drops the initialize handshake and the
// Mcp-Session-Id header from the wire, so every POST stands on its own and no instance has
// to remember a caller between requests.
builder.Services.AddMcpServer()
    .WithHttpTransport(options => options.SessionMode = HttpServerSessionMode.Stateless)
    .WithToolsFromAssembly(typeof(TicketTools).Assembly);

// Readiness only: this check is untagged, so /health waits on the database and /alive does not.
builder.Services.AddHealthChecks()
    .AddDbContextCheck<TicketsDbContext>("tickets-db");

var app = builder.Build();

// Maps /alive (liveness) and /health (readiness). Replaces the hand written /health stub.
app.MapDefaultEndpoints();

// Migrations:ApplyOnStartup is off by default, and only the AppHost turns it on. An explicit
// switch beats sniffing the environment: you can read it in one line, the labs that run the
// host on its own keep migrating by hand with dotnet ef database update, and the API tests,
// which drop and re-create their own database in the factory, never migrate twice.
if (app.Configuration.GetValue<bool>("Migrations:ApplyOnStartup"))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<TicketsDbContext>().Database.MigrateAsync();
}

app.MapTicketEndpoints();

// The MCP endpoint sits beside the HTTP routes, not in front of them or instead of them.
app.MapMcp("/mcp");

app.Run();

// Top-level statements hide the generated entry point class, and
// WebApplicationFactory<Program> in the API tests needs to see it.
public partial class Program { }
