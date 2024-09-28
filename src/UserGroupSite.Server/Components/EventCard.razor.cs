using System.Globalization;
using NodaTime;
using UserGroupSite.Shared.DTOs;

namespace UserGroupSite.Server.Components;

public partial class EventCard : ComponentBase
{
    [Parameter] public EventDto SpeakingEvent { get; set; } = new();
    [Parameter] public bool IsEditMode { get; set; } = false;

    private string NavigateUrl => IsEditMode ? $"/admin/editevent/{SpeakingEvent.EventId}" : $"/event/{SpeakingEvent.Slug}";

    private string ShortenedDescription => SpeakingEvent.Description.Length > 100
        ? $"{SpeakingEvent.Description.Substring(0, 100)}..."
        : SpeakingEvent.Description;
}