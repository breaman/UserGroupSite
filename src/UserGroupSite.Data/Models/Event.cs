using System.ComponentModel.DataAnnotations;

namespace UserGroupSite.Data.Models;

public class Event : FingerPrintEntityBase
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

    public DateTime EventDateTime { get; set; }

    [Required]
    [MaxLength(200)]
    public string Location { get; set; } = "";

    public bool IsPublished { get; set; } = false;

    public ICollection<EventSpeaker> Speakers { get; set; } = new List<EventSpeaker>();
}
