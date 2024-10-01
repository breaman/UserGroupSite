namespace UserGroupSite.Client.Services;

public interface IApplicationUserService
{
    public Task<List<UserDto>> GetAllUsersAsync();
}