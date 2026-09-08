namespace ServiCore.Application.Common.Interfaces;

public interface IEmailSender
{
    Task SendAsync(
        string recipientEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default);
}