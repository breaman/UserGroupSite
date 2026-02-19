namespace UserGroupSite.Shared.Users;

/// <summary>Represents a user for management purposes.</summary>
public sealed record UserForManagement(
    int Id,
    string Email,
    string? FirstName,
    string? LastName,
    bool IsAdmin,
    bool IsSpeaker);

/// <summary>Represents a request to update a user's roles.</summary>
public sealed record UpdateUserRolesRequest(
    int UserId,
    bool IsAdmin,
    bool IsSpeaker);

/// <summary>Represents a response for updating user roles.</summary>
public sealed record UpdateUserRolesResponse(
    int UserId,
    bool IsAdmin,
    bool IsSpeaker);
/// <summary>Represents the current user's basic information.</summary>
public sealed record CurrentUserResponse(
    int UserId);