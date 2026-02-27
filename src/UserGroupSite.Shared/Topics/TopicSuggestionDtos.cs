namespace UserGroupSite.Shared.Topics;

/// <summary>Represents a topic suggestion displayed in the list.</summary>
public sealed record TopicSuggestionDto(
    int Id,
    string Title,
    string Description,
    int LikeCount,
    bool HasLiked,
    string SubmittedBy,
    DateTime SubmittedOnUtc,
    int? VolunteerSpeakerId,
    string? VolunteerSpeakerName);

/// <summary>Represents a request to create a new topic suggestion.</summary>
public sealed record CreateTopicSuggestionRequest(
    string Title,
    string Description);

/// <summary>Represents the response after creating a topic suggestion.</summary>
public sealed record CreateTopicSuggestionResponse(int Id);
