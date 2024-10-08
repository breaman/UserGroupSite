namespace UserGroupSite.Shared.Models;

public class ProblemDetails
{
    public string? Type { get; set; }
    public string? Title { get; set; }
    public int Status { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
}