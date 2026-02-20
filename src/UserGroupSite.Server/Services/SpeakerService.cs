using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserGroupSite.Data.Models;
using UserGroupSite.Shared.Events;
using UserGroupSite.Shared.Services;

namespace UserGroupSite.Server.Services;

/// <summary>Server-side implementation that retrieves speakers from the database.</summary>
public sealed class SpeakerService : ISpeakerService
{
    private const string AdminRoleNormalized = "ADMIN";
    private const string SpeakerRoleNormalized = "SPEAKER";

    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    /// <summary>Initializes a new instance of the <see cref="SpeakerService"/> class.</summary>
    /// <param name="dbContextFactory">Factory used to create isolated DbContext instances.</param>
    public SpeakerService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SpeakerOption>> GetSpeakersAsync()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var roleIds = await dbContext.Set<Role>()
            .AsNoTracking()
            .Where(role => role.NormalizedName == AdminRoleNormalized || role.NormalizedName == SpeakerRoleNormalized)
            .Select(role => role.Id)
            .ToListAsync();

        if (roleIds.Count == 0)
        {
            return Array.Empty<SpeakerOption>();
        }

        var userIds = await dbContext.Set<IdentityUserRole<int>>()
            .AsNoTracking()
            .Where(userRole => roleIds.Contains(userRole.RoleId))
            .Select(userRole => userRole.UserId)
            .Distinct()
            .ToListAsync();

        if (userIds.Count == 0)
        {
            return Array.Empty<SpeakerOption>();
        }

        var users = await dbContext.Set<User>()
            .AsNoTracking()
            .Where(user => userIds.Contains(user.Id))
            .ToListAsync();

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

    /// <summary>Builds a display name from a user's profile data.</summary>
    /// <param name="user">The user to format.</param>
    /// <returns>The display name for the user.</returns>
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
