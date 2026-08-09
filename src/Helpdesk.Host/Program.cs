using Helpdesk.Tickets.Infrastructure;
using Helpdesk.Tickets.Presentation;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTicketsModule(builder.Configuration);

var app = builder.Build();
app.MapGet("/health", () => "OK");
app.MapTicketEndpoints();
app.Run();

// Top-level statements hide the generated entry point class, and
// WebApplicationFactory<Program> in the API tests needs to see it.
public partial class Program { }
