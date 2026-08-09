namespace Helpdesk.SharedKernel;

public interface IIntegrationEvent
{
    Guid Id { get; }
    DateTime OccurredOnUtc { get; }
}

public interface IEventBus
{
    Task Publish(IIntegrationEvent integrationEvent, CancellationToken ct);
}

public interface IIntegrationEventHandler<in TEvent> where TEvent : IIntegrationEvent
{
    Task Handle(TEvent integrationEvent, CancellationToken ct);
}
