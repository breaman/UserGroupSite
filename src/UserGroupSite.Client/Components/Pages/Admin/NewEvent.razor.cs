using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Components;
using UserGroupSite.Shared.Events;
using UserGroupSite.Shared.Services;

namespace UserGroupSite.Client.Components.Pages.Admin;

public partial class NewEvent : ComponentBase
{
    [Inject] private HttpClient HttpClient { get; set; } = default!;
    [Inject] private ISpeakerService SpeakerService { get; set; } = default!;

    private readonly InputModel Input = new();

    [PersistentState]
    public IReadOnlyList<SpeakerOption>? SpeakerOptions { get; set; }

    private bool isLoading = true;
    private bool isSubmitting;
    private string? loadError;
    private string? errorMessage;
    private string? successMessage;

    protected override async Task OnInitializedAsync()
    {
        // Only load speakers if not already persisted from server prerender
        if (SpeakerOptions is null || SpeakerOptions.Count == 0)
        {
            await LoadSpeakersAsync();
        }
        else
        {
            isLoading = false;
        }
    }

    private async Task LoadSpeakersAsync()
    {
        try
        {
            isLoading = true;
            loadError = null;

            SpeakerOptions = await SpeakerService.GetSpeakersAsync();
        }
        catch (Exception ex)
        {
            loadError = $"Failed to load speakers: {ex.Message}";
            SpeakerOptions = Array.Empty<SpeakerOption>();
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task HandleValidSubmit()
    {
        errorMessage = null;
        successMessage = null;

        EnsureSlug();

        if (!TryParseEventDateTimeUtc(Input.EventDateTimeLocal, out var eventDateTimeUtc))
        {
            errorMessage = "Event date/time must be valid.";
            return;
        }

        var request = new CreateEventRequest(
            Input.Name.Trim(),
            Input.Slug.Trim(),
            Input.Description.Trim(),
            eventDateTimeUtc,
            Input.Location.Trim(),
            Input.SpeakerIds.ToArray());

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

    private void ToggleSpeaker(int speakerId, ChangeEventArgs args)
    {
        if (args.Value is bool isSelected)
        {
            if (isSelected)
            {
                Input.SpeakerIds.Add(speakerId);
            }
            else
            {
                Input.SpeakerIds.Remove(speakerId);
            }
        }
    }

    private void HandleNameBlur()
    {
        EnsureSlug();
    }

    private void EnsureSlug()
    {
        if (!string.IsNullOrWhiteSpace(Input.Slug))
        {
            return;
        }

        Input.Slug = ToKebabCase(Input.Name);
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

    private static string ToKebabCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length);
        var previousWasSeparator = false;
        var previousWasLowerOrDigit = false;

        foreach (var c in value.Trim())
        {
            if (char.IsLetterOrDigit(c))
            {
                if (char.IsUpper(c) && previousWasLowerOrDigit && !previousWasSeparator)
                {
                    builder.Append('-');
                }

                builder.Append(char.ToLowerInvariant(c));
                previousWasSeparator = false;
                previousWasLowerOrDigit = char.IsLower(c) || char.IsDigit(c);
                continue;
            }

            if (!previousWasSeparator && builder.Length > 0)
            {
                builder.Append('-');
                previousWasSeparator = true;
                previousWasLowerOrDigit = false;
            }
        }

        return builder.ToString().Trim('-');
    }

    private sealed class InputModel
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = "";

        [Required]
        [MaxLength(200)]
        public string Slug { get; set; } = "";

        [Required]
        [MaxLength(2000)]
        public string Description { get; set; } = "";

        [Required]
        public string EventDateTimeLocal { get; set; } = "";

        [Required]
        [MaxLength(200)]
        public string Location { get; set; } = "";

        public HashSet<int> SpeakerIds { get; } = new();

        public void Reset()
        {
            Name = "";
            Slug = "";
            Description = "";
            EventDateTimeLocal = "";
            Location = "";
            SpeakerIds.Clear();
        }
    }
}
