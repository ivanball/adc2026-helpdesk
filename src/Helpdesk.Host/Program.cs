using Helpdesk.Tickets.Infrastructure;
using Helpdesk.Tickets.Presentation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Telemetry, resilience, service discovery and the "self" liveness check.
builder.AddServiceDefaults();

builder.Services.AddTicketsModule(builder.Configuration);

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
app.Run();

// Top-level statements hide the generated entry point class, and
// WebApplicationFactory<Program> in the API tests needs to see it.
public partial class Program { }
