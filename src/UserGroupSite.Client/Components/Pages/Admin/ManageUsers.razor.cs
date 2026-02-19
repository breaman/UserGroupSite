using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using UserGroupSite.Shared.Users;

namespace UserGroupSite.Client.Components.Pages.Admin;

public partial class ManageUsers : ComponentBase
{
    [Inject] private HttpClient HttpClient { get; set; } = default!;

    private readonly List<UserForManagement> users = new();
    private readonly Dictionary<int, (bool IsAdmin, bool IsSpeaker)> pendingUpdates = new();
    private readonly Dictionary<int, UserForManagement> originalUsers = new();

    private Toast? toastComponent;
    private bool isLoading = true;
    private bool isSubmitting;
    private int savingUserId;
    private int currentUserId;
    private string? loadError;

    protected override async Task OnInitializedAsync()
    {
        if (!OperatingSystem.IsBrowser())
        {
            return;
        }

        await LoadCurrentUserAsync();
        await LoadUsersAsync();
    }

    private async Task LoadCurrentUserAsync()
    {
        try
        {
            var response = await HttpClient.GetFromJsonAsync<CurrentUserResponse>("/api/users/me");
            if (response != null)
            {
                currentUserId = response.UserId;
            }
        }
        catch (Exception ex)
        {
            loadError = $"Failed to load current user: {ex.Message}";
        }
    }

    private async Task LoadUsersAsync()
    {
        try
        {
            isLoading = true;
            loadError = null;
            users.Clear();
            originalUsers.Clear();

            var userList = await HttpClient.GetFromJsonAsync<IReadOnlyList<UserForManagement>>("/api/users");
            if (userList != null)
            {
                users.AddRange(userList);
                // Store original state for each user
                foreach (var user in users)
                {
                    originalUsers[user.Id] = user;
                }
            }
        }
        catch (Exception ex)
        {
            loadError = $"Failed to load users: {ex.Message}";
        }
        finally
        {
            isLoading = false;
        }
    }

    private bool GetIsAdmin(UserForManagement user)
    {
        return pendingUpdates.TryGetValue(user.Id, out var pending)
            ? pending.IsAdmin
            : user.IsAdmin;
    }

    private bool GetIsSpeaker(UserForManagement user)
    {
        return pendingUpdates.TryGetValue(user.Id, out var pending)
            ? pending.IsSpeaker
            : user.IsSpeaker;
    }

    private void HandleRoleChange(int userId, bool currentIsAdmin, bool currentIsSpeaker, string roleType, ChangeEventArgs args)
    {
        if (args.Value is not bool newValue)
        {
            return;
        }

        var isAdmin = roleType == "admin" ? newValue : currentIsAdmin;
        var isSpeaker = roleType == "speaker" ? newValue : currentIsSpeaker;

        // Check if trying to remove own admin role
        if (roleType == "admin" && currentIsAdmin && !newValue && userId == currentUserId)
        {
            toastComponent?.ShowError("You cannot remove your own admin role.");
            return;
        }

        pendingUpdates[userId] = (isAdmin, isSpeaker);
    }

    private void HandleAdminRoleChange(int userId, bool currentIsAdmin, bool currentIsSpeaker, ChangeEventArgs args)
    {
        if (args.Value is not bool newValue)
        {
            return;
        }

        // Check if trying to remove own admin role
        if (currentIsAdmin && !newValue && userId == currentUserId)
        {
            toastComponent?.ShowError("You cannot remove your own admin role.");
            return;
        }

        pendingUpdates[userId] = (newValue, currentIsSpeaker);
    }

    private void HandleSpeakerRoleChange(int userId, bool currentIsAdmin, bool currentIsSpeaker, ChangeEventArgs args)
    {
        if (args.Value is not bool newValue)
        {
            return;
        }

        pendingUpdates[userId] = (currentIsAdmin, newValue);
    }

    private async Task SaveUserRoles(int userId)
    {
        if (!pendingUpdates.TryGetValue(userId, out var updates))
        {
            return;
        }

        isSubmitting = true;
        savingUserId = userId;

        try
        {
            var request = new UpdateUserRolesRequest(userId, updates.IsAdmin, updates.IsSpeaker);
            var response = await HttpClient.PutAsJsonAsync($"/api/users/{userId}/roles", request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMsg = string.IsNullOrWhiteSpace(errorContent)
                    ? "Failed to update user roles."
                    : errorContent;
                toastComponent?.ShowError(errorMsg);
                return;
            }

            // Update the user in the local list
            var userIndex = users.FindIndex(u => u.Id == userId);
            if (userIndex >= 0)
            {
                users[userIndex] = users[userIndex] with
                {
                    IsAdmin = updates.IsAdmin,
                    IsSpeaker = updates.IsSpeaker
                };
                // Also update the original state
                originalUsers[userId] = users[userIndex];
            }

            pendingUpdates.Remove(userId);
            toastComponent?.ShowSuccess("User roles updated successfully.");
        }
        catch (HttpRequestException ex)
        {
            toastComponent?.ShowError($"Failed to update user roles: {ex.Message}");
        }
        finally
        {
            isSubmitting = false;
            savingUserId = 0;
        }
    }

    private void ResetUserRoles(int userId)
    {
        pendingUpdates.Remove(userId);
        
        // Restore the user to the original state
        if (originalUsers.TryGetValue(userId, out var originalUser))
        {
            var userIndex = users.FindIndex(u => u.Id == userId);
            if (userIndex >= 0)
            {
                users[userIndex] = originalUser;
            }
        }
        
        StateHasChanged();
    }
}
