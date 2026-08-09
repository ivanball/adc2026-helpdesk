using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Notifications.Infrastructure;

public sealed class Notification
{
    public Guid Id { get; set; }
    public string Message { get; set; } = null!;
    public DateTime CreatedOnUtc { get; set; }
}

public sealed class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options)
    : DbContext(options)
{
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(builder =>
        {
            builder.HasKey(n => n.Id);
            builder.Property(n => n.Message).HasMaxLength(500);
        });
    }
}
