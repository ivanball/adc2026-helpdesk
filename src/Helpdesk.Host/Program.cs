using Helpdesk.Tickets.Infrastructure;
using Helpdesk.Tickets.Presentation;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTicketsModule(builder.Configuration);

var app = builder.Build();
app.MapGet("/health", () => "OK");
app.MapTicketEndpoints();
app.Run();
