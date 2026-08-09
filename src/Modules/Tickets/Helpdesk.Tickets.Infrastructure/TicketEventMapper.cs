using Helpdesk.SharedKernel;
using Helpdesk.Tickets.Contracts;
using Helpdesk.Tickets.Domain;

namespace Helpdesk.Tickets.Infrastructure;

internal static class TicketEventMapper
{
    public static IIntegrationEvent? ToIntegrationEvent(IDomainEvent domainEvent) =>
        domainEvent switch
        {
            TicketClosed e => new TicketClosedIntegrationEvent(Guid.NewGuid(), e.TicketId, e.OccurredOnUtc),
            _ => null,   // most domain events never leave the module, on purpose
        };
}
