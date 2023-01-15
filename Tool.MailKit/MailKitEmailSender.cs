﻿using GestioneSagre.Tools.MailKit.Options;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace GestioneSagre.Tools.MailKit;

public class MailKitEmailSender : IEmailClient
{
    private readonly IOptionsMonitor<SmtpOptions> smtpOptionsMonitor;
    private readonly ILogger<MailKitEmailSender> logger;
    public MailKitEmailSender(IOptionsMonitor<SmtpOptions> smtpOptionsMonitor, ILogger<MailKitEmailSender> logger)
    {
        this.logger = logger;
        this.smtpOptionsMonitor = smtpOptionsMonitor;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        return SendEmailAsync(email, string.Empty, subject, htmlMessage);
    }

    public async Task<bool> SendEmailAsync(string recipientEmail, string replyToEmail, string subject, string htmlMessage, CancellationToken token = default)
    {
        try
        {
            var options = smtpOptionsMonitor.CurrentValue;

            using SmtpClient client = new();

            await client.ConnectAsync(options.Host, options.Port, options.Security, token);

            if (!string.IsNullOrEmpty(options.Username))
            {
                await client.AuthenticateAsync(options.Username, options.Password);
            }

            MimeMessage message = new();

            message.From.Add(MailboxAddress.Parse(options.Sender));
            message.To.Add(MailboxAddress.Parse(recipientEmail));

            if (replyToEmail is not (null or ""))
            {
                message.ReplyTo.Add(MailboxAddress.Parse(replyToEmail));
            }

            message.Subject = subject;
            message.Body = new TextPart("html")
            {
                Text = htmlMessage
            };

            await client.SendAsync(message, token);
            await client.DisconnectAsync(true, token);

            logger.LogInformation("Message successfully sent to the email address {recipient}", recipientEmail);

            return true;
        }
        catch (Exception exc)
        {
            logger.LogError(exc, "Couldn't send email to {email} with message {message}", recipientEmail, htmlMessage);
            return false;
        }
    }
}