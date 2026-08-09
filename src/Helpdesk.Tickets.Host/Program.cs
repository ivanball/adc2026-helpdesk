using Helpdesk.ModuleKit;
using Helpdesk.SharedKernel;
using Helpdesk.Tickets.Host;
using Helpdesk.Tickets.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// The extracted service: the SAME module type, registered alone. Nothing in Domain,
// Application, Presentation or Infrastructure changed to make this file possible.
var modules = new IModule[] { new TicketsModule() };

foreach (var module in modules)
{
    module.Add(builder.Services, builder.Configuration);
}

builder.Services.AddSingleton<IEventBus, LoggingEventBus>();

var app = builder.Build();
app.MapGet("/health", () => "OK");

foreach (var module in modules)
{
    module.MapEndpoints(app);
}

app.Run();
