using Helpdesk.Tickets.Domain;

namespace Helpdesk.Tickets.Domain.Tests;

public class PlaceholderTests
{
    [Fact]
    public void Domain_module_is_named_Tickets()
    {
        Assert.Equal("Tickets", ModuleInfo.Name);
    }
}
