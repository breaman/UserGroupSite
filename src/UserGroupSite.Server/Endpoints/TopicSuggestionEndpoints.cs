using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using UserGroupSite.Shared.Services;
using UserGroupSite.Shared.Topics;

namespace UserGroupSite.Server.Endpoints;

/// <summary>Maps topic suggestion API endpoints.</summary>
public static class TopicSuggestionEndpoints
{
    /// <summary>Registers topic suggestion endpoints under /api/topics.</summary>
    public static WebApplication MapTopicSuggestionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/topics");

        group.MapGet("/", GetTopicSuggestionsAsync).AllowAnonymous();
        group.MapPost("/", CreateTopicSuggestionAsync).RequireAuthorization();
        group.MapPost("/{id:int}/like", LikeTopicSuggestionAsync).RequireAuthorization();
        group.MapPost("/{id:int}/volunteer", VolunteerForTopicAsync).RequireAuthorization();

        return app;
    }

    private static async Task<Ok<IReadOnlyList<TopicSuggestionDto>>> GetTopicSuggestionsAsync(
        ITopicSuggestionService service)
    {
        var topics = await service.GetTopicSuggestionsAsync();
        return TypedResults.Ok(topics);
    }

    private static async Task<Results<Created<CreateTopicSuggestionResponse>, BadRequest<string>>> CreateTopicSuggestionAsync(
        CreateTopicSuggestionRequest request,
        ITopicSuggestionService service)
    {
        var result = await service.CreateTopicSuggestionAsync(request);
        if (!result.IsSuccess)
        {
            return TypedResults.BadRequest(result.ErrorMessage ?? "Failed to create topic suggestion.");
        }

        return TypedResults.Created($"/api/topics/{result.Value!.Id}", result.Value);
    }

    private static async Task<Results<Ok, BadRequest<string>, NotFound>> LikeTopicSuggestionAsync(
        int id,
        ITopicSuggestionService service)
    {
        var result = await service.LikeTopicSuggestionAsync(id);
        if (!result.IsSuccess)
        {
            if (string.Equals(result.ErrorMessage, "Topic suggestion not found.", StringComparison.OrdinalIgnoreCase))
            {
                return TypedResults.NotFound();
            }

            return TypedResults.BadRequest(result.ErrorMessage ?? "Failed to like topic suggestion.");
        }

        return TypedResults.Ok();
    }

    private static async Task<Results<Ok, BadRequest<string>, NotFound>> VolunteerForTopicAsync(
        int id,
        ITopicSuggestionService service)
    {
        var result = await service.VolunteerForTopicAsync(id);
        if (!result.IsSuccess)
        {
            if (string.Equals(result.ErrorMessage, "Topic suggestion not found.", StringComparison.OrdinalIgnoreCase))
            {
                return TypedResults.NotFound();
            }

            return TypedResults.BadRequest(result.ErrorMessage ?? "Failed to volunteer for topic.");
        }

        return TypedResults.Ok();
    }
}
