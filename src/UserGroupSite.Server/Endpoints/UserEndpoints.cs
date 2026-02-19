using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using UserGroupSite.Data.Models;
using UserGroupSite.Shared.Users;

namespace UserGroupSite.Server.Endpoints;

public static class UserEndpoints
{
    public static WebApplication MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/users")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

        group.MapGet("/", GetAllUsersAsync);
        group.MapGet("/me", GetCurrentUserAsync);
        group.MapPut("/{id}/roles", UpdateUserRolesAsync);

        return app;
    }

    private static async Task<Results<Ok<CurrentUserResponse>, BadRequest<string>>> GetCurrentUserAsync(
        UserManager<User> userManager,
        HttpContext httpContext)
    {
        var currentUser = await userManager.GetUserAsync(httpContext.User);
        if (currentUser == null)
        {
            return TypedResults.BadRequest("Unable to identify the current user.");
        }

        return TypedResults.Ok(new CurrentUserResponse(currentUser.Id));
    }

    private static async Task<Ok<IReadOnlyList<UserForManagement>>> GetAllUsersAsync(
        UserManager<User> userManager)
    {
        var users = userManager.Users.ToList();
        var result = new List<UserForManagement>();

        foreach (var user in users)
        {
            var isAdmin = await userManager.IsInRoleAsync(user, "Admin");
            var isSpeaker = await userManager.IsInRoleAsync(user, "Speaker");

            result.Add(new UserForManagement(
                user.Id,
                user.Email ?? "",
                user.FirstName,
                user.LastName,
                isAdmin,
                isSpeaker));
        }

        return TypedResults.Ok((IReadOnlyList<UserForManagement>)result);
    }

    private static async Task<Results<Ok<UpdateUserRolesResponse>, BadRequest<string>, NotFound>> UpdateUserRolesAsync(
        int id,
        UpdateUserRolesRequest request,
        UserManager<User> userManager,
        HttpContext httpContext)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return TypedResults.NotFound();
        }

        // Get the current admin user from the HTTP context
        var currentUser = await userManager.GetUserAsync(httpContext.User);
        if (currentUser == null)
        {
            return TypedResults.BadRequest("Unable to identify the current user.");
        }

        // Prevent removing admin role from the current user
        var isUserCurrentlyAdmin = await userManager.IsInRoleAsync(user, "Admin");
        if (isUserCurrentlyAdmin && !request.IsAdmin && user.Id == currentUser.Id)
        {
            return TypedResults.BadRequest("You cannot remove your own admin role.");
        }

        // Update Admin role
        if (request.IsAdmin && !isUserCurrentlyAdmin)
        {
            var addAdminResult = await userManager.AddToRoleAsync(user, "Admin");
            if (!addAdminResult.Succeeded)
            {
                return TypedResults.BadRequest("Failed to add admin role.");
            }
        }
        else if (!request.IsAdmin && isUserCurrentlyAdmin)
        {
            var removeAdminResult = await userManager.RemoveFromRoleAsync(user, "Admin");
            if (!removeAdminResult.Succeeded)
            {
                return TypedResults.BadRequest("Failed to remove admin role.");
            }
        }

        // Update Speaker role
        var isUserCurrentlySpeaker = await userManager.IsInRoleAsync(user, "Speaker");
        if (request.IsSpeaker && !isUserCurrentlySpeaker)
        {
            var addSpeakerResult = await userManager.AddToRoleAsync(user, "Speaker");
            if (!addSpeakerResult.Succeeded)
            {
                return TypedResults.BadRequest("Failed to add speaker role.");
            }
        }
        else if (!request.IsSpeaker && isUserCurrentlySpeaker)
        {
            var removeSpeakerResult = await userManager.RemoveFromRoleAsync(user, "Speaker");
            if (!removeSpeakerResult.Succeeded)
            {
                return TypedResults.BadRequest("Failed to remove speaker role.");
            }
        }

        return TypedResults.Ok(new UpdateUserRolesResponse(user.Id, request.IsAdmin, request.IsSpeaker));
    }
}
