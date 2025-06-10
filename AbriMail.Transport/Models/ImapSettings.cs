namespace AbriMail.Transport.Models;

/// <summary>
/// Configuration settings for IMAP server connection.
/// </summary>
public class ImapSettings
{
    /// <summary>
    /// IMAP server hostname (e.g., "imap.gmail.com").
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// IMAP server port (default 993 for implicit TLS).
    /// </summary>
    public int Port { get; set; } = 993;

    /// <summary>
    /// Email username/address for authentication.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Email password for authentication.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Whether to use implicit TLS (default true).
    /// </summary>
    public bool UseTLS { get; set; } = true;

    /// <summary>
    /// Connection timeout in milliseconds (default 30 seconds).
    /// </summary>
    public int TimeoutMs { get; set; } = 30000;
}
