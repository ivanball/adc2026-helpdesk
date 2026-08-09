using System.Text.Json;
using Helpdesk.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Helpdesk.Tickets.Infrastructure;

internal sealed class OutboxRelay(IServiceScopeFactory scopeFactory, ILogger<OutboxRelay> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using (var scope = scopeFactory.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<TicketsDbContext>();
                var bus = scope.ServiceProvider.GetRequiredService<IEventBus>();

                var pending = await db.OutboxMessages
                    .Where(m => m.ProcessedOnUtc == null)
                    .OrderBy(m => m.OccurredOnUtc)
                    .Take(20)
                    .ToListAsync(stoppingToken);

                foreach (var message in pending)
                {
                    try
                    {
                        var type = Type.GetType(message.Type)!;
                        var integrationEvent =
                            (IIntegrationEvent)JsonSerializer.Deserialize(message.Content, type)!;
                        await bus.Publish(integrationEvent, stoppingToken);
                        message.ProcessedOnUtc = DateTime.UtcNow;
                    }
                    catch (Exception ex)
                    {
                        message.Error = ex.Message;
                        logger.LogError(ex, "Outbox message {Id} failed", message.Id);
                    }
                }

                await db.SaveChangesAsync(stoppingToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
