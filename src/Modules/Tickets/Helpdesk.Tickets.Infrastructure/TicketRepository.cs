using Helpdesk.Tickets.Application.Abstractions;
using Helpdesk.Tickets.Domain;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Tickets.Infrastructure;

internal sealed class TicketRepository(TicketsDbContext db) : ITicketRepository
{
    public void Add(Ticket ticket) => db.Tickets.Add(ticket);

    public Task<Ticket?> GetById(Guid id, CancellationToken ct) =>
        db.Tickets.FirstOrDefaultAsync(t => t.Id == id, ct);
}
