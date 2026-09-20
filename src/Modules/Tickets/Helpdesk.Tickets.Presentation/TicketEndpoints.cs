using Helpdesk.Tickets.Application.Abstractions;
using Helpdesk.Tickets.Application.Tickets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Helpdesk.Tickets.Presentation;

public sealed record CreateTicketRequest(string Title);

public static class TicketEndpoints
{
    public static IEndpointRouteBuilder MapTicketEndpoints(this IEndpointRouteBuilder app)
    {
        // Slices add endpoints here.
        // Shortcut: take the concrete handler straight out of the container so the
        // request skips the validation and logging decorators.
        app.MapPost("/tickets", async (CreateTicketRequest request,
            CreateTicketHandler handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new CreateTicket(request.Title), ct);
            return result.IsSuccess
                ? Results.Created($"/tickets/{result.Value}", new { id = result.Value })
                : Results.BadRequest(new { result.Error.Code, result.Error.Message });
        });

        app.MapPost("/tickets/{id:guid}/close", async (Guid id,
            ICommandHandler<CloseTicket, bool> handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new CloseTicket(id), ct);
            if (result.IsSuccess) return Results.NoContent();
            return result.Error.Code == "Tickets.NotFound"
                ? Results.NotFound()
                : Results.BadRequest(new { result.Error.Code, result.Error.Message });
        });

        app.MapGet("/tickets/{id:guid}", async (Guid id,
            IQueryHandler<GetTicketById, TicketResponse> handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new GetTicketById(id), ct);
            if (result.IsSuccess) return Results.Ok(result.Value);
            return result.Error.Code == "Tickets.NotFound"
                ? Results.NotFound()
                : Results.BadRequest(new { result.Error.Code, result.Error.Message });
        });

        return app;
    }
}
