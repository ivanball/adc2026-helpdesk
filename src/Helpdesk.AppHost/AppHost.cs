var builder = DistributedApplication.CreateBuilder(args);

// Aspire runs its own SQL Server container. It gets its own container name and whatever host
// port Aspire assigns, so the docker compose container on 14330 that labs 1 to 5 use keeps
// running beside it and neither one can take the other's port.
var sql = builder.AddSqlServer("sql")
    .WithContainerName("helpdesk-aspire-sql")
    .WithLifetime(ContainerLifetime.Persistent)   // survives a stop, so a restart is seconds
    .WithDataVolume("helpdesk-aspire-sql-data");  // and the data survives with it

// The resource name is the connection string name that WithReference injects. Calling this
// database "Tickets" is what lands it on ConnectionStrings:Tickets, which is the key the
// Tickets module already reads. The physical database is still HelpdeskTickets.
var tickets = sql.AddDatabase("Tickets", "HelpdeskTickets");

builder.AddProject<Projects.Helpdesk_Host>("api")
    .WithReference(tickets)

    // WaitFor gates startup on the SQL resource reporting healthy: that is liveness of a
    // dependency, and it is the right thing to wait on. A service must never gate its own
    // startup on its own readiness endpoint (/health), because /health only goes green after
    // the schema is in place, and the schema only arrives after the service starts. Wait on
    // what you depend on, report your own readiness, and never wait on yourself.
    .WaitFor(tickets)

    // Only the orchestrated run migrates on startup. Labs 1 to 5 keep using
    // dotnet ef database update against the compose container, so the default stays off.
    .WithEnvironment("Migrations__ApplyOnStartup", "true");

builder.Build().Run();
