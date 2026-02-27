namespace UserGroupSite.Shared.Comments;

/// <summary>Represents a comment displayed on an event page.</summary>
/// <param name="Id">The unique comment identifier.</param>
/// <param name="AuthorName">The display name of the comment author.</param>
/// <param name="Content">The raw Markdown content of the comment.</param>
/// <param name="CreatedOn">The UTC timestamp when the comment was created.</param>
public sealed record CommentDto(int Id, string AuthorName, string Content, DateTime CreatedOn);

/// <summary>Represents a request to create a new comment on an event.</summary>
/// <param name="Content">The raw Markdown content of the comment.</param>
public sealed record CreateCommentRequest(string Content);

/// <summary>Represents the response after a comment is created.</summary>
/// <param name="Id">The ID of the newly created comment.</param>
public sealed record CreateCommentResponse(int Id);
