using Helpdesk.Host;
using Helpdesk.ModuleKit;
using Helpdesk.Notifications.Infrastructure;
using Helpdesk.SharedKernel;
using Helpdesk.Tickets.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// The module list. THE EXTRACTION DEMO: comment out the Tickets assembly here, run
// Helpdesk.Tickets.Host on 5101 and Helpdesk.Gateway on 5000, and the client sees no change.
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

builder.Services.AddSingleton<IEventBus, InProcessEventBus>();

var app = builder.Build();
app.MapGet("/health", () => "OK");

foreach (var module in modules)
{
    module.MapEndpoints(app);
}

app.Run();
