using Microsoft.AspNetCore.Identity;
using UserGroupSite.Data.Interfaces;

namespace UserGroupSite.Data.Models;

public class Role : IdentityRole<int>, IEntityBase
{
}