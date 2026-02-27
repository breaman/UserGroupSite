using UserGroupSite.Shared.Topics;

namespace UserGroupSite.Shared.Services;

/// <summary>Service for managing topic suggestions.</summary>
public interface ITopicSuggestionService
{
    /// <summary>Gets all topic suggestions ordered by like count descending.</summary>
    /// <returns>A read-only list of topic suggestions.</returns>
    Task<IReadOnlyList<TopicSuggestionDto>> GetTopicSuggestionsAsync();

    /// <summary>Creates a new topic suggestion.</summary>
    /// <param name="request">The creation request.</param>
    /// <returns>The result of the creation operation.</returns>
    Task<EventServiceResult<CreateTopicSuggestionResponse>> CreateTopicSuggestionAsync(CreateTopicSuggestionRequest request);

    /// <summary>Toggles a like on a topic suggestion for the current user.</summary>
    /// <param name="topicSuggestionId">The topic suggestion ID to like.</param>
    /// <returns>The result of the like operation.</returns>
    Task<EventServiceResult> LikeTopicSuggestionAsync(int topicSuggestionId);

    /// <summary>Volunteers the current user to speak on a topic suggestion.</summary>
    /// <param name="topicSuggestionId">The topic suggestion ID to volunteer for.</param>
    /// <returns>The result of the volunteer operation.</returns>
    Task<EventServiceResult> VolunteerForTopicAsync(int topicSuggestionId);
}
