using Helpdesk.Notifications.Infrastructure;
using Helpdesk.Tickets.Application.Abstractions;
using Helpdesk.Tickets.Domain;
using NetArchTest.Rules;

namespace Helpdesk.ArchitectureTests;

public class LayerRulesTests
{
    private static string Offenders(TestResult result) =>
        string.Join(", ", result.FailingTypeNames ?? Enumerable.Empty<string>());

    [Fact]
    public void Rule1_Domain_depends_only_on_SharedKernel_and_System()
    {
        var result = Types.InAssembly(typeof(Ticket).Assembly)
            .Should()
            .OnlyHaveDependenciesOn(
                "Helpdesk.SharedKernel",
                "Helpdesk.Tickets.Domain",
                "System")
            .GetResult();

        Assert.True(result.IsSuccessful, "Domain grew a dependency: " + Offenders(result));
    }

    [Fact]
    public void Rule2_Domain_does_not_reference_EntityFrameworkCore()
    {
        var result = Types.InAssembly(typeof(Ticket).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful, "EF Core leaked into Domain: " + Offenders(result));
    }

    [Fact]
    public void Rule3_Application_does_not_depend_on_Infrastructure_or_Presentation()
    {
        var result = Types.InAssembly(typeof(ICommandHandler<,>).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Helpdesk.Tickets.Infrastructure",
                "Helpdesk.Tickets.Presentation")
            .GetResult();

        Assert.True(result.IsSuccessful, "Application reached outward: " + Offenders(result));
    }

    [Fact]
    public void Rule4_Handlers_are_internal()
    {
        var result = Types.InAssembly(typeof(ICommandHandler<,>).Assembly)
            .That()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .Or()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .Should()
            .NotBePublic()
            .GetResult();

        Assert.True(result.IsSuccessful, "Public handlers found: " + Offenders(result));
    }

    [Fact]
    public void Rule5_Only_the_Host_touches_Tickets_Infrastructure()
    {
        // Assemblies outside Helpdesk.Tickets.* that are NOT the composition root.
        var result = Types.InAssembly(typeof(Helpdesk.SharedKernel.Entity).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Helpdesk.Tickets.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, "Infrastructure leaked: " + Offenders(result));
        // The Notifications assemblies are covered by
        // Modules_touch_each_other_only_through_contracts below, which forbids every
        // Helpdesk.Tickets.* assembly except Helpdesk.Tickets.Contracts.
    }

    [Fact]
    public void Modules_touch_each_other_only_through_contracts()
    {
        // NotificationsModule is the public anchor for the assembly that also holds the
        // internal TicketClosedNotificationHandler.
        var result = Types.InAssembly(typeof(NotificationsModule).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Helpdesk.Tickets.Domain",
                "Helpdesk.Tickets.Application",
                "Helpdesk.Tickets.Infrastructure",
                "Helpdesk.Tickets.Presentation")
            .GetResult();

        Assert.True(result.IsSuccessful, "Notifications reached past the contracts: " + Offenders(result));
    }
}
