using System.ComponentModel.DataAnnotations;

namespace UserGroupSite.Data.Models;

public class Event : FingerPrintEntityBase
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = "";

    [Required]
    [MaxLength(200)]
    public string Slug { get; set; } = "";

    [MaxLength(500)]
    public string ShortDescription { get; set; } = "";

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = "";

    public DateTime EventDateTime { get; set; }

    [Required]
    [MaxLength(200)]
    public string Location { get; set; } = "";

    public bool IsPublished { get; set; } = false;

    /// <summary>The Meetup.com event ID, populated after the event is first published to Meetup.</summary>
    [MaxLength(50)]
    public string? MeetupEventId { get; set; }

    public ICollection<EventSpeaker> Speakers { get; set; } = new List<EventSpeaker>();

    public ICollection<EventComment> Comments { get; set; } = new List<EventComment>();
}
