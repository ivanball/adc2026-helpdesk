using Helpdesk.Host;
using Helpdesk.ModuleKit;
using Helpdesk.Notifications.Infrastructure;
using Helpdesk.SharedKernel;
using Helpdesk.Tickets.Infrastructure;
using Helpdesk.Tickets.Presentation;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Telemetry, resilience, service discovery and the "self" liveness check.
builder.AddServiceDefaults();

// The module list. THE EXTRACTION DEMO: comment out the Tickets assembly here, run
// Helpdesk.Tickets.Host on 5101 and Helpdesk.Gateway on 5200, and the client sees no change.
var moduleAssemblies = new[]
{
    typeof(TicketsModule).Assembly,
    typeof(NotificationsModule).Assembly,
};

var modules = moduleAssemblies
    .SelectMany(assembly => assembly.GetTypes())
    .Where(type => typeof(IModule).IsAssignableFrom(type) && type is { IsAbstract: false, IsInterface: false })
    .Select(type => (IModule)Activator.CreateInstance(type)!)
    .ToList();

foreach (var module in modules)
{
    module.Add(builder.Services, builder.Configuration);
}

// The transport the outbox relay publishes onto. Each module registers its own handlers,
// so the composition root names the bus and nothing else about the conversation.
builder.Services.AddSingleton<IEventBus, InProcessEventBus>();

// MCP is one more adapter at the API layer: the tools in the Tickets Presentation assembly
// call the same handlers the HTTP endpoints below call.
// Stateless because the 2026-07-28 specification drops the initialize handshake and the
// Mcp-Session-Id header from the wire, so every POST stands on its own and no instance has
// to remember a caller between requests.
builder.Services.AddMcpServer()
    .WithHttpTransport(options => options.SessionMode = HttpServerSessionMode.Stateless)
    .WithToolsFromAssembly(typeof(TicketTools).Assembly);

// Readiness only: these checks are untagged, so /health waits on both module databases
// and /alive does not.
builder.Services.AddHealthChecks()
    .AddDbContextCheck<TicketsDbContext>("tickets-db")
    .AddDbContextCheck<NotificationsDbContext>("notifications-db");

var app = builder.Build();

// Maps /alive (liveness) and /health (readiness). Replaces the hand written /health stub.
app.MapDefaultEndpoints();

// Migrations:ApplyOnStartup is off by default, and only the AppHost turns it on. An explicit
// switch beats sniffing the environment: you can read it in one line, the labs that run the
// host on its own keep migrating by hand with dotnet ef database update, and the API tests,
// which drop and re-create their own databases in the factory, never migrate twice.
if (app.Configuration.GetValue<bool>("Migrations:ApplyOnStartup"))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<TicketsDbContext>().Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<NotificationsDbContext>().Database.MigrateAsync();
}

foreach (var module in modules)
{
    module.MapEndpoints(app);
}

// The MCP endpoint sits beside the HTTP routes, not in front of them or instead of them.
app.MapMcp("/mcp");

app.Run();

// Top-level statements hide the generated entry point class, and
// WebApplicationFactory<Program> in the API tests needs to see it.
public partial class Program { }
