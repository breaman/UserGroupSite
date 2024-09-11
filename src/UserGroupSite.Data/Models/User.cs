using Microsoft.AspNetCore.Identity;
using UserGroupSite.Data.Interfaces;

namespace UserGroupSite.Data.Models;

public class User : IdentityUser<int>, IEntityBase
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime MemberSince { get; set; }
}