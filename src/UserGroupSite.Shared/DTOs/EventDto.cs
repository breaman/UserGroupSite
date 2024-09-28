using System.Globalization;
using FluentValidation;
using NodaTime;

namespace UserGroupSite.Shared.DTOs;

public class EventDto
{
    public int EventId { get; set; }
    public string? Title { get; set; }
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public DateTime? EventDate { get; set; }
    public DateTime? EventDateUtc {
        get
        {
            var localDate = LocalDateTime.FromDateTime(this.EventDate ?? DateTime.Now);
            var pacificTime = DateTimeZoneProviders.Tzdb["America/Los_Angeles"];
            var pacificZonedDateTime = pacificTime.AtStrictly(localDate);
            return pacificZonedDateTime.ToDateTimeUtc();
        }
    }
    public bool IsPublished { get; set; }
    public int CategoryId { get; set; }
    public int SpeakerId { get; set; }
    public string? CategoryName { get; set; }
    public string? CategoryColor { get; set; }
    public string? SpeakerName { get; set; }
    
    public string ConvertedDateTime
    {
        get
        {
            var localDateTime = LocalDateTime.FromDateTime(EventDate!.Value);
            return localDateTime.ToString("MMM dd, yyyy hh:mm tt", DateTimeFormatInfo.CurrentInfo);
        }
    }
}

public class EventDtoValidator : AbstractValidator<EventDto>
{
    public EventDtoValidator()
    {
        RuleFor(viewModel => viewModel.Title).NotEmpty();
        RuleFor(viewModel => viewModel.Slug).NotEmpty();
        RuleFor(viewModel => viewModel.Description).NotEmpty();
        RuleFor(viewModel => viewModel.EventDate).NotEmpty();
        RuleFor(viewModel => viewModel.CategoryId).GreaterThan(0);
        RuleFor(viewModel => viewModel.SpeakerId).GreaterThan(0);
    }
}