using System.Net;
using System.Net.Http.Json;

namespace Helpdesk.ApiTests;

public class CloseTicketTests(HelpdeskApiFactory factory) : IClassFixture<HelpdeskApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Closing_an_open_ticket_succeeds_and_the_ticket_reads_back_as_closed()
    {
        var id = await CreateTicket("Printer on fire");

        var close = await _client.PostAsync($"/tickets/{id}/close", null);

        Assert.Equal(HttpStatusCode.NoContent, close.StatusCode);
        var ticket = await _client.GetFromJsonAsync<TicketView>($"/tickets/{id}");
        Assert.Equal("Closed", ticket!.Status);
    }

    [Fact]
    public async Task Closing_the_same_ticket_twice_is_rejected_as_a_bad_request()
    {
        var id = await CreateTicket("Monitor flickering");
        await _client.PostAsync($"/tickets/{id}/close", null);

        var secondClose = await _client.PostAsync($"/tickets/{id}/close", null);

        Assert.Equal(HttpStatusCode.BadRequest, secondClose.StatusCode);
        var error = await secondClose.Content.ReadFromJsonAsync<ErrorView>();
        Assert.Equal("Tickets.AlreadyClosed", error!.Code);
    }

    private async Task<Guid> CreateTicket(string title)
    {
        var response = await _client.PostAsJsonAsync("/tickets", new { title });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<CreatedTicketView>();
        return created!.Id;
    }

    private sealed record CreatedTicketView(Guid Id);

    private sealed record TicketView(Guid Id, string Title, string Status);

    private sealed record ErrorView(string Code, string Message);
}
