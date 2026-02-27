using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using UserGroupSite.Data.Models;
using UserGroupSite.Shared.Events;
using UserGroupSite.Shared.Services;

namespace UserGroupSite.Server.Endpoints;

public static class EventEndpoints
{
    public static WebApplication MapEventEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/events")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,Speaker" });

        group.MapGet("/speakers", GetSpeakersAsync);
        group.MapPost("/", CreateEventAsync)
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
        group.MapGet("/{slug}", GetEventBySlugAsync);
        group.MapPut("/{slug}", UpdateEventAsync);

        return app;
    }

    private static async Task<Ok<IReadOnlyList<SpeakerOption>>> GetSpeakersAsync(ISpeakerService speakerService)
    {
        var options = await speakerService.GetSpeakersAsync();
        return TypedResults.Ok(options);
    }

    private static async Task<Results<Created<CreateEventResponse>, BadRequest<string>>> CreateEventAsync(
        CreateEventRequest request,
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
        ISpeakerService speakerService)
    {
        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Description) ||
            string.IsNullOrWhiteSpace(request.Location))
        {
            return TypedResults.BadRequest("Name, description, and location are required.");
        }

        if (request.EventDateTimeUtc == default)
        {
            return TypedResults.BadRequest("Event date/time is required.");
        }

        var normalizedEventDateTime = NormalizeToUtc(request.EventDateTimeUtc);
        var speakerIds = request.SpeakerIds?.Distinct().ToArray() ?? Array.Empty<int>();
        var eligibleSpeakers = await speakerService.GetSpeakersAsync();
        var eligibleIds = eligibleSpeakers.Select(speaker => speaker.Id).ToHashSet();

        if (speakerIds.Length > 0)
        {
            var invalidIds = speakerIds.Where(id => !eligibleIds.Contains(id)).ToArray();
            if (invalidIds.Length > 0)
            {
                return TypedResults.BadRequest("One or more speakers are not eligible.");
            }
        }

        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? ToKebabCase(request.Name)
            : ToKebabCase(request.Slug);

        if (string.IsNullOrWhiteSpace(slug))
        {
            return TypedResults.BadRequest("Slug is required.");
        }

        var eventEntity = new Event
        {
            Name = request.Name.Trim(),
            Slug = slug,
            ShortDescription = request.ShortDescription?.Trim() ?? "",
            Description = request.Description.Trim(),
            Location = request.Location.Trim(),
            EventDateTime = normalizedEventDateTime,
            IsPublished = request.IsPublished
        };

        foreach (var speakerId in speakerIds)
        {
            eventEntity.Speakers.Add(new EventSpeaker
            {
                SpeakerId = speakerId
            });
        }

        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.Events.Add(eventEntity);
        await dbContext.SaveChangesAsync();

        return TypedResults.Created($"/api/events/{eventEntity.Id}", new CreateEventResponse(eventEntity.Id));
    }

    private static async Task<Results<Ok<EventEditResponse>, NotFound, ForbidHttpResult>> GetEventBySlugAsync(
        string slug,
        IEventService eventService,
        HttpContext httpContext)
    {
        var result = await eventService.GetEventAsync(slug);
        if (!result.IsSuccess || result.Value is null)
        {
            return TypedResults.NotFound();
        }

        if (!httpContext.User.IsInRole("Admin") && !IsSpeakerForEvent(httpContext.User, result.Value))
        {
            return TypedResults.Forbid();
        }

        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<NoContent, NotFound, BadRequest<string>, ForbidHttpResult>> UpdateEventAsync(
        string slug,
        UpdateEventRequest request,
        IEventService eventService,
        HttpContext httpContext)
    {
        if (!httpContext.User.IsInRole("Admin"))
        {
            var existingResult = await eventService.GetEventAsync(slug);
            if (!existingResult.IsSuccess || existingResult.Value is null)
            {
                return TypedResults.NotFound();
            }

            if (!IsSpeakerForEvent(httpContext.User, existingResult.Value))
            {
                return TypedResults.Forbid();
            }
        }

        var result = await eventService.UpdateEventAsync(slug, request);
        if (!result.IsSuccess)
        {
            if (string.Equals(result.ErrorMessage, "Event not found.", StringComparison.OrdinalIgnoreCase))
            {
                return TypedResults.NotFound();
            }

            return TypedResults.BadRequest(result.ErrorMessage ?? "Failed to update event.");
        }

        return TypedResults.NoContent();
    }

    private static bool IsSpeakerForEvent(ClaimsPrincipal user, EventEditResponse eventEditResponse)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userId, out var id) && eventEditResponse.SpeakerIds.Contains(id);
    }

    private static DateTime NormalizeToUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }

    private static string ToKebabCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length);
        var previousWasSeparator = false;
        var previousWasLowerOrDigit = false;

        foreach (var c in value.Trim())
        {
            if (char.IsLetterOrDigit(c))
            {
                if (char.IsUpper(c) && previousWasLowerOrDigit && !previousWasSeparator)
                {
                    builder.Append('-');
                }

                builder.Append(char.ToLowerInvariant(c));
                previousWasSeparator = false;
                previousWasLowerOrDigit = char.IsLower(c) || char.IsDigit(c);
                continue;
            }

            if (!previousWasSeparator && builder.Length > 0)
            {
                builder.Append('-');
                previousWasSeparator = true;
                previousWasLowerOrDigit = false;
            }
        }

        return builder.ToString().Trim('-');
    }
}
