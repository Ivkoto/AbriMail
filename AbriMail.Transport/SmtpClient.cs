using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using AbriMail.Transport.Models;

namespace AbriMail.Transport;

/// <summary>
/// Minimal SMTP client for sending emails via implicit TLS (port 465).
/// </summary>
public class SmtpClient : IAsyncDisposable, IDisposable
{
    private TcpClient? _tcpClient;
    private SslStream? _sslStream;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private bool _disposed;

    /// <summary>
    /// Connects to the SMTP server using implicit TLS on port 465.
    /// </summary>
    /// <param name="host">SMTP server hostname</param>
    /// <param name="port">SMTP server port (default 465 for implicit TLS)</param>
    public async Task ConnectAsync(string host, int port = 465)
    {
        if (_tcpClient != null)
            throw new InvalidOperationException("Already connected to SMTP server");

        _tcpClient = new TcpClient();
        await _tcpClient.ConnectAsync(host, port);

        _sslStream = new SslStream(
            _tcpClient.GetStream(),
            leaveInnerStreamOpen: false,
            userCertificateValidationCallback: ValidateServerCertificate);
        await _sslStream.AuthenticateAsClientAsync(host);

        _reader = new StreamReader(_sslStream, Encoding.ASCII);
        _writer = new StreamWriter(_sslStream, Encoding.ASCII) { AutoFlush = true };

        var greeting = await _reader.ReadLineAsync();
        if (greeting == null || !greeting.StartsWith("220"))
            throw new InvalidOperationException($"Unexpected SMTP greeting: {greeting}");
    }

    /// <summary>
    /// Sends EHLO command and returns server-supported capabilities.
    /// </summary>
    public async Task<IList<string>> EhloAsync(string domain)
    {
        if (_writer is null || _reader is null)
            throw new InvalidOperationException("Not connected to SMTP server");

        await _writer.WriteAsync($"EHLO {domain}\r\n");

        var capabilities = new List<string>();
        string? line;

        while ((line = await _reader.ReadLineAsync()) != null)
        {
            if (line.Length < 4)
                throw new InvalidOperationException($"Unexpected SMTP response: {line}");

            var code = line.Substring(0, 3);
            var sep = line[3];
            if (code != "250")
                throw new InvalidOperationException($"EHLO failed: {line}");

            capabilities.Add(line.Substring(4));
            if (sep == ' ')
                break;
        }

        return capabilities;
    }

    /// <summary>
    /// Authenticates with the SMTP server using the AUTH LOGIN mechanism.
    /// </summary>
    /// <param name="username">Email username</param>
    /// <param name="password">Email password</param>
    public async Task AuthenticateAsync(string username, string password)
    {
        if (_writer is null || _reader is null)
            throw new InvalidOperationException("Not connected to SMTP server");

        await _writer.WriteAsync("AUTH LOGIN\r\n");
        var response = await _reader.ReadLineAsync();

        if (response == null || !response.StartsWith("334"))
            throw new InvalidOperationException($"SMTP AUTH LOGIN failed: {response}");

        // Base64-encoded username
        var userB64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(username));
        await _writer.WriteAsync(userB64 + "\r\n");
        response = await _reader.ReadLineAsync();

        if (response == null || !response.StartsWith("334"))
            throw new InvalidOperationException($"SMTP username rejected: {response}");

        // Base64-encoded password
        var passB64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(password));
        await _writer.WriteAsync(passB64 + "\r\n");
        response = await _reader.ReadLineAsync();

        if (response == null || !response.StartsWith("235"))
            throw new InvalidOperationException($"SMTP authentication failed: {response}");
    }

    /// <summary>
    /// Specifies the sender for the email.
    /// </summary>
    /// <param name="sender">Email address of the sender (e.g., user@example.com)</param>
    public async Task MailFromAsync(string sender)
    {
        if (_writer is null || _reader is null)
            throw new InvalidOperationException("Not connected to SMTP server");

        await _writer.WriteAsync($"MAIL FROM:<{sender}>\r\n");
        var response = await _reader.ReadLineAsync();

        if (response == null || !response.StartsWith("250"))
            throw new InvalidOperationException($"SMTP MAIL FROM failed: {response}");
    }

    /// <summary>
    /// Specifies a recipient for the email.
    /// </summary>
    /// <param name="recipient">Email address of the recipient (e.g., user@domain.com)</param>
    public async Task RcptToAsync(string recipient)
    {
        if (_writer is null || _reader is null)
            throw new InvalidOperationException("Not connected to SMTP server");

        await _writer.WriteAsync($"RCPT TO:<{recipient}>\r\n");
        var response = await _reader.ReadLineAsync();

        if (response == null || !response.StartsWith("250"))
            throw new InvalidOperationException($"SMTP RCPT TO failed: {response}");
    }

    /// <summary>
    /// Initiates the data section, sends the email content, and terminates with a single dot line.
    /// </summary>
    /// <param name="content">Full email content (headers+body) ending with CRLF.CRLF</param>
    public async Task DataAsync(string content)
    {
        if (_writer is null || _reader is null)
            throw new InvalidOperationException("Not connected to SMTP server");

        await _writer.WriteAsync("DATA\r\n");
        var response = await _reader.ReadLineAsync();

        if (response == null || !response.StartsWith("354"))
            throw new InvalidOperationException($"SMTP DATA initiation failed: {response}");

        // Write email content (must end with \r\n.\r\n)
        await _writer.WriteAsync(content);

        response = await _reader.ReadLineAsync();

        if (response == null || !response.StartsWith("250"))
            throw new InvalidOperationException($"SMTP DATA failed: {response}");
    }

    /// <summary>
    /// Closes the SMTP session.
    /// </summary>
    public async Task QuitAsync()
    {
        if (_writer is null || _reader is null)
            return;

        await _writer.WriteAsync("QUIT\r\n");
        var response = await _reader.ReadLineAsync();
        // Expect 221 or similar
        if (response == null || !response.StartsWith("221"))
            throw new InvalidOperationException($"SMTP QUIT failed: {response}");

        Dispose();
    }

    /// <summary>
    /// Synchronously disposes the client and underlying streams.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _reader?.Dispose();
        _writer?.Dispose();
        _sslStream?.Dispose();
        _tcpClient?.Dispose();
        _disposed = true;
    }

    /// <summary>
    /// Asynchronously disposes the client and underlying streams.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        Dispose();
        await Task.CompletedTask;
    }

    /// <summary>
    /// Sends an email by orchestrating connection, authentication, and data transfer.
    /// </summary>
    public async Task SendEmailAsync(
        string host,
        int port,
        string domain,
        string username,
        string password,
        MailMessageDetails message)
    {
        try
        {
            await ConnectAsync(host, port);
            await EhloAsync(domain);
            await AuthenticateAsync(username, password);
            await MailFromAsync(message.From);

            foreach (var recipient in message.To)
            {
                await RcptToAsync(recipient);
            }

            var sb = new StringBuilder();

            sb.Append($"From: {message.From}\r\n");
            sb.Append($"To: {string.Join(", ", message.To)}\r\n");
            sb.Append($"Subject: {message.Subject}\r\n");
            sb.Append($"Date: {DateTime.UtcNow:R}\r\n");

            foreach (var header in message.Headers)
            {
                sb.Append($"{header.Key}: {header.Value}\r\n");
            }

            sb.Append($"Content-Type: {message.ContentType}\r\n");
            sb.Append("\r\n");
            sb.Append(message.Body);

            if (!message.Body.EndsWith("\r\n"))
                sb.Append("\r\n");

            sb.Append(".\r\n");

            await DataAsync(sb.ToString());
            await QuitAsync();
        }
        finally
        {
            await DisposeAsync();
        }
    }

    /// <summary>
    /// Certificate validation callback for SMTP TLS.
    /// </summary>
    private static bool ValidateServerCertificate(
        object sender,
        System.Security.Cryptography.X509Certificates.X509Certificate? certificate,
        System.Security.Cryptography.X509Certificates.X509Chain? chain,
        SslPolicyErrors sslPolicyErrors)
    {
        return sslPolicyErrors == SslPolicyErrors.None;
    }
}
