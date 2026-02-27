namespace UserGroupSite.Shared.Services;

/// <summary>Service for publishing events to Meetup.com via its GraphQL API.</summary>
public interface IMeetupService
{
    /// <summary>
    /// Creates an event on Meetup.com and returns the Meetup event ID.
    /// Returns <c>null</c> if the service is not configured or the call fails.
    /// </summary>
    /// <param name="title">The event title.</param>
    /// <param name="description">The event description (HTML or plain text).</param>
    /// <param name="startDateTimeUtc">The event start date/time in UTC.</param>
    /// <param name="location">The event venue or location description.</param>
    /// <returns>The Meetup event ID on success, or <c>null</c> on failure or if not configured.</returns>
    Task<string?> CreateEventAsync(string title, string description, DateTime startDateTimeUtc, string location);
}
