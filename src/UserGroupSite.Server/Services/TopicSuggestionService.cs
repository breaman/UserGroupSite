using Microsoft.EntityFrameworkCore;
using UserGroupSite.Data.Interfaces;
using UserGroupSite.Data.Models;
using UserGroupSite.Shared.Services;
using UserGroupSite.Shared.Topics;

namespace UserGroupSite.Server.Services;

/// <summary>Server-side implementation that manages topic suggestions in the database.</summary>
public sealed class TopicSuggestionService : ITopicSuggestionService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly IUserService _userService;

    public TopicSuggestionService(
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
        IUserService userService)
    {
        _dbContextFactory = dbContextFactory;
        _userService = userService;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TopicSuggestionDto>> GetTopicSuggestionsAsync()
    {
        var currentUserId = _userService.UserId;

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var topics = await dbContext.TopicSuggestions
            .AsNoTracking()
            .Include(t => t.Likes)
            .Include(t => t.VolunteerSpeaker)
            .OrderByDescending(t => t.Likes.Count)
            .ThenByDescending(t => t.CreatedOn)
            .ToListAsync();

        // Collect creator user IDs and load their display names
        var creatorIds = topics.Select(t => t.CreatedBy).Distinct().ToArray();
        var creators = await dbContext.Users
            .AsNoTracking()
            .Where(u => creatorIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, BuildDisplayName);

        return topics.Select(t => new TopicSuggestionDto(
            t.Id,
            t.Title,
            t.Description,
            t.Likes.Count,
            t.Likes.Any(l => l.UserId == currentUserId),
            creators.GetValueOrDefault(t.CreatedBy, "Unknown"),
            t.CreatedOn ?? DateTime.UtcNow,
            t.VolunteerSpeakerId,
            t.VolunteerSpeaker is not null ? BuildDisplayName(t.VolunteerSpeaker) : null))
        .ToList();
    }

    /// <inheritdoc />
    public async Task<EventServiceResult<CreateTopicSuggestionResponse>> CreateTopicSuggestionAsync(
        CreateTopicSuggestionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return EventServiceResult<CreateTopicSuggestionResponse>.Failure("Title is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return EventServiceResult<CreateTopicSuggestionResponse>.Failure("Description is required.");
        }

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var topic = new TopicSuggestion
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim()
        };

        dbContext.TopicSuggestions.Add(topic);
        await dbContext.SaveChangesAsync();

        return EventServiceResult<CreateTopicSuggestionResponse>.Success(
            new CreateTopicSuggestionResponse(topic.Id));
    }

    /// <inheritdoc />
    public async Task<EventServiceResult> LikeTopicSuggestionAsync(int topicSuggestionId)
    {
        var currentUserId = _userService.UserId;

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var topic = await dbContext.TopicSuggestions
            .FirstOrDefaultAsync(t => t.Id == topicSuggestionId);

        if (topic is null)
        {
            return EventServiceResult.Failure("Topic suggestion not found.");
        }

        var existingLike = await dbContext.TopicSuggestionLikes
            .FirstOrDefaultAsync(l => l.TopicSuggestionId == topicSuggestionId && l.UserId == currentUserId);

        if (existingLike is not null)
        {
            dbContext.TopicSuggestionLikes.Remove(existingLike);
        }
        else
        {
            dbContext.TopicSuggestionLikes.Add(new TopicSuggestionLike
            {
                TopicSuggestionId = topicSuggestionId,
                UserId = currentUserId
            });
        }

        await dbContext.SaveChangesAsync();

        return EventServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<EventServiceResult> VolunteerForTopicAsync(int topicSuggestionId)
    {
        var currentUserId = _userService.UserId;

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var topic = await dbContext.TopicSuggestions
            .FirstOrDefaultAsync(t => t.Id == topicSuggestionId);

        if (topic is null)
        {
            return EventServiceResult.Failure("Topic suggestion not found.");
        }

        if (topic.VolunteerSpeakerId is not null)
        {
            return EventServiceResult.Failure("A volunteer has already been assigned to this topic.");
        }

        topic.VolunteerSpeakerId = currentUserId;
        await dbContext.SaveChangesAsync();

        return EventServiceResult.Success();
    }

    private static string BuildDisplayName(User user)
    {
        if (!string.IsNullOrWhiteSpace(user.FirstName) || !string.IsNullOrWhiteSpace(user.LastName))
        {
            return $"{user.FirstName} {user.LastName}".Trim();
        }

        return user.Email ?? "Unknown";
    }
}
