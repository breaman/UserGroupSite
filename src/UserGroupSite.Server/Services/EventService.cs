using Microsoft.EntityFrameworkCore;
using UserGroupSite.Data.Models;
using UserGroupSite.Shared.Events;
using UserGroupSite.Shared.Services;

namespace UserGroupSite.Server.Services;

/// <summary>Server-side implementation that retrieves and updates events from the database.</summary>
public sealed class EventService : IEventService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly ISpeakerService _speakerService;
    private readonly IMeetupService _meetupService;

    /// <summary>Initializes a new instance of the <see cref="EventService"/> class.</summary>
    /// <param name="dbContextFactory">The factory used to create isolated DbContext instances.</param>
    /// <param name="speakerService">The speaker service used to validate speaker eligibility.</param>
    /// <param name="meetupService">The Meetup.com service used to publish events externally.</param>
    public EventService(
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
        ISpeakerService speakerService,
        IMeetupService meetupService)
    {
        _dbContextFactory = dbContextFactory;
        _speakerService = speakerService;
        _meetupService = meetupService;
    }

    /// <inheritdoc />
    public async Task<EventServiceResult<EventEditResponse>> GetEventAsync(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return EventServiceResult<EventEditResponse>.Failure("Event slug is required.");
        }

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var eventEntity = await dbContext.Events
            .AsNoTracking()
            .Include(e => e.Speakers)
            .FirstOrDefaultAsync(e => e.Slug == slug);

        if (eventEntity is null)
        {
            return EventServiceResult<EventEditResponse>.Failure("Event not found.");
        }

        var response = new EventEditResponse(
            eventEntity.Id,
            eventEntity.Name,
            eventEntity.Slug,
            eventEntity.ShortDescription,
            eventEntity.Description,
            eventEntity.EventDateTime,
            eventEntity.Location,
            eventEntity.Speakers.Select(speaker => speaker.SpeakerId).ToArray(),
            eventEntity.IsPublished);

        return EventServiceResult<EventEditResponse>.Success(response);
    }

    /// <inheritdoc />
    public async Task<EventServiceResult> UpdateEventAsync(string slug, UpdateEventRequest request)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return EventServiceResult.Failure("Event slug is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Description) ||
            string.IsNullOrWhiteSpace(request.Location))
        {
            return EventServiceResult.Failure("Name, description, and location are required.");
        }

        if (request.EventDateTimeUtc == default)
        {
            return EventServiceResult.Failure("Event date/time is required.");
        }

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var eventEntity = await dbContext.Events
            .Include(e => e.Speakers)
            .FirstOrDefaultAsync(e => e.Slug == slug);

        if (eventEntity is null)
        {
            return EventServiceResult.Failure("Event not found.");
        }

        var normalizedEventDateTime = NormalizeToUtc(request.EventDateTimeUtc);
        var speakerIds = request.SpeakerIds?.Distinct().ToArray() ?? Array.Empty<int>();
        var eligibleSpeakers = await _speakerService.GetSpeakersAsync();
        var eligibleIds = eligibleSpeakers.Select(speaker => speaker.Id).ToHashSet();

        if (speakerIds.Length > 0)
        {
            var invalidIds = speakerIds.Where(id => !eligibleIds.Contains(id)).ToArray();
            if (invalidIds.Length > 0)
            {
                return EventServiceResult.Failure("One or more speakers are not eligible.");
            }
        }

        eventEntity.Name = request.Name.Trim();
        eventEntity.ShortDescription = request.ShortDescription?.Trim() ?? "";
        eventEntity.Description = request.Description.Trim();
        eventEntity.Location = request.Location.Trim();
        eventEntity.EventDateTime = normalizedEventDateTime;
        eventEntity.IsPublished = request.IsPublished;

        eventEntity.Speakers.Clear();
        foreach (var speakerId in speakerIds)
        {
            eventEntity.Speakers.Add(new EventSpeaker
            {
                EventId = eventEntity.Id,
                SpeakerId = speakerId
            });
        }

        await dbContext.SaveChangesAsync();

        // Publish to Meetup.com when the event transitions to published and hasn't been posted yet
        if (eventEntity.IsPublished && string.IsNullOrWhiteSpace(eventEntity.MeetupEventId))
        {
            var meetupEventId = await _meetupService.CreateEventAsync(
                eventEntity.Name,
                eventEntity.Description,
                eventEntity.EventDateTime,
                eventEntity.Location);

            if (!string.IsNullOrWhiteSpace(meetupEventId))
            {
                eventEntity.MeetupEventId = meetupEventId;
                await dbContext.SaveChangesAsync();
            }
        }

        return EventServiceResult.Success();
    }

    /// <summary>Normalizes a date/time value to UTC.</summary>
    /// <param name="value">The date/time value to normalize.</param>
    /// <returns>The normalized UTC value.</returns>
    private static DateTime NormalizeToUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}
