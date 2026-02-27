using UserGroupSite.Shared.Comments;

namespace UserGroupSite.Shared.Services;

/// <summary>Service for retrieving and creating comments on events.</summary>
public interface ICommentService
{
    /// <summary>Gets all comments for a given event.</summary>
    /// <param name="eventSlug">The slug of the event to retrieve comments for.</param>
    /// <returns>A read-only list of comments ordered by creation date ascending.</returns>
    Task<IReadOnlyList<CommentDto>> GetCommentsAsync(string eventSlug);

    /// <summary>Creates a new comment on an event.</summary>
    /// <param name="eventSlug">The slug of the event to comment on.</param>
    /// <param name="request">The comment creation request.</param>
    /// <returns>The result of the creation operation.</returns>
    Task<EventServiceResult<CreateCommentResponse>> CreateCommentAsync(string eventSlug, CreateCommentRequest request);
}
