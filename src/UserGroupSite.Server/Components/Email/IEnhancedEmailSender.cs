using Microsoft.AspNetCore.Identity;
using UserGroupSite.Data.Models;

namespace UserGroupSite.Server.Components.Email;

public interface IEnhancedEmailSender<TUser> : IEmailSender<TUser> where TUser : class
{
    Task SendCongratulationsEmail(User user, string email);
}