using Helpdesk.Tickets.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Helpdesk.ApiTests;

/// <summary>
/// Boots the real Host in memory and points it at the workshop SQL Server container,
/// using a database of its own so a test run never touches development data.
/// </summary>
public sealed class HelpdeskApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string TestConnectionString =
        "Server=localhost,14330;Database=Helpdesk_ApiTests;User Id=sa;Password=Workshop!Passw0rd;TrustServerCertificate=True";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
        => builder.UseSetting("ConnectionStrings:Tickets", TestConnectionString);

    /// <summary>Drops and re-creates the test database from the migrations, once per run.</summary>
    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<TicketsDbContext>().Database;
        await database.EnsureDeletedAsync();
        await database.MigrateAsync();
    }

    // WebApplicationFactory already owns teardown; this satisfies xUnit's async lifetime.
    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;
}
