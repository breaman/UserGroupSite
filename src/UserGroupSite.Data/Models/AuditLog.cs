namespace UserGroupSite.Data.Models;

public class AuditLog : EntityBase
{
    public int UserId { get; set; }
    public string Type { get; set; } = null!;
    public string TableName { get; set; } = null!;
    public DateTime DateTime { get; set; }
    public string? OldValues { get; set; }
    public string NewValues { get; set; } = null!;
    public string? AffectedColumns { get; set; }
    public string PrimaryKey { get; set; } = null!;
}