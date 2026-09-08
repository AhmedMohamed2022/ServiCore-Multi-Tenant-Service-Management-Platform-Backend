namespace ServiCore.Infrastructure.Email;

public class EmailOptions
{
    public const string SectionName = "Email";

    public string SmtpHost { get; set; } = null!;
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; } = null!;
    public string SmtpPassword { get; set; } = null!;
    public string FromEmail { get; set; } = null!;
    public string FromName { get; set; } = "ServiCore";
}