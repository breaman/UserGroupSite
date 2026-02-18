namespace UserGroupSite.Data.Models;

public class EventSpeaker
{
    public int EventId { get; set; }
    public Event Event { get; set; } = default!;

    public int SpeakerId { get; set; }
    public User Speaker { get; set; } = default!;
}
