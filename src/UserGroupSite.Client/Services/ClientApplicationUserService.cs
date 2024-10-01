using System.Net.Http.Json;
using SharedConstants = UserGroupSite.Shared.Models.Constants;

namespace UserGroupSite.Client.Services;

public class ClientApplicationUserService(HttpClient Client) : IApplicationUserService
{
    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        return await Client.GetFromJsonAsync<List<UserDto>>($"{SharedConstants.UserApiUrl}");
    }
}