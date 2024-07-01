using Microsoft.AspNetCore.Identity.UI.Services;
using UserGroupSite.Data.Models;
using UserGroupSite.Server.Components.Emails;

namespace UserGroupSite.Server.Components.Auth;

public sealed class IdentityNoOpEmailSender : IEnhancedEmailSender<User>
{
    private readonly IEmailSender emailSender = new NoOpEmailSender();

    public Task SendConfirmationLinkAsync(User user, string email, string confirmationLink)
    {
        return emailSender.SendEmailAsync(email, "Confirm your email",
            $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");
    }

    public Task SendPasswordResetLinkAsync(User user, string email, string resetLink)
    {
        return emailSender.SendEmailAsync(email, "Reset your password",
            $"Please reset your password by <a href='{resetLink}'>clicking here</a>.");
    }

    public Task SendPasswordResetCodeAsync(User user, string email, string resetCode)
    {
        return emailSender.SendEmailAsync(email, "Reset your password",
            $"Please reset your password using the following code: {resetCode}");
    }

    public Task SendCongratulationsEmail(User user, string email)
    {
        return emailSender.SendEmailAsync(email, "Your email address is confirmed",
            "Your email address has been confirmed, please return to the site and do all things.");
    }
}
