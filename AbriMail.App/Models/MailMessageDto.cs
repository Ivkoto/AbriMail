namespace AbriMail.App.Models;

/// <summary>
/// Represents the full email message for UI consumption.
/// </summary>
public class MailMessageDto
{
    /// <summary>
    /// Sequence number of the message.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Email subject.
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Sender information.
    /// </summary>
    public string From { get; set; } = string.Empty;

    /// <summary>
    /// Recipient information.
    /// </summary>
    public string To { get; set; } = string.Empty;

    /// <summary>
    /// Sent date as string.
    /// </summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// Email body content.
    /// </summary>
    public string Body { get; set; } = string.Empty;
}
