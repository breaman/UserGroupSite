namespace UserGroupSite.Client.Services;

public interface ICategoryService
{
    public Task<List<CategoryDto>> GetAllCategoriesAsync();
}