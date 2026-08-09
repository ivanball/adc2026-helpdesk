using Helpdesk.SharedKernel;
using Helpdesk.Tickets.Application.Abstractions;
using Helpdesk.Tickets.Domain;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Tickets.Infrastructure;

public sealed class TicketsDbContext(DbContextOptions<TicketsDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Ticket> Tickets => Set<Ticket>();

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
    }

    Task IUnitOfWork.SaveChangesAsync(CancellationToken ct) => SaveChangesAsync(ct);
}
