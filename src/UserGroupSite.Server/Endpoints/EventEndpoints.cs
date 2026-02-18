using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using UserGroupSite.Data.Models;
using UserGroupSite.Shared.Events;

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

    private static async Task<Ok<IReadOnlyList<SpeakerOption>>> GetSpeakersAsync(UserManager<User> userManager)
    {
        var users = await GetEligibleSpeakersAsync(userManager);
        var options = users
            .OrderBy(user => user.LastName)
            .ThenBy(user => user.FirstName)
            .ThenBy(user => user.Email)
            .Select(user => new SpeakerOption(
                user.Id,
                BuildDisplayName(user),
                user.Email ?? string.Empty))
            .ToList();

        return TypedResults.Ok<IReadOnlyList<SpeakerOption>>(options);
    }

    private static async Task<Results<Created<CreateEventResponse>, BadRequest<string>>> CreateEventAsync(
        CreateEventRequest request,
        ApplicationDbContext dbContext,
        UserManager<User> userManager)
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
        var eligibleUsers = await GetEligibleSpeakersAsync(userManager);
        var eligibleIds = eligibleUsers.Select(user => user.Id).ToHashSet();

        if (speakerIds.Length > 0)
        {
            var invalidIds = speakerIds.Where(id => !eligibleIds.Contains(id)).ToArray();
            if (invalidIds.Length > 0)
            {
                return TypedResults.BadRequest("One or more speakers are not eligible.");
            }
        }

        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? ToSnakeCase(request.Name)
            : ToSnakeCase(request.Slug);

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

    private static async Task<List<User>> GetEligibleSpeakersAsync(UserManager<User> userManager)
    {
        var speakers = await userManager.GetUsersInRoleAsync("Speaker");
        var admins = await userManager.GetUsersInRoleAsync("Admin");

        return speakers
            .Concat(admins)
            .DistinctBy(user => user.Id)
            .ToList();
    }

    private static string BuildDisplayName(User user)
    {
        var first = user.FirstName?.Trim();
        var last = user.LastName?.Trim();

        if (!string.IsNullOrWhiteSpace(first) && !string.IsNullOrWhiteSpace(last))
        {
            return $"{first} {last}";
        }

        if (!string.IsNullOrWhiteSpace(first))
        {
            return first;
        }

        if (!string.IsNullOrWhiteSpace(last))
        {
            return last;
        }

        return user.Email ?? "Unknown";
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

    private static string ToSnakeCase(string value)
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
                    builder.Append('_');
                }

                builder.Append(char.ToLowerInvariant(c));
                previousWasSeparator = false;
                previousWasLowerOrDigit = char.IsLower(c) || char.IsDigit(c);
                continue;
            }

            if (!previousWasSeparator && builder.Length > 0)
            {
                builder.Append('_');
                previousWasSeparator = true;
                previousWasLowerOrDigit = false;
            }
        }

        return builder.ToString().Trim('_');
    }
}
