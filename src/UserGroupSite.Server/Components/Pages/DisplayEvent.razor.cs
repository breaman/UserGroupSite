using Microsoft.EntityFrameworkCore;
using UserGroupSite.Shared.DTOs;

namespace UserGroupSite.Server.Components.Pages;

public partial class DisplayEvent : ComponentBase
{
    private EventDto? _eventDto = default;
    
    [Parameter] public string Slug { get; set; } = string.Empty;

    [Inject] private ApplicationDbContext DbContext { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        var speakingEvent = await DbContext.SpeakingEvents
            .Include(se => se.Category)
            .Include(se => se.Speaker)
            .SingleOrDefaultAsync(ev => ev.Slug == Slug);
        
        _eventDto = speakingEvent?.ToDto();
    }
}