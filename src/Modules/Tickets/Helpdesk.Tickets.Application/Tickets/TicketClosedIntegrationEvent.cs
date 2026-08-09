using Helpdesk.SharedKernel;

namespace Helpdesk.Tickets.Application.Tickets;

public sealed record TicketClosedIntegrationEvent(Guid Id, Guid TicketId, DateTime OccurredOnUtc)
    : IIntegrationEvent;
