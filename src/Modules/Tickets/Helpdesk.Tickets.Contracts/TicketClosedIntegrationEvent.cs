using Helpdesk.SharedKernel;

namespace Helpdesk.Tickets.Contracts;

public sealed record TicketClosedIntegrationEvent(Guid Id, Guid TicketId, DateTime OccurredOnUtc)
    : IIntegrationEvent;
