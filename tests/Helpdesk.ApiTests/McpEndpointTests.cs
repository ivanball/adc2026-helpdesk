using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace Helpdesk.ApiTests;

/// <summary>
/// The MCP endpoint is tested the way the HTTP endpoints are: through the real host the
/// shared factory boots, over the factory's in-memory handler. The SDK client speaks
/// streamable HTTP over any HttpClient, so no port is opened and no process is launched.
/// </summary>
[Collection(ApiCollection.Name)]
public class McpEndpointTests(HelpdeskApiFactory factory)
{
    private async Task<McpClient> ConnectAsync()
    {
        var httpClient = factory.CreateClient();
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp,
            },
            httpClient);

        return await McpClient.CreateAsync(transport);
    }

    [Fact]
    public async Task The_server_advertises_exactly_the_three_ticket_tools()
    {
        await using var client = await ConnectAsync();

        var tools = await client.ListToolsAsync();

        string[] expected = ["close_ticket", "create_ticket", "get_ticket"];
        string[] advertised = [.. tools.Select(t => t.Name).OrderBy(n => n, StringComparer.Ordinal)];
        Assert.Equal(expected, advertised);
    }

    [Fact]
    public async Task Creating_a_ticket_then_reading_it_back_round_trips_the_title()
    {
        await using var client = await ConnectAsync();

        var id = await CreateTicket(client, "Projector cable missing");
        var read = await client.CallToolAsync("get_ticket", new Dictionary<string, object?> { ["ticketId"] = id });

        Assert.False(read.IsError is true, TextOf(read));
        Assert.Contains("Projector cable missing", TextOf(read), StringComparison.Ordinal);
        Assert.Contains("Open", TextOf(read), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Closing_the_same_ticket_twice_comes_back_as_a_tool_error()
    {
        await using var client = await ConnectAsync();
        var id = await CreateTicket(client, "Badge reader dead");

        var first = await client.CallToolAsync("close_ticket", new Dictionary<string, object?> { ["ticketId"] = id });
        var second = await client.CallToolAsync("close_ticket", new Dictionary<string, object?> { ["ticketId"] = id });

        Assert.False(first.IsError is true, TextOf(first));
        Assert.True(second.IsError);
        Assert.Contains("Tickets.AlreadyClosed", TextOf(second), StringComparison.Ordinal);
    }

    private static async Task<string> CreateTicket(McpClient client, string title)
    {
        var created = await client.CallToolAsync("create_ticket", new Dictionary<string, object?> { ["title"] = title });
        Assert.False(created.IsError is true, TextOf(created));
        return Guid.Parse(TextOf(created)).ToString();
    }

    private static string TextOf(CallToolResult result) =>
        string.Concat(result.Content.OfType<TextContentBlock>().Select(c => c.Text));
}
