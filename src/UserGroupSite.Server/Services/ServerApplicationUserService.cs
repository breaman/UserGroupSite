using Microsoft.EntityFrameworkCore;
using UserGroupSite.Client.Services;
using UserGroupSite.Shared.DTOs;

namespace UserGroupSite.Server.Services;

public class ServerApplicationUserService(ApplicationDbContext DbContext) : IApplicationUserService
{
    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        return await DbContext.Users.Select(c => c.ToDto()).ToListAsync();
    }
}