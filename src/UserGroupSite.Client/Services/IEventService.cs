namespace UserGroupSite.Client.Services;

public interface IEventService
{
    public Task<EventDto> GetEventByIdAsync(int eventId);
}