using System.ComponentModel.DataAnnotations;
using System.Text;

namespace UserGroupSite.Client.Components.Pages.Admin;

/// <summary>Represents the event form input model shared between create and edit forms.</summary>
public class EventFormInput
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = "";

    [Required]
    [MaxLength(200)]
    public string Slug { get; set; } = "";

    [MaxLength(500)]
    public string ShortDescription { get; set; } = "";

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = "";

    [Required]
    public string EventDateTimeLocal { get; set; } = "";

    [Required]
    [MaxLength(200)]
    public string Location { get; set; } = "";

    public HashSet<int> SpeakerIds { get; set; } = new();

    public bool IsPublished { get; set; } = false;

    /// <summary>Converts event name to kebab-case slug.</summary>
    public string ToKebabCase(string value)
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

    /// <summary>Resets the form to empty state.</summary>
    public void Reset()
    {
        Name = "";
        Slug = "";
        ShortDescription = "";
        Description = "";
        EventDateTimeLocal = "";
        Location = "";
        SpeakerIds.Clear();
        IsPublished = false;
    }

    /// <summary>Copies data from this model to a new instance.</summary>
    public EventFormInput Clone()
    {
        var clone = new EventFormInput
        {
            Name = Name,
            Slug = Slug,
            ShortDescription = ShortDescription,
            Description = Description,
            EventDateTimeLocal = EventDateTimeLocal,
            Location = Location,
            IsPublished = IsPublished
        };

        foreach (var speakerId in SpeakerIds)
        {
            clone.SpeakerIds.Add(speakerId);
        }

        return clone;
    }
}
