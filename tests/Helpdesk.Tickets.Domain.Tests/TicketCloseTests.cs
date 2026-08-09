using Helpdesk.Tickets.Domain;

namespace Helpdesk.Tickets.Domain.Tests;

public class TicketCloseTests
{
    [Fact]
    public void Close_with_open_child_tasks_fails()
    {
        var ticket = Ticket.Create("Printer on fire").Value;
        ticket.AddChildTask("Extinguish printer");

        var result = ticket.Close();

        Assert.True(result.IsFailure);
        Assert.Equal(TicketErrors.OpenChildTasks, result.Error);
        Assert.Equal(TicketStatus.Open, ticket.Status);
    }

    [Fact]
    public void Close_with_all_child_tasks_done_succeeds_and_raises_TicketClosed()
    {
        var ticket = Ticket.Create("Printer on fire").Value;
        ticket.AddChildTask("Extinguish printer");
        ticket.CompleteChildTask(ticket.ChildTasks[0].Id);

        var result = ticket.Close();

        Assert.True(result.IsSuccess);
        Assert.Equal(TicketStatus.Closed, ticket.Status);
        Assert.Contains(ticket.DomainEvents, e => e is TicketClosed);
    }

    [Fact]
    public void Close_when_already_closed_fails()
    {
        var ticket = Ticket.Create("Printer on fire").Value;
        ticket.Close();

        var result = ticket.Close();

        Assert.True(result.IsFailure);
        Assert.Equal(TicketErrors.AlreadyClosed, result.Error);
    }

    [Fact]
    public void Create_with_blank_title_fails()
    {
        var result = Ticket.Create("   ");

        Assert.True(result.IsFailure);
        Assert.Equal(TicketErrors.EmptyTitle, result.Error);
    }
}
