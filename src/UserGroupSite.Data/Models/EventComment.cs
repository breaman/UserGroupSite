using System.ComponentModel.DataAnnotations;

namespace UserGroupSite.Data.Models;

/// <summary>Represents a comment left on an event by an authenticated user.</summary>
public class EventComment : FingerPrintEntityBase
{
    /// <summary>The ID of the event this comment belongs to.</summary>
    public int EventId { get; set; }

    /// <summary>Navigation property for the parent event.</summary>
    public Event Event { get; set; } = default!;

    /// <summary>The ID of the user who authored the comment.</summary>
    public int AuthorId { get; set; }

    /// <summary>Navigation property for the comment author.</summary>
    public User Author { get; set; } = default!;

    /// <summary>The raw Markdown content of the comment.</summary>
    [Required]
    [MaxLength(4000)]
    public string Content { get; set; } = "";
}
