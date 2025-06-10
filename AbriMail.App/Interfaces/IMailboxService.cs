using AbriMail.App.Models;

namespace AbriMail.App.Interfaces;

/// <summary>
/// Service facade exposing email operations to the UI.
/// </summary>
public interface IMailboxService
{
    /// <summary>
    /// Establishes IMAP and SMTP connections using provided settings.
    /// </summary>
    Task LoginAsync(ServerSettings settings);

    /// <summary>
    /// Fetches headers for all messages in the INBOX.
    /// </summary>
    Task<List<MailHeaderDto>> FetchInboxHeadersAsync();

    /// <summary>
    /// Fetches a full email message by sequence number.
    /// </summary>
    Task<MailMessageDto> FetchEmailAsync(int sequenceNumber);

    /// <summary>
    /// Sends an email via SMTP.
    /// </summary>
    Task SendEmailAsync(SendEmailDto message);

    /// <summary>
    /// Gets the current server settings in use.
    /// </summary>
    ServerSettings Settings { get; }

    /// <summary>
    /// Indicates whether the service is currently logged in.
    /// </summary>
    bool IsLoggedIn { get; }
}
