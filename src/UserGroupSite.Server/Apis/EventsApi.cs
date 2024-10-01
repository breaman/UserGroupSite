using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserGroupSite.Shared.DTOs;
using SharedConstants = UserGroupSite.Shared.Models.Constants;

namespace UserGroupSite.Server.Apis;

public static class EventsApi
{
    public static IEndpointConventionBuilder MapEventsApi(this IEndpointRouteBuilder endpoints)
    {
        var eventsGroup = endpoints.MapGroup(SharedConstants.EventApiUrl);
        eventsGroup.RequireAuthorization(SharedConstants.IsAdmin);

        eventsGroup.MapGet("/{eventId:int}", async (int eventId, ApplicationDbContext dbContext) =>
        {
            return TypedResults.Ok(await dbContext.SpeakingEvents.Where(se => se.Id == eventId)
                .Select(se => se.ToDto())
                .SingleOrDefaultAsync());
        });
        
        eventsGroup.MapPost("/create", async Task<Results<Created<EventDto>, ValidationProblem>> (EventDto dto,
            ApplicationDbContext dbContext, ILogger<EventDto> logger) =>
        {
            var speakingEvent = new SpeakingEvent();
            speakingEvent.FromDto(dto);
            
            dbContext.SpeakingEvents.Add(speakingEvent);
            
            try
            {
                await dbContext.SaveChangesAsync();
                logger.LogInformation("Successfully created Event {title}", dto.Title);

                dto.EventId = speakingEvent.Id;
                return TypedResults.Created("", dto);
            }
            catch (Exception e)
            {
                logger.LogError("Unable to create Event {title} with the following exception: {exception}",
                    dto.Title, e);
                Dictionary<string, string[]> problems = new();
                problems.Add("error",
                    new[]
                    {
                        "An error occurred while trying to save the Event. If this continues please contact support."
                    });
                return TypedResults.ValidationProblem(problems);
            }
        });
        
        eventsGroup.MapPut("/update/{eventId:int}", async Task<Results<Ok<EventDto>, ValidationProblem>> (int eventId,
            EventDto dto, ApplicationDbContext dbContext, ILogger<EventDto> logger) =>
        {
            var existingEvent =
                await dbContext.SpeakingEvents.SingleOrDefaultAsync(p => p.Id == dto.EventId);

            if (existingEvent is not null)
            {
                existingEvent.FromDto(dto);

                try
                {
                    await dbContext.SaveChangesAsync();
                    logger.LogInformation("Successfully updated Event {title}", dto.Title);
                    return TypedResults.Ok(dto);
                }
                catch (Exception e)
                {
                    logger.LogError("Unable to update Event {title} do to the following exception: {exception}",
                        dto.Title, e);
                    Dictionary<string, string[]> problems = new();
                    problems.Add("error",
                    [
                        "An error occurred while trying to update the Event. If this continues please contact support."
                    ]);
                    return TypedResults.ValidationProblem(problems);
                }
            }
            else
            {
                logger.LogError(
                    "Unable to update Event: {title} since it does not exist in the system",
                    dto.Title);
                Dictionary<string, string[]> problems = new();
                problems.Add("error",
                [
                    "Unable to update a Event that has does not exist in the system."
                ]);
                return TypedResults.ValidationProblem(problems);
            }
        });
        
        // eventsGroup.MapPost("/delete", async (
        //     ApplicationDbContext dbContext,
        //     [FromForm] int eventId) =>
        // {
        //     var eventToDelete = await dbContext.SpeakingEvents.SingleOrDefaultAsync(c => c.Id == eventId);
        //     dbContext.SpeakingEvents.Remove(eventToDelete);
        //     await dbContext.SaveChangesAsync();
        //     return TypedResults.LocalRedirect("/Admin/ManageEvents");
        // });
        eventsGroup.MapPost("/delete/{eventId:int}", async (
            ApplicationDbContext dbContext,
            int eventId) =>
        {
            var eventToDelete = await dbContext.SpeakingEvents.SingleOrDefaultAsync(c => c.Id == eventId);
            dbContext.SpeakingEvents.Remove(eventToDelete);
            await dbContext.SaveChangesAsync();
            return TypedResults.Ok();
            // return TypedResults.LocalRedirect("/Admin/ManageEvents");
        });
        
        return eventsGroup;
    }
}