using Helpdesk.Tickets.Application.Abstractions;
using Helpdesk.Tickets.Application.Tickets;
using Microsoft.Extensions.DependencyInjection;

namespace Helpdesk.Tickets.Application;

public static class TicketsApplication
{
    public static IServiceCollection AddTicketsApplication(this IServiceCollection services)
    {
        // Slices register here as they are added.
        // Commands
        services.AddCommandHandler<CreateTicket, Guid, CreateTicketHandler>();
        services.AddScoped<IValidator<CreateTicket>, CreateTicketValidator>();
        services.AddCommandHandler<CloseTicket, bool, CloseTicketHandler>();

        // Queries
        services.AddQueryHandler<GetTicketById, TicketResponse, GetTicketByIdHandler>();
        return services;
    }
}
