using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using UserGroupSite.Data.Models;

namespace UserGroupSite.Server.Components.Pages;

public partial class Home : ComponentBase
{
    [Inject] private IDbContextFactory<ApplicationDbContext> DbContextFactory { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    private readonly List<EventSummary> events = new();
    private bool isLoading = true;
    private string? loadError;

    /// <summary>Loads the most recent events in descending order.</summary>
    protected override async Task OnInitializedAsync()
    {
        await LoadEventsAsync();
    }

    /// <summary>Queries events from the database and updates UI state.</summary>
    private async Task LoadEventsAsync()
    {
        try
        {
            isLoading = true;
            loadError = null;
            events.Clear();

            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            var isAdmin = user.IsInRole("Admin");
            
            int? userId = null;
            if (user.Identity?.IsAuthenticated ?? false)
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out var id))
                {
                    userId = id;
                }
            }

            await using var dbContext = await DbContextFactory.CreateDbContextAsync();

            IQueryable<Event> query = dbContext.Events
                .AsNoTracking()
                .Include(e => e.Speakers);

            // Filter based on user role and publish status
            if (!isAdmin)
            {
                query = query.Where(e => 
                    e.IsPublished || 
                    (userId.HasValue && e.Speakers.Any(s => s.SpeakerId == userId.Value)));
            }

            var items = await query
                .OrderByDescending(eventEntity => eventEntity.EventDateTime)
                .Select(eventEntity => new EventSummary(
                    eventEntity.Id,
                    eventEntity.Slug,
                    eventEntity.Name,
                    eventEntity.ShortDescription,
                    eventEntity.EventDateTime,
                    eventEntity.Location,
                    eventEntity.IsPublished))
                .ToListAsync();

            events.AddRange(items);
        }
        catch (Exception ex)
        {
            loadError = $"Unable to load events: {ex.Message}";
        }
        finally
        {
            isLoading = false;
        }
    }

    private sealed record EventSummary(
        int Id,
        string Slug,
        string Name,
        string ShortDescription,
        DateTime EventDateTime,
        string Location,
        bool IsPublished);
}