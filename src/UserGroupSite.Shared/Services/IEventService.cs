using UserGroupSite.Shared.Events;

namespace UserGroupSite.Shared.Services;

/// <summary>Service for retrieving and updating events.</summary>
public interface IEventService
{
    /// <summary>Gets the event edit details for a given slug.</summary>
    /// <param name="slug">The event slug to retrieve.</param>
    /// <returns>The event edit response or an error.</returns>
    Task<EventServiceResult<EventEditResponse>> GetEventAsync(string slug);

    /// <summary>Updates an event with the provided details.</summary>
    /// <param name="slug">The slug of the event to update.</param>
    /// <param name="request">The update request.</param>
    /// <returns>The outcome of the update operation.</returns>
    Task<EventServiceResult> UpdateEventAsync(string slug, UpdateEventRequest request);
}
