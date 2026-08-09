using Microsoft.Extensions.DependencyInjection;

namespace Helpdesk.Notifications.Application;

public static class NotificationsApplication
{
    /// <summary>
    /// Slices register here as they are added. Today the module's only behavior is the
    /// integration-event handler, which needs the DbContext and therefore lives in
    /// Helpdesk.Notifications.Infrastructure.
    /// </summary>
    public static IServiceCollection AddNotificationsApplication(this IServiceCollection services)
    {
        return services;
    }
}
