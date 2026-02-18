using UserGroupSite.Shared.Events;

namespace UserGroupSite.Shared.Services;

/// <summary>Service for retrieving speaker options for event creation.</summary>
public interface ISpeakerService
{
    /// <summary>Gets the list of eligible speakers (Admins and Speakers).</summary>
    /// <returns>A read-only list of speaker options ordered by name.</returns>
    Task<IReadOnlyList<SpeakerOption>> GetSpeakersAsync();
}
