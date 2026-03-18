using Mailjet.Client;
using Mailjet.Client.TransactionalEmails;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using UserGroupSite.Server.Models;

namespace UserGroupSite.Server.Components.Email;

public class MailjetEmailSender : IEmailSender
{
    private readonly IMailjetClient _mailjetClient;
    private readonly EmailConfig _emailConfig;
    private readonly ILogger _logger;

    public MailjetEmailSender(IMailjetClient mailjetClient, IOptions<AppSettings> appSettings, ILogger logger)
    {
        _mailjetClient = mailjetClient;
        _emailConfig = appSettings.Value.Email ?? new EmailConfig();
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        _logger.LogInformation("Sending email to {Email} with subject '{Subject}'", email, subject);
        // construct your email with builder
        var emailToSend = new TransactionalEmailBuilder()
            .WithFrom(new SendContact(_emailConfig.FromAddress ?? "your@email.com", _emailConfig.FromName ?? "Do Not Reply"))
            .WithSubject(subject)
            .WithHtmlPart(htmlMessage)
            .WithTo(new SendContact(email))
            .Build();

        // invoke API to send email
        var response = await _mailjetClient.SendTransactionalEmailAsync(emailToSend);
        if (response?.Messages?.Length > 0)
        {
            var message = response.Messages[0];
            if (message.Status == "success")
            {
                _logger.LogInformation("Email sent successfully to {Email}. Message ID: {MessageId}", email, message.To);
            }
            else
            {
                _logger.LogError("Failed to send email to {Email}. Status: {Status}, Errors: {Errors}", email, message.Status, string.Join(", ", message.Errors.Select(e => e.ErrorMessage)));
            }
        }
        else
        {
            _logger.LogError("No response from Mailjet API when sending email to {Email}", email);
        }
    }
}