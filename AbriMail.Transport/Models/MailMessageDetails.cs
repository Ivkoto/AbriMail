namespace AbriMail.Transport.Models;

/// <summary>
/// Details for constructing and sending an email message.
/// </summary>
public class MailMessageDetails
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

    /// <summary>
    /// Content type of the message (e.g., text/plain, text/html).
    /// </summary>
    public string ContentType { get; set; } = "text/plain";

    /// <summary>
    /// Additional headers to include in the message.
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
}
