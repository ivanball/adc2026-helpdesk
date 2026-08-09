using Helpdesk.Tickets.Domain;

namespace Helpdesk.Tickets.Application.Abstractions;

public interface ITicketRepository
{
    void Add(Ticket ticket);
    Task<Ticket?> GetById(Guid id, CancellationToken ct);   // child tasks load automatically (owned collection)
}

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct);
}
