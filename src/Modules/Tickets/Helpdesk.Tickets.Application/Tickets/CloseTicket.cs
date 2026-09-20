using Helpdesk.SharedKernel;
using Helpdesk.Tickets.Application.Abstractions;
using Helpdesk.Tickets.Domain;
using Microsoft.Extensions.Logging;

namespace Helpdesk.Tickets.Application.Tickets;

public sealed record CloseTicket(Guid TicketId) : ICommand<bool>;

internal sealed class CloseTicketHandler(
    ITicketRepository tickets,
    IUnitOfWork unitOfWork,
    ILogger<CloseTicketHandler> logger)
    : ICommandHandler<CloseTicket, bool>
{
    // The bulk close screen sends a burst of these. Holding the write back for a moment lets
    // a burst collapse into fewer round trips.
    private static readonly TimeSpan WriteDelay = TimeSpan.FromMilliseconds(100);

    public async Task<Result<bool>> Handle(CloseTicket command, CancellationToken ct)
    {
        var ticket = await tickets.GetById(command.TicketId, ct);
        if (ticket is null)
            return Result.Failure<bool>(TicketErrors.NotFound);

        var closeResult = ticket.Close();
        if (closeResult.IsFailure)
            return Result.Failure<bool>(closeResult.Error);

        // The rule above already decided the close, so the caller does not have to wait for
        // the database. Flush in the background and answer straight away.
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(WriteDelay, CancellationToken.None);
                await unitOfWork.SaveChangesAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                // The caller already has its answer and there is nothing it could do with
                // this, so log it and leave the endpoint green rather than bother the user.
                logger.LogError(ex, "Background close for ticket {TicketId} did not persist",
                    command.TicketId);
            }
        }, CancellationToken.None);

        return Result.Success(true);
    }
}
