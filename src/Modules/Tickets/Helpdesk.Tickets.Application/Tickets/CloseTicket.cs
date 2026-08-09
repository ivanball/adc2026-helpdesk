using Helpdesk.SharedKernel;
using Helpdesk.Tickets.Application.Abstractions;
using Helpdesk.Tickets.Domain;

namespace Helpdesk.Tickets.Application.Tickets;

public sealed record CloseTicket(Guid TicketId) : ICommand<bool>;

internal sealed class CloseTicketHandler(ITicketRepository tickets, IUnitOfWork unitOfWork)
    : ICommandHandler<CloseTicket, bool>
{
    public async Task<Result<bool>> Handle(CloseTicket command, CancellationToken ct)
    {
        var ticket = await tickets.GetById(command.TicketId, ct);
        if (ticket is null)
            return Result.Failure<bool>(TicketErrors.NotFound);

        var closeResult = ticket.Close();
        if (closeResult.IsFailure)
            return Result.Failure<bool>(closeResult.Error);

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}
