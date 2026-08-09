using Helpdesk.ModuleKit;
using Helpdesk.Tickets.Application;
using Helpdesk.Tickets.Application.Abstractions;
using Helpdesk.Tickets.Presentation;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Helpdesk.Tickets.Infrastructure;

public static class TicketsModuleRegistration
{
    public static IServiceCollection AddTicketsModule(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TicketsDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Tickets")));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<TicketsDbContext>());
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddTicketsApplication();
        return services;
    }

    // OutboxRelay is internal to this assembly, so the composition root registers it
    // through this public extension point rather than naming the type.
    public static IServiceCollection AddTicketsOutboxRelay(this IServiceCollection services)
    {
        services.AddHostedService<OutboxRelay>();
        return services;
    }
}

public sealed class TicketsModule : IModule
{
    public void Add(IServiceCollection services, IConfiguration configuration)
    {
        services.AddTicketsModule(configuration);
        services.AddTicketsOutboxRelay();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapTicketEndpoints();
    }
}
