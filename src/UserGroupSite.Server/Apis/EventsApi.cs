using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedConstants = UserGroupSite.Shared.Models.Constants;

namespace UserGroupSite.Server.Apis;

public static class EventsApi
{
    public static IEndpointConventionBuilder MapEventsApi(this IEndpointRouteBuilder endpoints)
    {
        var eventsGroup = endpoints.MapGroup(SharedConstants.EventApiUrl);
        eventsGroup.RequireAuthorization(SharedConstants.IsAdmin);
        
        eventsGroup.MapPost("/delete", async (
            ApplicationDbContext dbContext,
            [FromForm] int eventId) =>
        {
            var eventToDelete = await dbContext.SpeakingEvents.SingleOrDefaultAsync(c => c.Id == eventId);
            dbContext.SpeakingEvents.Remove(eventToDelete);
            await dbContext.SaveChangesAsync();
            return TypedResults.LocalRedirect("/Admin/ManageEvents");
        });
        
        return eventsGroup;
    }
}