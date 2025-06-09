using AbriMail.Transport.Models;
using AbriMail.Web.Models;

namespace AbriMail.Web.Services
{
    /// <summary>
    /// Provides methods to manage email configuration.
    /// </summary>
    public interface IMailService
    {
        /// <summary>
        /// Gets the current mail configuration.
        /// </summary>
        MailConfig Config { get; }

        /// <summary>
        /// Updates the mail configuration.
        /// </summary>
        Task UpdateConfigAsync(MailConfig config);

        /// <summary>
        /// Fetches list of message headers from INBOX.
        /// </summary>
        Task<List<EmailHeader>> GetInboxAsync();

        /// <summary>
        /// Fetches full message by sequence number.
        /// </summary>
        Task<EmailMessage> GetMessageAsync(int sequenceNumber);

        /// <summary>
        /// Sends a new email message.
        /// </summary>
        Task SendEmailAsync(ComposeModel model);
    }
}
