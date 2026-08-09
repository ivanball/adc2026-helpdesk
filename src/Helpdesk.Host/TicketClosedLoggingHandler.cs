using Helpdesk.SharedKernel;
using Helpdesk.Tickets.Application.Tickets;

namespace Helpdesk.Host;

/// <summary>
/// Throwaway stand-in for a real consumer. Session 6 replaces it with the Notifications module.
/// </summary>
internal sealed class TicketClosedLoggingHandler(ILogger<TicketClosedLoggingHandler> logger)
    : IIntegrationEventHandler<TicketClosedIntegrationEvent>
{
    public Task Handle(TicketClosedIntegrationEvent integrationEvent, CancellationToken ct)
    {
        logger.LogInformation("NOTIFICATION: ticket {TicketId} was closed", integrationEvent.TicketId);
        return Task.CompletedTask;
    }
}
