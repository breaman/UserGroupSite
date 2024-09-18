using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using UserGroupSite.Data.Models;
using UserGroupSite.Server.Components.Email;

namespace UserGroupSite.Server.Components.Auth.Pages;

public partial class ConfirmEmail : ComponentBase
{
    private string? statusMessage;

    [CascadingParameter] private HttpContext HttpContext { get; set; } = default!;

    [SupplyParameterFromQuery] private string? Email { get; set; }

    [SupplyParameterFromQuery] private string? Code { get; set; }

    [Inject] private IdentityRedirectManager RedirectManager { get; set; }
    [Inject] private UserManager<User> UserManager { get; set; }
    [Inject] private ILogger<ConfirmEmail> Logger { get; set; }
    [Inject] private NavigationManager NavigationManager { get; set; }
    [Inject] private IEnhancedEmailSender<User> EmailSender { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (Email is null || Code is null) RedirectManager.RedirectTo("");

        var user = await UserManager.FindByEmailAsync(Email);
        if (user is null)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            statusMessage = $"Error loading user with email {Email}";
            Logger.LogError("Error loading user with {Email}", Email);
        }
        else
        {
            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Code));
            var result = await UserManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                Logger.LogInformation("Successfully confirmed {Email}", Email);
                var teamsLink = NavigationManager.ToAbsoluteUri("Teams").AbsoluteUri;
                if (EmailSender is IdentityNoOpEmailSender)
                {
                    // Once you add a real email sender, you should remove this code that lets you confirm the account
                    statusMessage =
                        "Your account has been confirmed, please proceed with whatever you were trying to do now.";
                }
                else
                {
                    await EmailSender.SendCongratulationsEmail(user, Email);
                    RedirectManager.RedirectTo("/");
                }
            }
            else
            {
                Logger.LogError("Unable to activate {Email} with response of {Error}", Email, result.Errors);
                statusMessage =
                    "Your account cannot be activated since the user cannot be found. Please try to create your account again.";
            }
        }
    }
}