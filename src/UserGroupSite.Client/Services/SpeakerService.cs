using System.Net.Http.Json;
using UserGroupSite.Shared.Events;
using UserGroupSite.Shared.Services;

namespace UserGroupSite.Client.Services;

/// <summary>Client-side implementation that retrieves speakers from the API.</summary>
public sealed class SpeakerService : ISpeakerService
{
    private readonly HttpClient _httpClient;

    public SpeakerService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SpeakerOption>> GetSpeakersAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<SpeakerOption[]>("/api/events/speakers");
        return response ?? Array.Empty<SpeakerOption>();
    }
}
