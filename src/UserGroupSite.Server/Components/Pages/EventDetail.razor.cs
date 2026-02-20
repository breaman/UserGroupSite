using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using UserGroupSite.Data.Models;

namespace UserGroupSite.Server.Components.Pages;

public partial class EventDetail : ComponentBase
{
    [Inject] private IDbContextFactory<ApplicationDbContext> DbContextFactory { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    [Parameter] public string Slug { get; set; } = default!;

    private EventDetailViewModel? eventDetail;
    private bool isLoading = true;
    private string? loadError;
    private bool canEdit;

    protected override async Task OnInitializedAsync()
    {
        await LoadEventAsync();
    }

    private async Task LoadEventAsync()
    {
        try
        {
            isLoading = true;
            loadError = null;

            await using var dbContext = await DbContextFactory.CreateDbContextAsync();

            var eventEntity = await dbContext.Events
                .AsNoTracking()
                .Include(e => e.Speakers)
                .ThenInclude(es => es.Speaker)
                .FirstOrDefaultAsync(e => e.Slug == Slug);

            if (eventEntity is null)
            {
                return;
            }

            var speakers = eventEntity.Speakers
                .Select(es => new SpeakerInfo(
                    es.SpeakerId,
                    BuildDisplayName(es.Speaker),
                    es.Speaker.Email ?? ""))
                .ToList();

            eventDetail = new EventDetailViewModel(
                eventEntity.Id,
                eventEntity.Slug,
                eventEntity.Name,
                eventEntity.Description,
                eventEntity.EventDateTime,
                eventEntity.Location,
                speakers);

            // Check if user can edit
            await CheckCanEditAsync(eventEntity);
        }
        catch (Exception ex)
        {
            loadError = $"Unable to load event: {ex.Message}";
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task CheckCanEditAsync(Event eventEntity)
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            canEdit = false;
            return;
        }

        // Check if user is Admin
        if (user.IsInRole("Admin"))
        {
            canEdit = true;
            return;
        }

        // Check if user is a speaker for this event
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            canEdit = eventEntity.Speakers.Any(es => es.SpeakerId == userId);
        }
    }

    private static string BuildDisplayName(User user)
    {
        if (!string.IsNullOrWhiteSpace(user.FirstName) || !string.IsNullOrWhiteSpace(user.LastName))
        {
            return $"{user.FirstName} {user.LastName}".Trim();
        }

        return user.Email ?? "Unknown";
    }

    private sealed record EventDetailViewModel(
        int Id,
        string Slug,
        string Name,
        string Description,
        DateTime EventDateTime,
        string Location,
        IReadOnlyList<SpeakerInfo> Speakers);

    private sealed record SpeakerInfo(
        int Id,
        string DisplayName,
        string Email);
}
