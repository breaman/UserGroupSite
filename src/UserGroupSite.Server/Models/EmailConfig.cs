namespace UserGroupSite.Server.Models;

public class EmailConfig
{
    public string? ServiceName { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? FromAddress { get; set; }
    public string? FromName { get; set; }
}