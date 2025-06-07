namespace AbriMail.Transport
{
    /// <summary>
    /// Interface for IMAP client operations.
    /// </summary>
    public interface IImapClient : IDisposable
    {
        /// <summary>
        /// Connects to the IMAP server using implicit TLS.
        /// </summary>
        /// <param name="host">IMAP server hostname</param>
        /// <param name="port">IMAP server port (default 993)</param>
        Task ConnectAsync(string host, int port = 993);

        /// <summary>
        /// Connects to the IMAP server using provided settings.
        /// </summary>
        /// <param name="settings">IMAP connection settings</param>
        Task ConnectAsync(ImapSettings settings);

        /// <summary>
        /// Connects and authenticates using the provided settings.
        /// </summary>
        /// <param name="settings">IMAP connection settings including credentials</param>
        Task ConnectAndLoginAsync(ImapSettings settings);

        /// <summary>
        /// Authenticates with the IMAP server.
        /// </summary>
        /// <param name="username">Email username</param>
        /// <param name="password">Email password</param>
        Task LoginAsync(string username, string password);

        /// <summary>
        /// Selects the specified mailbox.
        /// </summary>
        /// <param name="mailbox">Mailbox name to select</param>
        /// <returns>Information about the selected mailbox</returns>
        Task<MailboxInfo> SelectMailboxAsync(string mailbox = "INBOX");

        /// <summary>
        /// Fetches email headers for a range of messages.
        /// </summary>
        /// <param name="start">Starting message number (1-based)</param>
        /// <param name="count">Number of messages to fetch</param>
        Task<List<EmailHeader>> FetchHeadersAsync(int start, int count);

        /// <summary>
        /// Fetches the full message content for a specific message.
        /// </summary>
        /// <param name="messageId">Message sequence number (1-based)</param>
        /// <returns>Full email message content</returns>
        Task<EmailMessage> FetchMessageAsync(int messageId);

        /// <summary>
        /// Fetches the full raw headers for a range of messages (BODY[HEADER]).
        /// </summary>
        /// <param name="start">Starting message number (1-based)</param>
        /// <param name="count">Number of messages to fetch</param>
        Task<List<string>> FetchRawHeadersAsync(int start, int count);

        /// <summary>
        /// Fetches the full raw RFC822 message (headers+body) for the specified message.
        /// </summary>
        /// <param name="messageId">Message sequence number (1-based)</param>
        /// <returns>Raw message content including headers and body</returns>
        Task<string> FetchRawMessageAsync(int messageId);

        /// <summary>
        /// Logs out and disconnects from the IMAP server.
        /// </summary>
        Task LogoutAsync();
    }
}
