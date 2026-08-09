using System.Text.Json;
using Helpdesk.SharedKernel;
using Helpdesk.Tickets.Application.Abstractions;
using Helpdesk.Tickets.Domain;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Tickets.Infrastructure;

public sealed class TicketsDbContext(DbContextOptions<TicketsDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Ticket> Tickets => Set<Ticket>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ticket>(builder =>
        {
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Title).HasMaxLength(200);
            builder.Ignore(t => t.DomainEvents);
            builder.OwnsMany(t => t.ChildTasks, child =>
            {
                child.ToTable("ChildTasks");
                child.WithOwner().HasForeignKey("TicketId");
                child.HasKey(c => c.Id);
                child.Property(c => c.Description).HasMaxLength(500);
            });
        });

        modelBuilder.Entity<OutboxMessage>(builder =>
        {
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Type).HasMaxLength(500);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var outboxMessages = ChangeTracker.Entries<Entity>()
            .SelectMany(entry =>
            {
                var events = entry.Entity.DomainEvents.ToList();
                entry.Entity.ClearDomainEvents();
                return events;
            })
            .Select(TicketEventMapper.ToIntegrationEvent)
            .Where(e => e is not null)
            .Select(e => new OutboxMessage
            {
                Id = e!.Id,
                Type = e.GetType().AssemblyQualifiedName!,
                Content = JsonSerializer.Serialize(e, e.GetType()),
                OccurredOnUtc = e.OccurredOnUtc,
            })
            .ToList();

        OutboxMessages.AddRange(outboxMessages);

        return await base.SaveChangesAsync(ct);   // ONE transaction: ticket + outbox rows
    }

    Task IUnitOfWork.SaveChangesAsync(CancellationToken ct) => SaveChangesAsync(ct);
}
