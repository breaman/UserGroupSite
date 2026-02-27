namespace UserGroupSite.Data.Models;

/// <summary>Tracks a single user liking a topic suggestion. One like per user per topic.</summary>
public class TopicSuggestionLike
{
    public int TopicSuggestionId { get; set; }
    public TopicSuggestion TopicSuggestion { get; set; } = default!;

    public int UserId { get; set; }
    public User User { get; set; } = default!;
}
