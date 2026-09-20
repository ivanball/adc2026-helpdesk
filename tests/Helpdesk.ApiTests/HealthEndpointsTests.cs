using System.Net;

namespace Helpdesk.ApiTests;

/// <summary>
/// The health endpoints are mapped in every environment, so they are reachable from a probe
/// outside the process. These two tests are the headless proof; the dashboard is the demo.
/// </summary>
[Collection(ApiCollection.Name)]
public class HealthEndpointsTests(HelpdeskApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Liveness_answers_healthy()
    {
        var response = await _client.GetAsync("/alive");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Readiness_answers_healthy_when_the_database_is_reachable()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }
}
