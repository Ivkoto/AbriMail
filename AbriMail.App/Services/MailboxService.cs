using System.Text;
using AbriMail.App.Interfaces;
using AbriMail.App.Models;
using AbriMail.Transport;
using AbriMail.Transport.Models;

namespace AbriMail.App.Services;

/// <summary>
/// Service facade implementing the business logic for email operations.
/// </summary>
public class MailboxService : IMailboxService, IAsyncDisposable, IDisposable
{
    private ServerSettings? _settings;
    private IImapClient? _imapClient;
    private bool _disposed = false;

    /// <inheritdoc />
    public ServerSettings Settings => _settings ?? throw new InvalidOperationException("Not logged in. Call LoginAsync first.");

    /// <inheritdoc />
    public bool IsLoggedIn => _settings != null;

    /// <inheritdoc />
    public async Task LoginAsync(ServerSettings settings)
    {
        if (settings == null) throw new ArgumentNullException(nameof(settings));

        // TODO @IvayloK - move to a custom validator
        if (string.IsNullOrWhiteSpace(settings.ImapServer))
            throw new ArgumentException("IMAP Server is required", nameof(settings));
        if (settings.ImapPort <= 0 || settings.ImapPort > 65535)
            throw new ArgumentException("IMAP Port must be between 1 and 65535", nameof(settings.ImapPort));
        if (string.IsNullOrWhiteSpace(settings.ImapUsername))
            throw new ArgumentException("IMAP Username is required", nameof(settings));
        if (string.IsNullOrWhiteSpace(settings.ImapPassword))
            throw new ArgumentException("IMAP Password is required", nameof(settings));
        if (string.IsNullOrWhiteSpace(settings.SmtpServer))
            throw new ArgumentException("SMTP Server is required", nameof(settings));
        if (string.IsNullOrWhiteSpace(settings.SmtpUsername))
            throw new ArgumentException("SMTP Username is required", nameof(settings));
        if (string.IsNullOrWhiteSpace(settings.SmtpPassword))
            throw new ArgumentException("SMTP Password is required", nameof(settings));

        _settings = settings;

        _imapClient = new ImapClient();

        var imapSettings = new ImapSettings
        {
            Host = settings.ImapServer,
            Port = settings.ImapPort,
            Username = settings.ImapUsername,
            Password = settings.ImapPassword
        };

        await _imapClient.ConnectAndLoginAsync(imapSettings);
    }

    /// <inheritdoc />
    public async Task<List<MailHeaderDto>> FetchInboxHeadersAsync()
    {
        if (_imapClient == null) throw new InvalidOperationException("Not logged in. Login using username and password in the Settings page first.");

        var mailboxInfo = await _imapClient.SelectMailboxAsync("INBOX");

        var headers = await _imapClient.FetchHeadersAsync(1, mailboxInfo.MessageCount);

        return [.. headers.Select(h => new MailHeaderDto
        {
            Id = h.SequenceNumber,
            Subject = h.Subject,
            From = h.From,
            Date = h.Date
        })];
    }

    /// <inheritdoc />
    public async Task<MailMessageDto> FetchEmailAsync(int sequenceNumber)
    {
        if (_imapClient == null)
            throw new InvalidOperationException("Not logged in. Call LoginAsync first.");

        var msg = await _imapClient.FetchMessageAsync(sequenceNumber);

        return new MailMessageDto
        {
            Id = msg.Id,
            Subject = msg.Subject,
            From = msg.From,
            To = msg.To,
            Date = msg.Headers.TryGetValue("date", out var date) ? date : string.Empty,
            Body = msg.Body
        };
    }

    /// <inheritdoc />
    public async Task SendEmailAsync(SendEmailDto message)
    {
        if (_settings == null) throw new InvalidOperationException("Not initialized. Call LoginAsync first.");
        if (message == null) throw new ArgumentNullException(nameof(message));
        if (message.To == null || !message.To.Any()) throw new ArgumentException("At least one recipient is required.", nameof(message));
        if (string.IsNullOrWhiteSpace(message.Body)) throw new ArgumentException("Email body cannot be empty.", nameof(message));

        // Use configured SMTP username as sender
        var sender = _settings.SmtpUsername;

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_settings.SmtpServer, _settings.SmtpPort);

        var domain = new UriBuilder(_settings.SmtpServer).Host;

        await smtp.EhloAsync(domain);
        await smtp.AuthenticateAsync(_settings.SmtpUsername, _settings.SmtpPassword);
        await smtp.MailFromAsync(sender);

        foreach (var rcpt in message.To)
        {
            await smtp.RcptToAsync(rcpt);
        }

        // Message content
        var builder = new StringBuilder();
        builder.AppendLine($"From: {sender}");
        builder.AppendLine($"To: {string.Join(", ", message.To)}");
        builder.AppendLine($"Subject: {message.Subject}");
        builder.AppendLine($"Date: {DateTime.UtcNow:R}");
        builder.AppendLine();
        builder.AppendLine(message.Body);
        builder.AppendLine(".");

        await smtp.DataAsync(builder.ToString());
        await smtp.QuitAsync();
    }

    /// <summary>
    /// Disposes any open IMAP connection asynchronously.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        if (_imapClient != null)
        {
            await _imapClient.LogoutAsync();
            if (_imapClient is IAsyncDisposable asyncDisp)
                await asyncDisp.DisposeAsync();
            else
                _imapClient.Dispose();
            _imapClient = null;
        }
        _disposed = true;
    }

    /// <summary>
    /// Disposes any open IMAP connection synchronously.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        if (_imapClient != null)
        {
            _imapClient.Dispose();
            _imapClient = null;
        }
        _disposed = true;
    }
}
