using Aufy.Core.EmailSender;

namespace Aufy.FluentEmail;

public class FluentEmailOptions
{
    public const string SectionName = "FluentEmail";
    
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; }
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    
    public Dictionary<EmailType, FluentEmailTemplateSettings> Emails { get; set; } = new();
}

public class FluentEmailTemplateSettings
{
    public string? TemplatePath { get; set; }
    public string? Subject { get; set; }
}