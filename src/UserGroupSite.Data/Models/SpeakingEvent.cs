using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;
using UserGroupSite.Shared.DTOs;

namespace UserGroupSite.Data.Models;

public class SpeakingEvent : FingerPrintEntityBase
{
    [MaxLength(50)]
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    [MaxLength(70)]
    public string? Slug { get; set; }
    public bool IsPublished { get; set; }
    public int SpeakerId { get; set; }
    public User? Speaker { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    
    public EventDto ToDto()
    {
        var eventDto = new EventDto
        {
            EventId = Id,
            Title = Title,
            Description = Description,
            IsPublished = IsPublished,
            Slug = Slug,
            SpeakerId = SpeakerId,
            CategoryId = CategoryId,
            CategoryName = Category?.Name,
            CategoryColor = Category?.BackgroundColor,
            SpeakerName = $"{Speaker?.FirstName} {Speaker?.LastName}",
        };
        // convert UTC date to pacific - will probably need to figure out how to convert to "user time"
        var trueUtcTime = DateTime.SpecifyKind(EventDate, DateTimeKind.Utc);
        var utcDate = Instant.FromDateTimeUtc(trueUtcTime);
        var pacificTime = DateTimeZoneProviders.Tzdb["America/Los_Angeles"];
        var zonedTime = utcDate.InZone(pacificTime);
        eventDto.EventDate = zonedTime.ToDateTimeUnspecified();

        return eventDto;
    }

    public void FromDto(EventDto dto)
    {
        Title = dto.Title;
        Description = dto.Description;
        IsPublished = dto.IsPublished;
        // convert EventDate to pacific time, then to utc
        var localDate = LocalDateTime.FromDateTime(dto.EventDate ?? DateTime.Now);
        var pacificTime = DateTimeZoneProviders.Tzdb["America/Los_Angeles"];
        var pacificZonedDateTime = pacificTime.AtStrictly(localDate);
        EventDate = pacificZonedDateTime.ToDateTimeUtc();
        Slug = dto.Slug;
        SpeakerId = dto.SpeakerId;
        CategoryId = dto.CategoryId;
    }
}