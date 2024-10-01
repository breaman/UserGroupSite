using Microsoft.EntityFrameworkCore;
using UserGroupSite.Client.Services;
using UserGroupSite.Shared.DTOs;

namespace UserGroupSite.Server.Services;

public class ServerEventService(ApplicationDbContext DbContext) : IEventService
{
    public async Task<EventDto> GetEventByIdAsync(int eventId)
    {
        return await DbContext.SpeakingEvents.Where(se => se.Id == eventId).Select(se => se.ToDto()).SingleOrDefaultAsync();
    }
}