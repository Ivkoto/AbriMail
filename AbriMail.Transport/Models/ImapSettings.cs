namespace AbriMail.Transport
{
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

    /// <summary>
    /// Validates that all required settings are provided.
    /// </summary>
    public void Validate()
    {
      if (string.IsNullOrWhiteSpace(Host))
        throw new ArgumentException("Host is required", nameof(Host));

      if (Port <= 0 || Port > 65535)
        throw new ArgumentException("Port must be between 1 and 65535", nameof(Port));

      if (string.IsNullOrWhiteSpace(Username))
        throw new ArgumentException("Username is required", nameof(Username));

      if (string.IsNullOrWhiteSpace(Password))
        throw new ArgumentException("Password is required", nameof(Password));
    }

    /// <summary>
    /// Creates settings for common email providers.
    /// </summary>
    public static class Presets
    {
      /// <summary>
      /// Standart settings for Gmail IMAP access.
      /// </summary>
      public static ImapSettings Gmail(string username, string password) => new()
      {
        Host = "imap.gmail.com",
        Port = 993,
        Username = username,
        Password = password,
        UseTLS = true
      };

      /// <summary>
      /// Standart settings for Outlook IMAP access.
      /// </summary>
      public static ImapSettings Outlook(string username, string password) => new()
      {
        Host = "outlook.office365.com",
        Port = 993,
        Username = username,
        Password = password,
        UseTLS = true
      };

      /// <summary>
      /// Standart settings for Yahoo IMAP access.
      /// </summary>
      public static ImapSettings Yahoo(string username, string password) => new()
      {
        Host = "imap.mail.yahoo.com",
        Port = 993,
        Username = username,
        Password = password,
        UseTLS = true
      };

      /// <summary>
      /// Standart settings for Fastmail IMAP access.
      /// </summary>
      public static ImapSettings Fastmail(string username, string password) => new()
      {
        Host = "imap.fastmail.com",
        Port = 993,
        Username = username,
        Password = password,
        UseTLS = true
      };
    }
  }
}
