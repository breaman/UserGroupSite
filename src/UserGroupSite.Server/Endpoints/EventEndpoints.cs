using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using UserGroupSite.Data.Models;
using UserGroupSite.Shared.Events;
using UserGroupSite.Shared.Services;

namespace UserGroupSite.Server.Endpoints;

public static class EventEndpoints
{
    public static WebApplication MapEventEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/events")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

        group.MapGet("/speakers", GetSpeakersAsync);
        group.MapPost("/", CreateEventAsync);

        return app;
    }

    private static async Task<Ok<IReadOnlyList<SpeakerOption>>> GetSpeakersAsync(ISpeakerService speakerService)
    {
        var options = await speakerService.GetSpeakersAsync();
        return TypedResults.Ok(options);
    }

    private static async Task<Results<Created<CreateEventResponse>, BadRequest<string>>> CreateEventAsync(
        CreateEventRequest request,
        ApplicationDbContext dbContext,
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
            Description = request.Description.Trim(),
            Location = request.Location.Trim(),
            EventDateTime = normalizedEventDateTime
        };

        foreach (var speakerId in speakerIds)
        {
            eventEntity.Speakers.Add(new EventSpeaker
            {
                SpeakerId = speakerId
            });
        }

        dbContext.Events.Add(eventEntity);
        await dbContext.SaveChangesAsync();

        return TypedResults.Created($"/api/events/{eventEntity.Id}", new CreateEventResponse(eventEntity.Id));
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
