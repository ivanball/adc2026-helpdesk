using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Helpdesk.ModuleKit;

/// <summary>
/// One implementation per module. The host discovers them and calls both members in a
/// foreach: this is a convention, not a framework.
/// </summary>
public interface IModule
{
    void Add(IServiceCollection services, IConfiguration configuration);

    void MapEndpoints(IEndpointRouteBuilder endpoints);
}
