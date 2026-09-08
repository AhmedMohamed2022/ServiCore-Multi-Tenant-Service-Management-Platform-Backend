using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using ServiCore.Application.Common.Interfaces;

namespace ServiCore.Infrastructure.Email;

public sealed class EmailSender : IEmailSender
{
    private readonly EmailOptions _options;

    public EmailSender(
        IOptions<EmailOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendAsync(
        string recipientEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(recipientEmail))
            throw new ArgumentException(
                "Recipient email is required.",
                nameof(recipientEmail));

        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException(
                "Email subject is required.",
                nameof(subject));

        if (string.IsNullOrWhiteSpace(htmlBody))
            throw new ArgumentException(
                "Email body is required.",
                nameof(htmlBody));

        var message = new MimeMessage();

        message.From.Add(
            new MailboxAddress(
                _options.FromName,
                _options.FromEmail));

        message.To.Add(
            MailboxAddress.Parse(recipientEmail));

        message.Subject = subject;

        message.Body = new BodyBuilder
        {
            HtmlBody = htmlBody
        }.ToMessageBody();

        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(
            _options.SmtpHost,
            _options.SmtpPort,
            SecureSocketOptions.StartTls,
            cancellationToken);

        await smtp.AuthenticateAsync(
            _options.SmtpUsername,
            _options.SmtpPassword,
            cancellationToken);

        await smtp.SendAsync(
            message,
            cancellationToken);

        await smtp.DisconnectAsync(
            true,
            cancellationToken);
    }
}