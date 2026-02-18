namespace UserGroupSite.Shared.Events;

/// <summary>Represents a selectable speaker for event creation.</summary>
public sealed record SpeakerOption(int Id, string DisplayName, string Email);

/// <summary>Represents a request to create an event.</summary>
public sealed record CreateEventRequest(
    string Name,
    string Slug,
    string Description,
    DateTime EventDateTimeUtc,
    string Location,
    IReadOnlyList<int> SpeakerIds);

/// <summary>Represents a response for a created event.</summary>
public sealed record CreateEventResponse(int Id);
