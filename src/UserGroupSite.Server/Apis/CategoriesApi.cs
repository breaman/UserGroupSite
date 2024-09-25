using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedConstants = UserGroupSite.Shared.Models.Constants;

namespace UserGroupSite.Server.Apis;

public static class CategoriesApi
{
    public static IEndpointConventionBuilder MapCategoriesApi(this IEndpointRouteBuilder endpoints)
    {
        var categoriesGroup = endpoints.MapGroup(SharedConstants.CategoryApiUrl);
        categoriesGroup.RequireAuthorization(SharedConstants.IsAdmin);

        categoriesGroup.MapPost("/delete", async (
            ApplicationDbContext dbContext,
            [FromForm] int categoryId) =>
        {
            var categoryToDelete = await dbContext.Categories.SingleOrDefaultAsync(c => c.Id == categoryId);
            dbContext.Categories.Remove(categoryToDelete);
            await dbContext.SaveChangesAsync();
            return TypedResults.LocalRedirect("/Admin/ManageCategories");
        });
        
        return categoriesGroup;
    }
}