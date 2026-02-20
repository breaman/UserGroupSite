using System.Net;
using System.Net.Http.Json;
using UserGroupSite.Shared.Events;
using UserGroupSite.Shared.Services;

namespace UserGroupSite.Client.Services;

/// <summary>Client-side implementation that retrieves and updates events via the API.</summary>
public sealed class EventService : IEventService
{
    private readonly HttpClient httpClient;

    /// <summary>Initializes a new instance of the <see cref="EventService"/> class.</summary>
    /// <param name="httpClient">The HTTP client used to call the events API.</param>
    public EventService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<EventServiceResult<EventEditResponse>> GetEventAsync(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return EventServiceResult<EventEditResponse>.Failure("Event slug is required.");
        }

        var response = await httpClient.GetAsync($"/api/events/{Uri.EscapeDataString(slug)}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return EventServiceResult<EventEditResponse>.Failure("Event not found.");
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            return EventServiceResult<EventEditResponse>.Failure("Access denied.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var serverError = await response.Content.ReadAsStringAsync();
            var message = string.IsNullOrWhiteSpace(serverError)
                ? "Failed to load event."
                : serverError;
            return EventServiceResult<EventEditResponse>.Failure(message);
        }

        var eventData = await response.Content.ReadFromJsonAsync<EventEditResponse>();
        if (eventData is null)
        {
            return EventServiceResult<EventEditResponse>.Failure("Failed to load event.");
        }

        return EventServiceResult<EventEditResponse>.Success(eventData);
    }

    /// <inheritdoc />
    public async Task<EventServiceResult> UpdateEventAsync(string slug, UpdateEventRequest request)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return EventServiceResult.Failure("Event slug is required.");
        }

        var response = await httpClient.PutAsJsonAsync($"/api/events/{Uri.EscapeDataString(slug)}", request);
        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            return EventServiceResult.Failure("Access denied.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var serverError = await response.Content.ReadAsStringAsync();
            var message = string.IsNullOrWhiteSpace(serverError)
                ? "Failed to update event."
                : serverError;
            return EventServiceResult.Failure(message);
        }

        return EventServiceResult.Success();
    }
}
