using Helpdesk.Host;
using Helpdesk.SharedKernel;
using Helpdesk.Tickets.Application.Tickets;
using Helpdesk.Tickets.Infrastructure;
using Helpdesk.Tickets.Presentation;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTicketsModule(builder.Configuration);

// Events: the in-process bus, the outbox relay that feeds it, and today's throwaway consumer.
builder.Services.AddSingleton<IEventBus, InProcessEventBus>();
builder.Services.AddTicketsOutboxRelay();
builder.Services.AddScoped<IIntegrationEventHandler<TicketClosedIntegrationEvent>, TicketClosedLoggingHandler>();

var app = builder.Build();
app.MapGet("/health", () => "OK");
app.MapTicketEndpoints();
app.Run();
