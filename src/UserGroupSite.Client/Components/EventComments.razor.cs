using Microsoft.AspNetCore.Components;
using UserGroupSite.Shared.Comments;
using UserGroupSite.Shared.Services;

namespace UserGroupSite.Client.Components;

/// <summary>Code-behind for the EventComments component that displays and creates comments on events.</summary>
public partial class EventComments : ComponentBase
{
    [Inject] private ICommentService CommentService { get; set; } = default!;
    [Inject] private IMarkdownService MarkdownService { get; set; } = default!;

    /// <summary>The slug of the event to display comments for.</summary>
    [Parameter, EditorRequired] public string EventSlug { get; set; } = default!;

    private List<CommentDto> comments = [];
    private readonly HashSet<int> expandedRawIds = [];
    private string newCommentContent = "";
    private bool showPreview;

    private bool isLoading = true;
    private bool isSubmitting;
    private string? errorMessage;
    private string? successMessage;

    protected override async Task OnInitializedAsync()
    {
        await LoadCommentsAsync();
    }

    /// <summary>Loads all comments for the current event from the service.</summary>
    private async Task LoadCommentsAsync()
    {
        try
        {
            isLoading = true;
            errorMessage = null;

            var result = await CommentService.GetCommentsAsync(EventSlug);
            comments = result.ToList();
        }
        catch (Exception ex)
        {
            errorMessage = $"Failed to load comments: {ex.Message}";
        }
        finally
        {
            isLoading = false;
        }
    }

    /// <summary>Handles the form submission to create a new comment.</summary>
    private async Task HandleSubmitComment()
    {
        if (string.IsNullOrWhiteSpace(newCommentContent))
        {
            return;
        }

        errorMessage = null;
        successMessage = null;
        isSubmitting = true;

        try
        {
            var request = new CreateCommentRequest(newCommentContent.Trim());
            var result = await CommentService.CreateCommentAsync(EventSlug, request);

            if (!result.IsSuccess)
            {
                errorMessage = result.ErrorMessage ?? "Failed to post comment.";
                return;
            }

            successMessage = "Comment posted!";
            newCommentContent = "";
            showPreview = false;

            await LoadCommentsAsync();
        }
        catch (Exception ex)
        {
            errorMessage = $"Failed to post comment: {ex.Message}";
        }
        finally
        {
            isSubmitting = false;
        }
    }

    /// <summary>Formats a UTC timestamp as a human-readable relative time string.</summary>
    /// <param name="utcDateTime">The UTC date/time to format.</param>
    /// <returns>A relative time description such as "2 hours ago".</returns>
    private static string FormatRelativeTime(DateTime utcDateTime)
    {
        var elapsed = DateTime.UtcNow - utcDateTime;

        return elapsed.TotalSeconds switch
        {
            < 60 => "just now",
            < 3600 => $"{(int)elapsed.TotalMinutes} min ago",
            < 86400 => $"{(int)elapsed.TotalHours}h ago",
            < 604800 => $"{(int)elapsed.TotalDays}d ago",
            _ => utcDateTime.ToLocalTime().ToString("MMM d, yyyy")
        };
    }
}
