using System.Net;
using System.Net.Http.Json;
using UserGroupSite.Shared.Comments;
using UserGroupSite.Shared.Services;

namespace UserGroupSite.Client.Services;

/// <summary>Client-side implementation that retrieves and creates comments via the API.</summary>
public sealed class CommentService : ICommentService
{
    private readonly HttpClient _httpClient;

    /// <summary>Initializes a new instance of the <see cref="CommentService"/> class.</summary>
    /// <param name="httpClient">The HTTP client used to call the comments API.</param>
    public CommentService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CommentDto>> GetCommentsAsync(string eventSlug)
    {
        if (string.IsNullOrWhiteSpace(eventSlug))
        {
            return [];
        }

        var comments = await _httpClient.GetFromJsonAsync<IReadOnlyList<CommentDto>>(
            $"/api/events/{Uri.EscapeDataString(eventSlug)}/comments");

        return comments ?? [];
    }

    /// <inheritdoc />
    public async Task<EventServiceResult<CreateCommentResponse>> CreateCommentAsync(
        string eventSlug,
        CreateCommentRequest request)
    {
        if (string.IsNullOrWhiteSpace(eventSlug))
        {
            return EventServiceResult<CreateCommentResponse>.Failure("Event slug is required.");
        }

        var response = await _httpClient.PostAsJsonAsync(
            $"/api/events/{Uri.EscapeDataString(eventSlug)}/comments",
            request);

        if (response.StatusCode is HttpStatusCode.NotFound)
        {
            return EventServiceResult<CreateCommentResponse>.Failure("Event not found.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var serverError = await response.Content.ReadAsStringAsync();
            var message = string.IsNullOrWhiteSpace(serverError)
                ? "Failed to create comment."
                : serverError;

            return EventServiceResult<CreateCommentResponse>.Failure(message);
        }

        var result = await response.Content.ReadFromJsonAsync<CreateCommentResponse>();
        if (result is null)
        {
            return EventServiceResult<CreateCommentResponse>.Failure("Failed to read comment response.");
        }

        return EventServiceResult<CreateCommentResponse>.Success(result);
    }
}
