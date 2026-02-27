using System.Globalization;
using Microsoft.AspNetCore.Components;
using UserGroupSite.Shared.Events;
using UserGroupSite.Shared.Services;

namespace UserGroupSite.Client.Components.Pages.Admin;

public partial class EditEvent : ComponentBase
{
    [Inject] private IEventService EventService { get; set; } = default!;
    [Inject] private ISpeakerService SpeakerService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    [Parameter] public string Slug { get; set; } = string.Empty;

    [PersistentState]
    public EventFormInput? Input { get; set; }

    [PersistentState]
    public IReadOnlyList<SpeakerOption>? SpeakerOptions { get; set; }

#pragma warning disable CS0414 // Field is assigned but its value is never used
    private bool isLoadingEvent = true;
    private bool isLoadingSpeakers = true;
    private bool isSubmitting;
    private string? loadEventError;
    private string? loadSpeakersError;
    private string? errorMessage;
    private string? successMessage;
#pragma warning restore CS0414

    protected override async Task OnInitializedAsync()
    {
        if (RendererInfo.IsInteractive)
        {
            // Load event and speakers in parallel
            var eventTask = LoadEventAsync();
            var speakersTask = LoadSpeakersAsync();

            await Task.WhenAll(eventTask, speakersTask);
        }
    }

    private async Task LoadEventAsync()
    {
        try
        {
            isLoadingEvent = true;
            loadEventError = null;

            var result = await EventService.GetEventAsync(Slug);
            if (!result.IsSuccess || result.Value is null)
            {
                loadEventError = result.ErrorMessage ?? "Failed to load event.";

                // Redirect to not authorized page if access is denied
                if (result.ErrorMessage is not null && result.ErrorMessage.Contains("Access denied", StringComparison.OrdinalIgnoreCase))
                {
                    NavigationManager.NavigateTo("/not-authorized");
                }

                return;
            }

            var eventData = result.Value;

            // Convert UTC to local for the datetime-local input
            var localDateTime = eventData.EventDateTimeUtc.ToLocalTime();
            var eventDateTimeLocal = localDateTime.ToString("yyyy-MM-ddTHH:mm");

            Input = new EventFormInput
            {
                Name = eventData.Name,
                Slug = eventData.Slug,
                ShortDescription = eventData.ShortDescription,
                Description = eventData.Description,
                EventDateTimeLocal = eventDateTimeLocal,
                Location = eventData.Location,
                IsPublished = eventData.IsPublished
            };

            // Set speaker IDs
            foreach (var speakerId in eventData.SpeakerIds)
            {
                Input.SpeakerIds.Add(speakerId);
            }
        }
        catch (Exception ex)
        {
            loadEventError = $"Failed to load event: {ex.Message}";
        }
        finally
        {
            isLoadingEvent = false;
        }
    }

    private async Task LoadSpeakersAsync()
    {
        try
        {
            // Only load speakers if not already persisted from server prerender
            if (SpeakerOptions is null || SpeakerOptions.Count == 0)
            {
                isLoadingSpeakers = true;
                loadSpeakersError = null;

                SpeakerOptions = await SpeakerService.GetSpeakersAsync();
            }
        }
        catch (Exception ex)
        {
            loadSpeakersError = $"Failed to load speakers: {ex.Message}";
            SpeakerOptions = Array.Empty<SpeakerOption>();
        }
        finally
        {
            isLoadingSpeakers = false;
        }
    }

    private async Task HandleValidSubmit()
    {
        if (Input is null)
        {
            return;
        }

        errorMessage = null;
        successMessage = null;

        if (!TryParseEventDateTimeUtc(Input.EventDateTimeLocal, out var eventDateTimeUtc))
        {
            errorMessage = "Event date/time must be valid.";
            return;
        }

        var request = new UpdateEventRequest(
            Input.Name.Trim(),
            Input.ShortDescription?.Trim() ?? "",
            Input.Description.Trim(),
            eventDateTimeUtc,
            Input.Location.Trim(),
            Input.SpeakerIds.ToArray(),
            Input.IsPublished);

        isSubmitting = true;

        try
        {
            var result = await EventService.UpdateEventAsync(Slug, request);
            if (!result.IsSuccess)
            {
                errorMessage = result.ErrorMessage ?? "Failed to update event.";

                // Redirect to home if access is denied
                if (result.ErrorMessage is not null && result.ErrorMessage.Contains("Access denied", StringComparison.OrdinalIgnoreCase))
                {
                    NavigationManager.NavigateTo("/");
                }

                return;
            }

            successMessage = "Event updated successfully.";
            
            // Redirect to event detail page after a short delay
            await Task.Delay(1500);
            NavigationManager.NavigateTo($"/events/{Slug}");
        }
        catch (HttpRequestException ex)
        {
            errorMessage = $"Failed to update event: {ex.Message}";
        }
        finally
        {
            isSubmitting = false;
        }
    }

    private void HandleToggleSpeaker(int speakerId)
    {
        if (Input is null)
        {
            return;
        }

        if (Input.SpeakerIds.Contains(speakerId))
        {
            Input.SpeakerIds.Remove(speakerId);
        }
        else
        {
            Input.SpeakerIds.Add(speakerId);
        }
    }

    private void SkipNameBlur()
    {
        // No-op for edit mode - slug is read-only
    }

    private static bool TryParseEventDateTimeUtc(string? value, out DateTime utcDateTime)
    {
        utcDateTime = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var formats = new[] { "yyyy-MM-ddTHH:mm", "yyyy-MM-ddTHH:mm:ss" };
        if (!DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var localDateTime))
        {
            return false;
        }

        utcDateTime = DateTime.SpecifyKind(localDateTime, DateTimeKind.Local).ToUniversalTime();
        return true;
    }
}
