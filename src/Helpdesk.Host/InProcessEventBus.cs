using Helpdesk.SharedKernel;

namespace Helpdesk.Host;

/// <summary>
/// Today's transport: the handlers live in this process, so publishing is a DI lookup plus a
/// method call. The interface (IEventBus) is where a broker transport plugs in later.
/// </summary>
internal sealed class InProcessEventBus(IServiceScopeFactory scopeFactory) : IEventBus
{
    public async Task Publish(IIntegrationEvent integrationEvent, CancellationToken ct)
    {
        var handlerType = typeof(IIntegrationEventHandler<>)
            .MakeGenericType(integrationEvent.GetType());
        var handleMethod = handlerType.GetMethod(nameof(IIntegrationEventHandler<IIntegrationEvent>.Handle))!;

        await using var scope = scopeFactory.CreateAsyncScope();

        foreach (var handler in scope.ServiceProvider.GetServices(handlerType))
        {
            if (handler is null)
            {
                continue;
            }

            await (Task)handleMethod.Invoke(handler, [integrationEvent, ct])!;
        }
    }
}
