using Helpdesk.Tickets.Infrastructure;
using Helpdesk.Tickets.Presentation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// The stdio fallback host: the same TicketTools over a pipe instead of a port, for clients
// that only launch a process. Same module registration, same handlers, no web server.
var builder = Host.CreateApplicationBuilder(args);

// On stdio the protocol owns stdout, so a log line printed there corrupts the stream.
builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);

// Same database the compose container serves on 14330; override with the
// ConnectionStrings__Tickets environment variable when pointing it somewhere else.
builder.Configuration["ConnectionStrings:Tickets"] =
    builder.Configuration.GetConnectionString("Tickets")
    ?? "Server=localhost,14330;Database=HelpdeskTickets;User Id=sa;Password=Workshop!Passw0rd;TrustServerCertificate=True";

builder.Services.AddTicketsModule(builder.Configuration);

builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly(typeof(TicketTools).Assembly);

await builder.Build().RunAsync();
