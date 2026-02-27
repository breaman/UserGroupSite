using System.ComponentModel.DataAnnotations;

namespace UserGroupSite.Data.Models;

/// <summary>Represents a topic suggestion submitted by a user.</summary>
public class TopicSuggestion : FingerPrintEntityBase
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = "";

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = "";

    /// <summary>The user who volunteered to speak on this topic, if any.</summary>
    public int? VolunteerSpeakerId { get; set; }

    /// <summary>Navigation property for the volunteer speaker.</summary>
    public User? VolunteerSpeaker { get; set; }

    /// <summary>Navigation property for the likes on this topic.</summary>
    public ICollection<TopicSuggestionLike> Likes { get; set; } = new List<TopicSuggestionLike>();
}
