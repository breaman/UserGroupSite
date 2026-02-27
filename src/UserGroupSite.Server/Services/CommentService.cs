using Microsoft.EntityFrameworkCore;
using UserGroupSite.Data.Interfaces;
using UserGroupSite.Data.Models;
using UserGroupSite.Shared.Comments;
using UserGroupSite.Shared.Services;

namespace UserGroupSite.Server.Services;

/// <summary>Server-side implementation that retrieves and creates comments from the database.</summary>
public sealed class CommentService : ICommentService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly IUserService _userService;

    /// <summary>Initializes a new instance of the <see cref="CommentService"/> class.</summary>
    /// <param name="dbContextFactory">The factory used to create isolated DbContext instances.</param>
    /// <param name="userService">The user service used to resolve the current user ID.</param>
    public CommentService(IDbContextFactory<ApplicationDbContext> dbContextFactory, IUserService userService)
    {
        _dbContextFactory = dbContextFactory;
        _userService = userService;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CommentDto>> GetCommentsAsync(string eventSlug)
    {
        if (string.IsNullOrWhiteSpace(eventSlug))
        {
            return [];
        }

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var comments = await dbContext.EventComments
            .AsNoTracking()
            .Include(c => c.Author)
            .Where(c => c.Event.Slug == eventSlug)
            .OrderBy(c => c.CreatedOn)
            .Select(c => new CommentDto(
                c.Id,
                BuildDisplayName(c.Author),
                c.Content,
                c.CreatedOn ?? DateTime.UtcNow))
            .ToListAsync();

        return comments;
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

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return EventServiceResult<CreateCommentResponse>.Failure("Comment content is required.");
        }

        if (request.Content.Length > 4000)
        {
            return EventServiceResult<CreateCommentResponse>.Failure("Comment content must be 4000 characters or fewer.");
        }

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var eventEntity = await dbContext.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Slug == eventSlug && e.IsPublished);

        if (eventEntity is null)
        {
            return EventServiceResult<CreateCommentResponse>.Failure("Event not found.");
        }

        var comment = new EventComment
        {
            EventId = eventEntity.Id,
            AuthorId = _userService.UserId,
            Content = request.Content.Trim()
        };

        dbContext.EventComments.Add(comment);
        await dbContext.SaveChangesAsync();

        return EventServiceResult<CreateCommentResponse>.Success(new CreateCommentResponse(comment.Id));
    }

    /// <summary>Builds a display name from a user, preferring first/last name over email.</summary>
    private static string BuildDisplayName(User user)
    {
        if (!string.IsNullOrWhiteSpace(user.FirstName) || !string.IsNullOrWhiteSpace(user.LastName))
        {
            return $"{user.FirstName} {user.LastName}".Trim();
        }

        return user.Email ?? "Unknown";
    }
}
