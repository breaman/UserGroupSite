using System.Net;
using System.Net.Http.Json;
using UserGroupSite.Shared.Services;
using UserGroupSite.Shared.Topics;

namespace UserGroupSite.Client.Services;

/// <summary>Client-side implementation that manages topic suggestions via the API.</summary>
public sealed class TopicSuggestionService : ITopicSuggestionService
{
    private readonly HttpClient _httpClient;

    public TopicSuggestionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TopicSuggestionDto>> GetTopicSuggestionsAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<IReadOnlyList<TopicSuggestionDto>>("/api/topics");
        return result ?? [];
    }

    /// <inheritdoc />
    public async Task<EventServiceResult<CreateTopicSuggestionResponse>> CreateTopicSuggestionAsync(
        CreateTopicSuggestionRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/topics", request);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return EventServiceResult<CreateTopicSuggestionResponse>.Failure(
                string.IsNullOrWhiteSpace(error) ? "Failed to create topic suggestion." : error);
        }

        var created = await response.Content.ReadFromJsonAsync<CreateTopicSuggestionResponse>();
        return created is not null
            ? EventServiceResult<CreateTopicSuggestionResponse>.Success(created)
            : EventServiceResult<CreateTopicSuggestionResponse>.Failure("Failed to create topic suggestion.");
    }

    /// <inheritdoc />
    public async Task<EventServiceResult> LikeTopicSuggestionAsync(int topicSuggestionId)
    {
        var response = await _httpClient.PostAsync($"/api/topics/{topicSuggestionId}/like", null);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return EventServiceResult.Failure("Topic suggestion not found.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return EventServiceResult.Failure(
                string.IsNullOrWhiteSpace(error) ? "Failed to like topic." : error);
        }

        return EventServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<EventServiceResult> VolunteerForTopicAsync(int topicSuggestionId)
    {
        var response = await _httpClient.PostAsync($"/api/topics/{topicSuggestionId}/volunteer", null);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return EventServiceResult.Failure("Topic suggestion not found.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return EventServiceResult.Failure(
                string.IsNullOrWhiteSpace(error) ? "Failed to volunteer for topic." : error);
        }

        return EventServiceResult.Success();
    }
}
