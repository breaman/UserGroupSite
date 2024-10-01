using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedConstants = UserGroupSite.Shared.Models.Constants;

namespace UserGroupSite.Server.Apis;

public static class UsersApi
{
    public static IEndpointConventionBuilder MapUsersApi(this IEndpointRouteBuilder endpoints)
    {
        var usersGroup = endpoints.MapGroup(SharedConstants.UserApiUrl);
        usersGroup.RequireAuthorization(SharedConstants.IsAdmin);

        usersGroup.MapGet("/", async (ApplicationDbContext dbContext) =>
        {
            var userDtos = await dbContext.Users.Select(u => u.ToDto()).ToListAsync();
            return TypedResults.Ok(userDtos);
        });
        
        return usersGroup;
    }
}