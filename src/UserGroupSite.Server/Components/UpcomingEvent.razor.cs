using UserGroupSite.Shared.DTOs;

namespace UserGroupSite.Server.Components;

public partial class UpcomingEvent : ComponentBase
{
    [Parameter] public EventDto SpeakingEvent { get; set; } = new();

    private string NavigateUrl => $"/event/{SpeakingEvent.Slug}";

    private string TimeTillEvent
    {
        get
        {
            var utcNow = DateTime.UtcNow;
            var difference = SpeakingEvent.EventDateUtc!.Value - utcNow;
            return $"In {difference.Days} Days {difference.Hours}:{difference.Minutes}:{difference.Seconds}";
        }
    }
}