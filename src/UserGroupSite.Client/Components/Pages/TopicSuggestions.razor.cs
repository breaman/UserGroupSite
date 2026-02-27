using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using UserGroupSite.Shared.Services;
using UserGroupSite.Shared.Topics;

namespace UserGroupSite.Client.Components.Pages;

/// <summary>Code-behind for the Topic Suggestions page.</summary>
public partial class TopicSuggestions : ComponentBase
{
    [Inject] private ITopicSuggestionService TopicSuggestionService { get; set; } = default!;

    private List<TopicSuggestionDto> topics = [];
    private readonly NewTopicInput newTopic = new();
    private readonly HashSet<int> processingIds = [];

    private bool isLoading = true;
    private bool isCreating;
    private string? errorMessage;
    private string? successMessage;

    protected override async Task OnInitializedAsync()
    {
        await LoadTopicsAsync();
    }

    private async Task LoadTopicsAsync()
    {
        try
        {
            isLoading = true;
            errorMessage = null;

            var result = await TopicSuggestionService.GetTopicSuggestionsAsync();
            topics = result.ToList();
        }
        catch (Exception ex)
        {
            errorMessage = $"Failed to load topics: {ex.Message}";
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task HandleCreateTopic()
    {
        errorMessage = null;
        successMessage = null;
        isCreating = true;

        try
        {
            var request = new CreateTopicSuggestionRequest(
                newTopic.Title.Trim(),
                newTopic.Description.Trim());

            var result = await TopicSuggestionService.CreateTopicSuggestionAsync(request);
            if (!result.IsSuccess)
            {
                errorMessage = result.ErrorMessage ?? "Failed to create suggestion.";
                return;
            }

            successMessage = "Topic suggestion submitted!";
            newTopic.Reset();

            await LoadTopicsAsync();
        }
        catch (Exception ex)
        {
            errorMessage = $"Failed to create suggestion: {ex.Message}";
        }
        finally
        {
            isCreating = false;
        }
    }

    private async Task HandleLike(int topicId)
    {
        if (!processingIds.Add(topicId))
        {
            return;
        }

        try
        {
            errorMessage = null;
            var result = await TopicSuggestionService.LikeTopicSuggestionAsync(topicId);
            if (!result.IsSuccess)
            {
                errorMessage = result.ErrorMessage ?? "Failed to like topic.";
                return;
            }

            await LoadTopicsAsync();
        }
        catch (Exception ex)
        {
            errorMessage = $"Failed to like topic: {ex.Message}";
        }
        finally
        {
            processingIds.Remove(topicId);
        }
    }

    private async Task HandleVolunteer(int topicId)
    {
        if (!processingIds.Add(topicId))
        {
            return;
        }

        try
        {
            errorMessage = null;
            var result = await TopicSuggestionService.VolunteerForTopicAsync(topicId);
            if (!result.IsSuccess)
            {
                errorMessage = result.ErrorMessage ?? "Failed to volunteer.";
                return;
            }

            successMessage = "You have volunteered to speak on this topic!";
            await LoadTopicsAsync();
        }
        catch (Exception ex)
        {
            errorMessage = $"Failed to volunteer: {ex.Message}";
        }
        finally
        {
            processingIds.Remove(topicId);
        }
    }

    /// <summary>Form input model for creating a new topic suggestion.</summary>
    private sealed class NewTopicInput
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = "";

        [Required]
        [MaxLength(2000)]
        public string Description { get; set; } = "";

        /// <summary>Resets the form to empty state.</summary>
        public void Reset()
        {
            Title = "";
            Description = "";
        }
    }
}
