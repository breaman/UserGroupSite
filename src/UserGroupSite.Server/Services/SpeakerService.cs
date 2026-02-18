using Microsoft.AspNetCore.Identity;
using UserGroupSite.Data.Models;
using UserGroupSite.Shared.Events;
using UserGroupSite.Shared.Services;

namespace UserGroupSite.Server.Services;

/// <summary>Server-side implementation that retrieves speakers from the database.</summary>
public sealed class SpeakerService : ISpeakerService
{
    private readonly UserManager<User> _userManager;

    public SpeakerService(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SpeakerOption>> GetSpeakersAsync()
    {
        var speakers = await _userManager.GetUsersInRoleAsync("Speaker");
        var admins = await _userManager.GetUsersInRoleAsync("Admin");

        var users = speakers
            .Concat(admins)
            .DistinctBy(user => user.Id)
            .ToList();

        var options = users
            .OrderBy(user => user.LastName)
            .ThenBy(user => user.FirstName)
            .ThenBy(user => user.Email)
            .Select(user => new SpeakerOption(
                user.Id,
                BuildDisplayName(user),
                user.Email ?? string.Empty))
            .ToList();

        return options;
    }

    private static string BuildDisplayName(User user)
    {
        var first = user.FirstName?.Trim();
        var last = user.LastName?.Trim();

        if (!string.IsNullOrWhiteSpace(first) && !string.IsNullOrWhiteSpace(last))
        {
            return $"{first} {last}";
        }

        if (!string.IsNullOrWhiteSpace(first))
        {
            return first;
        }

        if (!string.IsNullOrWhiteSpace(last))
        {
            return last;
        }

        return user.Email ?? "Unknown";
    }
}
