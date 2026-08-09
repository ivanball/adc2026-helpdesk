using Helpdesk.SharedKernel;
using Helpdesk.Tickets.Contracts;

namespace Helpdesk.Notifications.Infrastructure;

internal sealed class TicketClosedNotificationHandler(NotificationsDbContext db)
    : IIntegrationEventHandler<TicketClosedIntegrationEvent>
{
    public async Task Handle(TicketClosedIntegrationEvent e, CancellationToken ct)
    {
        db.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            Message = $"Ticket {e.TicketId} was closed.",
            CreatedOnUtc = DateTime.UtcNow,
        });
        await db.SaveChangesAsync(ct);   // idempotency note: see closing beat
    }
}
