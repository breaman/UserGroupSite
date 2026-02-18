using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using UserGroupSite.Data.Models;

namespace UserGroupSite.Server.Components.Pages;

public partial class Home : ComponentBase
{
    [Inject] private ApplicationDbContext DbContext { get; set; } = default!;

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

            var items = await DbContext.Events
                .AsNoTracking()
                .OrderByDescending(eventEntity => eventEntity.EventDateTime)
                .Select(eventEntity => new EventSummary(
                    eventEntity.Name,
                    eventEntity.Description,
                    eventEntity.EventDateTime,
                    eventEntity.Location))
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
        string Name,
        string Description,
        DateTime EventDateTime,
        string Location);
}