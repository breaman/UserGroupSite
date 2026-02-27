using Microsoft.AspNetCore.Http.HttpResults;
using UserGroupSite.Shared.Comments;
using UserGroupSite.Shared.Services;

namespace UserGroupSite.Server.Endpoints;

/// <summary>Maps event comment API endpoints.</summary>
public static class CommentEndpoints
{
    /// <summary>Registers comment endpoints under /api/events/{slug}/comments.</summary>
    public static WebApplication MapCommentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/events/{slug}/comments");

        group.MapGet("/", GetCommentsAsync).AllowAnonymous();
        group.MapPost("/", CreateCommentAsync).RequireAuthorization();

        return app;
    }

    private static async Task<Ok<IReadOnlyList<CommentDto>>> GetCommentsAsync(
        string slug,
        ICommentService service)
    {
        var comments = await service.GetCommentsAsync(slug);
        return TypedResults.Ok(comments);
    }

    private static async Task<Results<Created<CreateCommentResponse>, BadRequest<string>, NotFound>> CreateCommentAsync(
        string slug,
        CreateCommentRequest request,
        ICommentService service)
    {
        var result = await service.CreateCommentAsync(slug, request);
        if (!result.IsSuccess)
        {
            if (string.Equals(result.ErrorMessage, "Event not found.", StringComparison.OrdinalIgnoreCase))
            {
                return TypedResults.NotFound();
            }

            return TypedResults.BadRequest(result.ErrorMessage ?? "Failed to create comment.");
        }

        return TypedResults.Created(
            $"/api/events/{Uri.EscapeDataString(slug)}/comments/{result.Value!.Id}",
            result.Value);
    }
}
