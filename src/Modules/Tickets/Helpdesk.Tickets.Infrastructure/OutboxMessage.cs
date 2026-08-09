namespace Helpdesk.Tickets.Infrastructure;

public sealed class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = null!;        // CLR type name of the event
    public string Content { get; set; } = null!;     // JSON payload
    public DateTime OccurredOnUtc { get; set; }
    public DateTime? ProcessedOnUtc { get; set; }
    public string? Error { get; set; }
}
