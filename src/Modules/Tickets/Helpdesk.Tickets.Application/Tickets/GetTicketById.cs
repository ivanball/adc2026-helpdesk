using Helpdesk.SharedKernel;
using Helpdesk.Tickets.Application.Abstractions;
using Helpdesk.Tickets.Domain;

namespace Helpdesk.Tickets.Application.Tickets;

public sealed record GetTicketById(Guid TicketId) : IQuery<TicketResponse>;

public sealed record TicketResponse(
    Guid Id,
    string Title,
    string Status,
    IReadOnlyList<ChildTaskResponse> ChildTasks);

public sealed record ChildTaskResponse(Guid Id, string Description, bool IsDone);

internal sealed class GetTicketByIdHandler(ITicketRepository tickets)
    : IQueryHandler<GetTicketById, TicketResponse>
{
    public async Task<Result<TicketResponse>> Handle(GetTicketById query, CancellationToken ct)
    {
        var ticket = await tickets.GetById(query.TicketId, ct);
        if (ticket is null)
            return Result.Failure<TicketResponse>(TicketErrors.NotFound);

        var response = new TicketResponse(
            ticket.Id,
            ticket.Title,
            ticket.Status.ToString(),
            ticket.ChildTasks
                .Select(c => new ChildTaskResponse(c.Id, c.Description, c.IsDone))
                .ToList());

        return Result.Success(response);
    }
}
