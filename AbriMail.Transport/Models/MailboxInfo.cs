namespace AbriMail.Transport
{
  /// <summary>
  /// Contains information about a selected IMAP mailbox.
  /// </summary>
  public class MailboxInfo
  {
    /// <summary>
    /// Total number of messages in the mailbox.
    /// </summary>
    public int MessageCount { get; set; }

    /// <summary>
    /// Number of recent (new) messages in the mailbox.
    /// </summary>
    public int RecentCount { get; set; }

    /// <summary>
    /// Name of the selected mailbox.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if the mailbox is read-only.
    /// </summary>
    public bool IsReadOnly { get; set; }
  }
}
