using Helpdesk.SharedKernel;
using static Helpdesk.SharedKernel.Result;

namespace Helpdesk.Tickets.Domain;

public enum TicketStatus { Open, Closed }

public static class TicketErrors
{
    public static readonly Error EmptyTitle =
        new("Tickets.EmptyTitle", "A ticket needs a non-empty title.");
    public static readonly Error AlreadyClosed =
        new("Tickets.AlreadyClosed", "The ticket is already closed.");
    public static readonly Error OpenChildTasks =
        new("Tickets.OpenChildTasks", "Close all child tasks before closing the ticket.");
    public static readonly Error UnknownChildTask =
        new("Tickets.UnknownChildTask", "No such child task on this ticket.");
    public static readonly Error NotFound =
        new("Tickets.NotFound", "Ticket not found.");
}

public sealed record TicketCreated(Guid TicketId, DateTime OccurredOnUtc) : IDomainEvent;
public sealed record TicketClosed(Guid TicketId, DateTime OccurredOnUtc) : IDomainEvent;

public sealed class Ticket : Entity
{
    private readonly List<ChildTask> _childTasks = [];

    private Ticket() { }   // EF Core + nobody else

    public Guid Id { get; private set; }
    public string Title { get; private set; } = null!;
    public TicketStatus Status { get; private set; }
    public IReadOnlyList<ChildTask> ChildTasks => _childTasks;

    public static Result<Ticket> Create(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Failure<Ticket>(TicketErrors.EmptyTitle);

        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Status = TicketStatus.Open,
        };
        ticket.Raise(new TicketCreated(ticket.Id, DateTime.UtcNow));
        return Success(ticket);
    }

    public Result AddChildTask(string description)
    {
        if (Status == TicketStatus.Closed)
            return Failure(TicketErrors.AlreadyClosed);

        _childTasks.Add(new ChildTask(Guid.NewGuid(), description));
        return Success();
    }

    public Result CompleteChildTask(Guid taskId)
    {
        var task = _childTasks.FirstOrDefault(t => t.Id == taskId);
        if (task is null)
            return Failure(TicketErrors.UnknownChildTask);

        task.MarkDone();
        return Success();
    }

    public Result Close()
    {
        if (Status == TicketStatus.Closed)
            return Failure(TicketErrors.AlreadyClosed);

        if (_childTasks.Any(t => !t.IsDone))
            return Failure(TicketErrors.OpenChildTasks);

        Status = TicketStatus.Closed;
        Raise(new TicketClosed(Id, DateTime.UtcNow));
        return Success();
    }
}

public sealed class ChildTask
{
    private ChildTask() { }

    internal ChildTask(Guid id, string description)
    {
        Id = id;
        Description = description;
    }

    public Guid Id { get; private set; }
    public string Description { get; private set; } = null!;
    public bool IsDone { get; private set; }

    internal void MarkDone() => IsDone = true;
}
