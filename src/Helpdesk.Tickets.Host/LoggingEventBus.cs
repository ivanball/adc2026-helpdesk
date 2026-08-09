using Helpdesk.SharedKernel;

namespace Helpdesk.Tickets.Host;

/// <summary>
/// THE ONE REAL TODO. In this process there is no Notifications handler to call, and an
/// in-process bus does not cross process boundaries. This implementation logs the publish so
/// the extraction demo stays honest. A broker transport (RabbitMQ, Azure Service Bus, Redis)
/// replaces exactly this one class and one DI line: no handler and no outbox line changes.
/// </summary>
internal sealed class LoggingEventBus(ILogger<LoggingEventBus> logger) : IEventBus
{
    public Task Publish(IIntegrationEvent integrationEvent, CancellationToken ct)
    {
        logger.LogInformation(
            "PUBLISH (would go to the broker here): {EventType} {EventId}",
            integrationEvent.GetType().Name,
            integrationEvent.Id);
        return Task.CompletedTask;
    }
}
