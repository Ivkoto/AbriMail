using AbriMail.Transport;
using AbriMail.Transport.Models;
using AbriMail.Web.Models;
using Markdig;

namespace AbriMail.Web.Services
{
    /// <summary>
    /// In-memory implementation of IMailService, storing mail configuration.
    /// </summary>
    public class MailService : IMailService
    {
        private MailConfig _config = new MailConfig();

        /// <inheritdoc />
        public MailConfig Config => _config;
        /// <inheritdoc />

        public Task UpdateConfigAsync(MailConfig config)
        {
            // Store configuration in memory for the current scope
            _config = config;
            return Task.CompletedTask;
        }

        public async Task<List<EmailHeader>> GetInboxAsync()
        {
            var settings = new ImapSettings
            {
                Host = Config.ImapServer,
                Port = Config.ImapPort,
                Username = Config.ImapUsername,
                Password = Config.ImapPassword
            };

            using var client = new ImapClient();
            await client.ConnectAndLoginAsync(settings);

            var info = await client.SelectMailboxAsync("INBOX");
            var headers = await client.FetchHeadersAsync(1, info.MessageCount);

            await client.LogoutAsync();

            return headers;
        }

        public async Task<EmailMessage> GetMessageAsync(int sequenceNumber)
        {
            var settings = new ImapSettings
            {
                Host = Config.ImapServer,
                Port = Config.ImapPort,
                Username = Config.ImapUsername,
                Password = Config.ImapPassword
            };

            using var client = new ImapClient();
            await client.ConnectAndLoginAsync(settings);

            var message = await client.FetchMessageAsync(sequenceNumber);

            await client.LogoutAsync();

            return message;
        }

        /// <inheritdoc />
        public async Task SendEmailAsync(ComposeModel model)
        {
            // Parse comma-separated recipients
            var recipients = model.To
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(r => r.Trim())
                .ToList();

            var details = new MailMessageDetails
            {
                From = Config.SmtpUsername,
                To = recipients,
                Subject = model.Subject,
                Body = model.Body,
                ContentType = "text/plain"
            };

            // Determine EHLO domain
            var domain = Config.SmtpUsername.Contains('@')
                ? Config.SmtpUsername.Split('@')[1]
                : Config.SmtpServer;

            await using var client = new SmtpClient();

            await client.SendEmailAsync(
                Config.SmtpServer,
                Config.SmtpPort,
                domain,
                Config.SmtpUsername,
                Config.SmtpPassword,
                details
            );
        }

        /// <summary>
        /// Converts a markdown string to HTML using Markdig.
        /// </summary>
        public string ConvertMarkdownToHtml(string markdown)
        {
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            return Markdown.ToHtml(markdown, pipeline);
        }
    }
}
