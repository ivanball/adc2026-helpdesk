using Helpdesk.SharedKernel;

namespace Helpdesk.ArchitectureTests;

public class PlaceholderTests
{
    [Fact]
    public void SharedKernel_is_reachable()
    {
        Assert.Equal("SharedKernel", ModuleInfo.Name);
    }
}
