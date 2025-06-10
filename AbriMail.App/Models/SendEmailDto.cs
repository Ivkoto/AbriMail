namespace AbriMail.App.Models;

/// <summary>
/// DTO for sending emails.
/// </summary>
public class SendEmailDto
{
    /// <summary>
    /// Sender email address.
    /// </summary>
    public string From { get; set; } = string.Empty;

    /// <summary>
    /// Recipient email addresses.
    /// </summary>
    public List<string> To { get; set; } = new List<string>();

    /// <summary>
    /// Email subject.
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Email body content.
    /// </summary>
    public string Body { get; set; } = string.Empty;
}
