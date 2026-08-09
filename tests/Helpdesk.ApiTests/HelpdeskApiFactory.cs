using Helpdesk.Notifications.Infrastructure;
using Helpdesk.Tickets.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Helpdesk.ApiTests;

/// <summary>
/// Boots the real Host in memory and points it at the workshop SQL Server container,
/// using databases of its own so a test run never touches development data.
/// One database per module here too, because that is what the host expects.
/// </summary>
public sealed class HelpdeskApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string TicketsConnectionString =
        "Server=localhost,14330;Database=Helpdesk_ApiTests;User Id=sa;Password=Workshop!Passw0rd;TrustServerCertificate=True";

    private const string NotificationsConnectionString =
        "Server=localhost,14330;Database=Helpdesk_ApiTests_Notifications;User Id=sa;Password=Workshop!Passw0rd;TrustServerCertificate=True";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
        => builder
            .UseSetting("ConnectionStrings:Tickets", TicketsConnectionString)
            .UseSetting("ConnectionStrings:Notifications", NotificationsConnectionString);

    /// <summary>Drops and re-creates both test databases from the migrations, once per run.</summary>
    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();

        var tickets = scope.ServiceProvider.GetRequiredService<TicketsDbContext>().Database;
        await tickets.EnsureDeletedAsync();
        await tickets.MigrateAsync();

        var notifications = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>().Database;
        await notifications.EnsureDeletedAsync();
        await notifications.MigrateAsync();
    }

    // WebApplicationFactory already owns teardown; this satisfies xUnit's async lifetime.
    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;
}
