using Microsoft.EntityFrameworkCore;
using UserGroupSite.Shared.DTOs;

namespace UserGroupSite.Server.Components.Pages;

public partial class Home : ComponentBase
{
    List<EventDto> _speakingEvents { get; set; } = new();
    private EventDto? _upcomingEvent { get; set; }
    [Inject] private ApplicationDbContext DbContext { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        var speakingEvents = await DbContext.SpeakingEvents
            .Include(se => se.Category)
            .Include(se => se.Speaker)
            .Where(se => se.IsPublished)
            .OrderByDescending(se => se.EventDate)
            .ToListAsync();
        _speakingEvents = speakingEvents.Select(se => se.ToDto()).ToList();

        _upcomingEvent = _speakingEvents
            .Where(se => se.EventDateUtc > DateTime.UtcNow)
            .OrderBy(se => se.EventDate)
            .FirstOrDefault();
    }
}