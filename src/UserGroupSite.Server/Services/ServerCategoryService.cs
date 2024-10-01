using Microsoft.EntityFrameworkCore;
using UserGroupSite.Client.Services;
using UserGroupSite.Shared.DTOs;

namespace UserGroupSite.Server.Services;

public class ServerCategoryService(ApplicationDbContext DbContext, ILogger<ServerCategoryService> Logger) : ICategoryService
{
    public async Task<List<CategoryDto>> GetAllCategoriesAsync()
    {
        Logger.LogInformation("Getting all categories from the server.");
        return await DbContext.Categories.Select(c => c.ToDto()).ToListAsync();
    }
}