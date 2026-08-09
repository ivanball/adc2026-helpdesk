using Helpdesk.Tickets.Application;
using Helpdesk.Tickets.Application.Abstractions;
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
}
