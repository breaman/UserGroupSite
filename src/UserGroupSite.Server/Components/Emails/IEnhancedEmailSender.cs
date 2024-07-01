using Microsoft.AspNetCore.Identity;
using UserGroupSite.Data.Models;

namespace UserGroupSite.Server.Components.Emails;

public interface IEnhancedEmailSender<TUser> : IEmailSender<TUser> where TUser : class
{
    Task SendCongratulationsEmail(User user, string email);
}
