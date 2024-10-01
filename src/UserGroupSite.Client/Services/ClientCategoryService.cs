using System.Net.Http.Json;
using SharedConstants = UserGroupSite.Shared.Models.Constants;

namespace UserGroupSite.Client.Services;

public class ClientCategoryService(HttpClient Client, ILogger<ClientCategoryService> Logger) : ICategoryService
{
    public async Task<List<CategoryDto>> GetAllCategoriesAsync()
    {
        Logger.LogInformation("Getting all categories via HttpClient.");
        return await Client.GetFromJsonAsync<List<CategoryDto>>($"{SharedConstants.CategoryApiUrl}");
    }
}