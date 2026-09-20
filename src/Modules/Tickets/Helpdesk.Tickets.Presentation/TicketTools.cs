using System.ComponentModel;
using Helpdesk.SharedKernel;
using Helpdesk.Tickets.Application.Abstractions;
using Helpdesk.Tickets.Application.Tickets;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Helpdesk.Tickets.Presentation;

/// <summary>
/// The MCP adapter for the Tickets module. Three tools, one per HTTP endpoint in
/// <see cref="TicketEndpoints"/>, injecting the same handler interfaces those endpoints
/// inject. No rule lives here: a model calling close_ticket goes through exactly the
/// validation, the decorators and the aggregate that a curl calling POST /close goes through.
/// The descriptions below are what the model reads, so they are the only prose that matters.
/// </summary>
[McpServerToolType]
public static class TicketTools
{
    [McpServerTool(Name = "create_ticket"),
     Description("Opens a new helpdesk ticket and returns its identifier.")]
    public static async Task<CallToolResult> CreateTicketAsync(
        ICommandHandler<CreateTicket, Guid> handler,
        [Description("What is wrong, in one line. Required, 200 characters or fewer.")] string title,
        CancellationToken ct)
    {
        var result = await handler.Handle(new CreateTicket(title), ct);
        return result.IsSuccess ? Ok(result.Value.ToString()) : Failed(result.Error);
    }

    [McpServerTool(Name = "close_ticket", Idempotent = false),
     Description("Closes one open ticket. Refuses a ticket that is already closed or that still has open child tasks.")]
    public static async Task<CallToolResult> CloseTicketAsync(
        ICommandHandler<CloseTicket, bool> handler,
        [Description("Identifier of the single ticket to close, as returned by create_ticket.")] Guid ticketId,
        CancellationToken ct)
    {
        var result = await handler.Handle(new CloseTicket(ticketId), ct);
        return result.IsSuccess ? Ok($"Ticket {ticketId} is closed.") : Failed(result.Error);
    }

    [McpServerTool(Name = "get_ticket", ReadOnly = true),
     Description("Reads one ticket by identifier and returns its title, status and child task count.")]
    public static async Task<CallToolResult> GetTicketAsync(
        IQueryHandler<GetTicketById, TicketResponse> handler,
        [Description("Identifier of the single ticket to read.")] Guid ticketId,
        CancellationToken ct)
    {
        var result = await handler.Handle(new GetTicketById(ticketId), ct);
        if (result.IsFailure) return Failed(result.Error);

        var ticket = result.Value;
        return Ok($"{ticket.Id} | {ticket.Status} | {ticket.Title} | {ticket.ChildTasks.Count} child task(s)");
    }

    private static CallToolResult Ok(string text) =>
        new() { Content = [new TextContentBlock { Text = text }] };

    // A refused business rule is not a bug, so it is never an exception here. It comes back
    // as a tool error result carrying the same Error.Code the HTTP endpoints put in the body,
    // which is what lets the model read the refusal and stop instead of retrying blindly.
    private static CallToolResult Failed(Error error) =>
        new()
        {
            IsError = true,
            Content = [new TextContentBlock { Text = $"{error.Code}: {error.Message}" }],
        };
}
