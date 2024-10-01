using System.ComponentModel.DataAnnotations;

namespace UserGroupSite.Data.Models;

public class TopicSuggestion : FingerPrintEntityBase
{
    [MaxLength(50)]
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public DateTime? SuggestedPresentationSlot { get; set; }
    public int? VolunteeredSpeakerId { get; set; }
    public User? VolunteeredSpeaker { get; set; }
    public bool IsApproved { get; set; }
}