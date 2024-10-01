using System.Net.Http.Json;
using SharedConstants = UserGroupSite.Shared.Models.Constants;

namespace UserGroupSite.Client.Services;

public class ClientEventService(HttpClient Client) : IEventService
{
    public async Task<EventDto> GetEventByIdAsync(int eventId)
    {
        return await Client.GetFromJsonAsync<EventDto>($"{SharedConstants.EventApiUrl}/{eventId}");
    }
}