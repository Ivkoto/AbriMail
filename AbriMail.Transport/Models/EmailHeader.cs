namespace AbriMail.Transport.Models;

/// <summary>
/// Represents email header information retrieved from IMAP ENVELOPE response.
/// </summary>
public class EmailHeader
{
    /// <summary>
    /// Message sequence number (1-based) in the mailbox.
    /// </summary>
    public int SequenceNumber { get; set; }

    /// <summary>
    /// Subject of the email.
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Sender's email address and name.
    /// </summary>
    public string From { get; set; } = string.Empty;

    /// <summary>
    /// Recipient's email address and name.
    /// </summary>
    public string To { get; set; } = string.Empty;

    /// <summary>
    /// Date when the email was sent.
    /// </summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// Size of the message in bytes.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Indicates if the message has been read.
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// Message flags (e.g., \Seen, \Flagged, etc.).
    /// </summary>
    public List<string> Flags { get; set; } = new List<string>();
}
