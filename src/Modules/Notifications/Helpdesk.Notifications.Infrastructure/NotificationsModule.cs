using Helpdesk.ModuleKit;
using Helpdesk.Notifications.Application;
using Helpdesk.SharedKernel;
using Helpdesk.Tickets.Contracts;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Helpdesk.Notifications.Infrastructure;

public sealed class NotificationsModule : IModule
{
    public void Add(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<NotificationsDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Notifications")));
        services.AddNotificationsApplication();
        services.AddScoped<IIntegrationEventHandler<TicketClosedIntegrationEvent>,
            TicketClosedNotificationHandler>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // No HTTP surface today: this module only reacts to integration events.
    }
}
