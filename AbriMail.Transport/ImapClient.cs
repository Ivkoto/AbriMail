using System.Net.Security;
using System.Net.Sockets;
using System.Text;

namespace AbriMail.Transport
{
    /// <summary>
    /// Minimal IMAP4rev1 client for basic email retrieval.
    /// Supports connecting via TLS, authentication, and fetching messages from INBOX.
    /// </summary>
    public class ImapClient : IImapClient, IAsyncDisposable
    {
        private TcpClient? _tcpClient;
        private SslStream? _sslStream;
        private StreamReader? _reader;
        private StreamWriter? _writer;
        private int _commandTagCounter = 1;
        private bool _isConnected = false;
        private bool _isAuthenticated = false;
        private bool _disposed = false;

        /// <summary>
        /// Connects to the IMAP server using implicit TLS on port 993.
        /// </summary>
        /// <param name="host">IMAP server hostname</param>
        /// <param name="port">IMAP server port (default 993 for implicit TLS)</param>
        /// <inheritdoc />
        public async Task ConnectAsync(string host, int port = 993)
        {
            if (_isConnected)
                throw new InvalidOperationException("Already connected to IMAP server");

            try
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(host, port);

                _sslStream = new SslStream(_tcpClient.GetStream());
                await _sslStream.AuthenticateAsClientAsync(host);

                _reader = new StreamReader(_sslStream, Encoding.ASCII);
                _writer = new StreamWriter(_sslStream, Encoding.ASCII) { AutoFlush = true };

                // Read server greeting and ensure it's untagged OK
                var greetingLine = await _reader!.ReadLineAsync();
                if (greetingLine is null || !greetingLine.StartsWith("* OK"))
                {
                    throw new InvalidOperationException($"Unexpected IMAP greeting: {greetingLine}");
                }

                _isConnected = true;
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        /// <summary>
        /// Authenticates with the IMAP server using plain username/password.
        /// </summary>
        /// <param name="username">Email username</param>
        /// <param name="password">Email password</param>
        /// <inheritdoc />
        public async Task LoginAsync(string username, string password)
        {
            if (!_isConnected)
                throw new InvalidOperationException("Not connected to IMAP server");

            if (_isAuthenticated)
                throw new InvalidOperationException("Already authenticated");

            var tag = GetNextTag();
            var command = $"{tag} LOGIN \"{username}\" \"{password}\"\r\n";

            await _writer!.WriteAsync(command);
            var response = await ReadResponseAsync();

            // Parse tagged response - confirm success
            var lines = response.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);
            var taggedLine = lines.FirstOrDefault(l => l.StartsWith(tag));
            if (taggedLine == null || !taggedLine.StartsWith($"{tag} OK"))
            {
                throw new ImapException($"IMAP login failed: {taggedLine ?? response}")
                {
                    FailedCommand = command.Trim(),
                    ServerResponse = taggedLine ?? response
                };
            }

            _isAuthenticated = true;
        }

        /// <summary>
        /// Selects the specified mailbox (typically "INBOX").
        /// </summary>
        /// <param name="mailbox">Mailbox name to select</param>
        /// <returns>Information about the selected mailbox</returns>
        /// <inheritdoc />
        public async Task<MailboxInfo> SelectMailboxAsync(string mailbox = "INBOX")
        {
            if (!_isAuthenticated)
                throw new InvalidOperationException("Not authenticated with IMAP server");

            var tag = GetNextTag();
            var command = $"{tag} SELECT \"{mailbox}\"\r\n";

            await _writer!.WriteAsync(command);
            var response = await ReadResponseAsync();

            var lines = response.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);

            var taggedLine = lines.FirstOrDefault(l => l.StartsWith(tag));
            if (taggedLine == null || !taggedLine.StartsWith($"{tag} OK"))
            {
                throw new ImapException($"Failed to select mailbox {mailbox}: {taggedLine ?? response}")
                {
                    FailedCommand = command.Trim(),
                    ServerResponse = taggedLine ?? response
                };
            }

            // Parse mailbox info
            var info = new MailboxInfo { Name = mailbox };
            foreach (var line in lines)
            {
                if (line.StartsWith("* ") && line.Contains(" EXISTS"))
                {
                    var parts = line.Split(' ');
                    if (parts.Length >= 2 && int.TryParse(parts[1], out var count))
                        info.MessageCount = count;
                }
                else if (line.StartsWith("* ") && line.Contains(" RECENT"))
                {
                    var parts = line.Split(' ');
                    if (parts.Length >= 2 && int.TryParse(parts[1], out var recent))
                        info.RecentCount = recent;
                }
            }

            info.IsReadOnly = taggedLine.Contains("[READ-ONLY]");

            return info;
        }

        /// <summary>
        /// Fetches email headers for a range of messages.
        /// </summary>
        /// <param name="start">Starting message number (1-based)</param>
        /// <param name="count">Number of messages to fetch</param>
        /// <returns>List of email headers</returns>
        public async Task<List<EmailHeader>> FetchHeadersAsync(int start, int count)
        {
            if (!_isAuthenticated)
                throw new InvalidOperationException("Not authenticated with IMAP server");

            var end = start + count - 1;
            var tag = GetNextTag();
            var command = $"{tag} FETCH {start}:{end} (ENVELOPE FLAGS RFC822.SIZE)\r\n";

            await _writer!.WriteAsync(command);
            var response = await ReadResponseAsync();

            var linesList = response.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var taggedResponse = linesList.FirstOrDefault(l => l.StartsWith(tag));
            if (taggedResponse == null || !taggedResponse.StartsWith($"{tag} OK"))
            {
                throw new ImapException($"Failed to fetch headers: {taggedResponse ?? response}")
                {
                    FailedCommand = command.Trim(),
                    ServerResponse = taggedResponse ?? response
                };
            }

            return ParseEmailHeaders(response);
        }

        /// <summary>
        /// Fetches the full message content for a specific message.
        /// </summary>
        /// <param name="messageId">Message sequence number (1-based)</param>
        /// <returns>Full email message content</returns>
        public async Task<EmailMessage> FetchMessageAsync(int messageId)
        {
            if (!_isAuthenticated)
                throw new InvalidOperationException("Not authenticated with IMAP server");

            var tag = GetNextTag();
            var command = $"{tag} FETCH {messageId} (BODY[])\r\n";

            await _writer!.WriteAsync(command);
            var responseMsg = await ReadResponseAsync();

            var msgLines = responseMsg.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);
            var taggedMsgResp = msgLines.FirstOrDefault(l => l.StartsWith(tag));
            if (taggedMsgResp == null || !taggedMsgResp.StartsWith($"{tag} OK"))
            {
                throw new ImapException($"Failed to fetch message {messageId}: {taggedMsgResp ?? responseMsg}")
                {
                    FailedCommand = command.Trim(),
                    ServerResponse = taggedMsgResp ?? responseMsg
                };
            }

            var message = ParseEmailMessage(responseMsg, messageId);
            message.ParseRawContent();

            return message;
        }

        /// <summary>
        /// Fetches the full raw headers for a range of messages.
        /// </summary>
        /// <param name="start">Starting message number (1-based)</param>
        /// <param name="count">Number of messages to fetch</param>
        /// <returns>List of raw header text blocks</returns>
        public async Task<List<string>> FetchRawHeadersAsync(int start, int count)
        {
            if (!_isAuthenticated)
                throw new InvalidOperationException("Not authenticated with IMAP server");

            var end = start + count - 1;
            var tag = GetNextTag();
            var command = $"{tag} FETCH {start}:{end} (BODY[HEADER])\r\n";
            await _writer!.WriteAsync(command);
            var response = await ReadResponseAsync();

            var lines = response.Split(["\r\n", "\n"], StringSplitOptions.None);
            var headers = new List<string>();
            var builder = new StringBuilder();
            bool inBlock = false;

            foreach (var line in lines)
            {
                // Detect start of header literal
                if (line.Contains("BODY[HEADER]") && line.Contains("{"))
                {
                    inBlock = true;
                    builder.Clear();
                    continue;
                }
                // Detect end of a block when next untagged or tagged response appears
                if (inBlock && (line.StartsWith("*") || line.StartsWith(tag)))
                {
                    headers.Add(builder.ToString());
                    inBlock = false;

                    if (line.StartsWith(tag)) break;
                    continue;
                }
                if (inBlock)
                {
                    builder.AppendLine(line);
                }
            }

            return headers;
        }

        /// <summary>
        /// Fetches the full raw RFC822 message (headers+body) for the specified message.
        /// </summary>
        /// <param name="messageId">Message sequence number (1-based)</param>
        /// <returns>Full raw email message content</returns>
        public async Task<string> FetchRawMessageAsync(int messageId)
        {
            if (!_isAuthenticated)
                throw new InvalidOperationException("Not authenticated with IMAP server");

            var tag = GetNextTag();
            var command = $"{tag} FETCH {messageId} (BODY[])\r\n";
            await _writer!.WriteAsync(command);
            var response = await ReadResponseAsync();

            // Extract the literal block content
            var lines = response.Split(["\r\n", "\n"], StringSplitOptions.None);
            var builder = new StringBuilder();
            bool inBlock = false;
            foreach (var line in lines)
            {
                if (!inBlock)
                {
                    // Detect start of BODY[] literal
                    if (line.Contains("BODY[]") && line.Contains("{"))
                    {
                        inBlock = true;
                        continue;
                    }
                }
                else
                {
                    if (line.StartsWith(tag)) break;
                    builder.AppendLine(line);
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Logs out and disconnects from the IMAP server.
        /// </summary>
        public async Task LogoutAsync()
        {
            if (!_isConnected)
                return;

            var tag = GetNextTag();
            var command = $"{tag} LOGOUT\r\n";
            try
            {
                await _writer!.WriteAsync(command);
                string? line;

                while ((line = await _reader!.ReadLineAsync()) != null)
                {
                    if (line.StartsWith(tag))
                        break;
                }
            }
            catch
            {
                // Ignore errors during logout
            }
            finally
            {
                _isAuthenticated = false;
                _isConnected = false;
                // underlying streams remain until Dispose()
            }
        }

        /// <summary>
        /// Connects to the IMAP server using the provided settings.
        /// </summary>
        /// <param name="settings">IMAP connection settings</param>
        public async Task ConnectAsync(ImapSettings settings)
        {
            settings.Validate();
            await ConnectAsync(settings.Host, settings.Port);
        }

        /// <summary>
        /// Connects and authenticates using the provided settings.
        /// </summary>
        /// <param name="settings">IMAP connection settings including credentials</param>
        public async Task ConnectAndLoginAsync(ImapSettings settings)
        {
            settings.Validate();
            await ConnectAsync(settings.Host, settings.Port);
            await LoginAsync(settings.Username, settings.Password);
        }

        #region Private Helper Methods
        private string GetNextTag()
        {
            return $"A{_commandTagCounter++:000}";
        }

        private async Task<string> ReadResponseAsync()
        {
            var response = new StringBuilder();
            string? line;

            while ((line = await _reader!.ReadLineAsync()) != null)
            {
                response.AppendLine(line);

                // Check for literal data indicator {<number>}
                if (line.Contains('{') && line.Contains('}'))
                {
                    var literalMatch = System.Text.RegularExpressions.Regex.Match(line, @"\{(\d+)\}");
                    if (literalMatch.Success)
                    {
                        var literalSize = int.Parse(literalMatch.Groups[1].Value);
                        var buffer = new char[literalSize];
                        await _reader.ReadAsync(buffer, 0, literalSize);
                        response.Append(buffer);

                        await _reader.ReadLineAsync();
                    }
                }

                if (line.StartsWith($"A{_commandTagCounter - 1:000}"))
                {
                    break;
                }
            }

            return response.ToString();
        }

        private List<EmailHeader> ParseEmailHeaders(string response)
        {
            var headers = new List<EmailHeader>();
            var lines = response.Split('\n');

            foreach (var line in lines)
            {
                if (line.Contains("ENVELOPE"))
                {
                    // Basic envelope parsing - this is simplified
                    // In a full implementation, you'd parse the ENVELOPE structure properly
                    var header = new EmailHeader
                    {
                        Subject = ExtractFromEnvelope(line, "subject"),
                        From = ExtractFromEnvelope(line, "from"),
                        Date = ExtractFromEnvelope(line, "date")
                    };
                    headers.Add(header);
                }
            }

            return headers;
        }

        private EmailMessage ParseEmailMessage(string response, int messageId)
        {
            var lines = response.Split('\n');
            var bodyBuilder = new StringBuilder();
            var inBody = false;

            foreach (var line in lines)
            {
                if (line.Contains("BODY[]"))
                {
                    inBody = true;
                    continue;
                }

                if (inBody && line.StartsWith($"A{_commandTagCounter - 1:000}"))
                {
                    break;
                }

                if (inBody)
                {
                    bodyBuilder.AppendLine(line);
                }
            }

            return new EmailMessage
            {
                Id = messageId,
                RawContent = bodyBuilder.ToString()
            };
        }

        private string ExtractFromEnvelope(string envelope, string field)
        {
            // Simplified envelope parsing
            // In a real implementation, we'll properly parse the IMAP ENVELOPE structure
            return $"[{field} from envelope]";
        }
        #endregion

        /// <summary>
        /// Logs out and releases all resources synchronously.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            try
            {
                LogoutAsync().Wait(5000);
            }
            catch
            {
                // Ignore logout errors
            }

            _reader?.Dispose();
            _writer?.Dispose();
            _sslStream?.Dispose();
            _tcpClient?.Dispose();

            _disposed = true;
        }

        /// <summary>
        /// Asynchronously logs out and releases all resources used by this instance.
        /// </summary>
        /// <returns>A ValueTask representing the asynchronous dispose operation.</returns>
        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            try
            {
                await LogoutAsync();
            }
            catch
            {
                // Ignore logout errors
            }

            _reader?.Dispose();
            _writer?.Dispose();
            _sslStream?.Dispose();
            _tcpClient?.Dispose();

            _disposed = true;
        }
    }
}
