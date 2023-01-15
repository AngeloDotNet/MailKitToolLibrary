using Microsoft.AspNetCore.Identity.UI.Services;

namespace GestioneSagre.Tools.MailKit;

public interface IEmailClient : IEmailSender
{
    Task<bool> SendEmailAsync(string recipientEmail, string replyToEmail, string subject, string htmlMessage, CancellationToken token = default);
}