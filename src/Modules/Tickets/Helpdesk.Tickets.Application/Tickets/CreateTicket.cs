using Helpdesk.SharedKernel;
using Helpdesk.Tickets.Application.Abstractions;
using Helpdesk.Tickets.Domain;

namespace Helpdesk.Tickets.Application.Tickets;

public sealed record CreateTicket(string Title) : ICommand<Guid>;

public sealed class CreateTicketHandler(ITicketRepository tickets, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateTicket, Guid>
{
    public async Task<Result<Guid>> Handle(CreateTicket command, CancellationToken ct)
    {
        var ticketResult = Ticket.Create(command.Title);
        if (ticketResult.IsFailure)
            return Result.Failure<Guid>(ticketResult.Error);

        tickets.Add(ticketResult.Value);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(ticketResult.Value.Id);
    }
}

internal sealed class CreateTicketValidator : IValidator<CreateTicket>
{
    public IReadOnlyList<Error> Validate(CreateTicket instance)
    {
        var errors = new List<Error>();
        if (string.IsNullOrWhiteSpace(instance.Title))
            errors.Add(new Error("Tickets.Validation.EmptyTitle", "Title is required."));
        else if (instance.Title.Length > 200)
            errors.Add(new Error("Tickets.Validation.TitleTooLong", "Title must be 200 characters or fewer."));
        return errors;
    }
}
