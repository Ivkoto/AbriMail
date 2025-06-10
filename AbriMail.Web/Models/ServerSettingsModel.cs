using System.ComponentModel.DataAnnotations;

namespace AbriMail.Web.Models;

/// <summary>
/// Local model for server settings with validation attributes.
/// </summary>
public class ServerSettingsModel
{
    [Required(ErrorMessage = "IMAP Server is required")]
    public string ImapServer { get; set; } = string.Empty;

    [Range(1, 65535, ErrorMessage = "IMAP Port must be between 1 and 65535")]
    public int ImapPort { get; set; } = 993;

    [Required(ErrorMessage = "IMAP Username is required")]
    public string ImapUsername { get; set; } = string.Empty;

    [Required(ErrorMessage = "IMAP Password is required")]
    public string ImapPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "SMTP Server is required")]
    public string SmtpServer { get; set; } = string.Empty;

    [Range(1, 65535, ErrorMessage = "SMTP Port must be between 1 and 65535")]
    public int SmtpPort { get; set; } = 465;

    [Required(ErrorMessage = "SMTP Username is required")]
    public string SmtpUsername { get; set; } = string.Empty;

    [Required(ErrorMessage = "SMTP Password is required")]
    public string SmtpPassword { get; set; } = string.Empty;
}
