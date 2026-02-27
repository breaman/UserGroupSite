using System.Globalization;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using UserGroupSite.Shared.Events;
using UserGroupSite.Shared.Services;

namespace UserGroupSite.Client.Components.Pages.Admin;

public partial class NewEvent : ComponentBase
{
    [Inject] private HttpClient HttpClient { get; set; } = default!;
    [Inject] private ISpeakerService SpeakerService { get; set; } = default!;

    private readonly EventFormInput Input = new();

    [PersistentState]
    public IReadOnlyList<SpeakerOption>? SpeakerOptions { get; set; }

#pragma warning disable CS0414 // Field is assigned but its value is never used
    private bool isSpeakersLoading = true;
    private bool isSubmitting;
    private string? speakersError;
    private string? errorMessage;
    private string? successMessage;
#pragma warning restore CS0414 

    protected override async Task OnInitializedAsync()
    {
        // Only load speakers if not already persisted from server prerender
        if (SpeakerOptions is null || SpeakerOptions.Count == 0)
        {
            await LoadSpeakersAsync();
        }
        else
        {
            isSpeakersLoading = false;
        }
    }

    private async Task LoadSpeakersAsync()
    {
        try
        {
            isSpeakersLoading = true;
            speakersError = null;

            SpeakerOptions = await SpeakerService.GetSpeakersAsync();
        }
        catch (Exception ex)
        {
            speakersError = $"Failed to load speakers: {ex.Message}";
            SpeakerOptions = Array.Empty<SpeakerOption>();
        }
        finally
        {
            isSpeakersLoading = false;
        }
    }

    private async Task HandleValidSubmit()
    {
        errorMessage = null;
        successMessage = null;

        HandleNameBlur();

        if (!TryParseEventDateTimeUtc(Input.EventDateTimeLocal, out var eventDateTimeUtc))
        {
            errorMessage = "Event date/time must be valid.";
            return;
        }

        var request = new CreateEventRequest(
            Input.Name.Trim(),
            Input.Slug.Trim(),
            Input.ShortDescription?.Trim() ?? "",
            Input.Description.Trim(),
            eventDateTimeUtc,
            Input.Location.Trim(),
            Input.SpeakerIds.ToArray(),
            Input.IsPublished);

        isSubmitting = true;

        try
        {
            var response = await HttpClient.PostAsJsonAsync("/api/events", request);
            if (!response.IsSuccessStatusCode)
            {
                var serverError = await response.Content.ReadAsStringAsync();
                errorMessage = string.IsNullOrWhiteSpace(serverError)
                    ? "Failed to create event."
                    : serverError;
                return;
            }

            successMessage = "Event created.";
            Input.Reset();
        }
        catch (HttpRequestException ex)
        {
            errorMessage = $"Failed to create event: {ex.Message}";
        }
        finally
        {
            isSubmitting = false;
        }
    }

    private void HandleToggleSpeaker(int speakerId)
    {
        if (Input.SpeakerIds.Contains(speakerId))
        {
            Input.SpeakerIds.Remove(speakerId);
        }
        else
        {
            Input.SpeakerIds.Add(speakerId);
        }
    }

    private void HandleNameBlur()
    {
        if (!string.IsNullOrWhiteSpace(Input.Slug))
        {
            return;
        }

        Input.Slug = Input.ToKebabCase(Input.Name);
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
