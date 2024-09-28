using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using UserGroupSite.Shared.DTOs;

namespace UserGroupSite.Server.Components.Admin.Pages;

public partial class ManageEvents : ComponentBase
{
    private List<EventDto> _speakingEvents = [];

    [Inject] private ApplicationDbContext DbContext { get; set; } = default!;
    
    protected override async Task OnInitializedAsync()
    {
        var speakingEvents = await DbContext.SpeakingEvents
            .Include(se => se.Category)
            .Include(se => se.Speaker)
            .OrderByDescending(se => se.EventDate)
            .ToListAsync();
        _speakingEvents = speakingEvents.Select(se => se.ToDto()).ToList();
    }
}