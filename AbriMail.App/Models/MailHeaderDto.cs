namespace AbriMail.App.Models;

/// <summary>
/// Represents a simplified email header for display in the UI.
/// </summary>
public class MailHeaderDto
{
    /// <summary>
    /// Sequence number or unique identifier of the message.
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
    /// Sent date as string.
    /// </summary>
    public string Date { get; set; } = string.Empty;
}
