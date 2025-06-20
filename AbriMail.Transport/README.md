# AbriMail.Transport

This library provides low-level IMAP4rev1 protocol implementation for connecting to email servers over TLS/SSL.

## Features

- **Secure Connection**: Connects to IMAP servers using implicit TLS on port 993
- **Basic Authentication**: Supports plain username/password authentication for IMAP
- **SMTP Sending**: Send emails via SMTP over implicit TLS on port 465 (EHLO, AUTH LOGIN, MAIL FROM, RCPT TO, DATA, QUIT)
- **Mailbox Operations**: Select and examine mailboxes (primarily INBOX)
- **Message Retrieval**: Fetch email headers and full message content
- **Protocol Compliance**: Implements essential IMAP4rev1 and SMTP commands

## Supported IMAP Commands

- `LOGIN` - Authenticate with username/password
- `SELECT` - Select a mailbox (INBOX)
- `FETCH` - Retrieve message headers and content
- `FETCH BODY[HEADER]` - Retrieve raw headers for a range of messages
- `FETCH BODY[]` - Retrieve the full raw message (headers + body)
- `LOGOUT` - Close the connection

## IMAP Summary:

In summary, AbriMail.Transport’s IMAP implementation log in to the server, select INBOX, fetch either headers or full messages, and logout. It adheres to IMAP text protocol but only implements the commands needed for a basic read-only inbox view. This minimal set (LOGIN, SELECT, FETCH, LOGOUT) is sufficient to retrieve messages from a mailbox. Advanced features like searching or marking messages read are omitted in the version 1.0.0.

## Usage Example

```csharp
using AbriMail.Transport;

// Create settings (example for Gmail)
var settings = new ImapSettings
{
    Host = "imap.gmail.com",
    Port = 993,
    Username = "user@gmail.com",
    Password = "app-password"
};

using var client = new ImapClient();

try
{
    // Connect and authenticate
    await client.ConnectAndLoginAsync(settings);

    // Select INBOX
    var inbox = await client.SelectMailboxAsync();
    Console.WriteLine($"Total: {inbox.MessageCount}, Recent: {inbox.RecentCount}");

    // Fetch headers for first 5 messages
    var headers = await client.FetchHeadersAsync(1, 5);
    foreach (var header in headers)
    {
        Console.WriteLine($"From: {header.From}, Subject: {header.Subject}");
    }

    // Fetch full message #1
    var message = await client.FetchMessageAsync(1);
    Console.WriteLine($"Body: {message.Body}");

    // Logout when done
    await client.LogoutAsync();
}
catch (ImapException ex)
{
    Console.WriteLine($"IMAP Error: {ex.Message}");
}
```

## API Methods

### IMAP (Email Retrieval)

**Connection & Authentication**

- `ConnectAsync(string host, int port=993)`
- `ConnectAsync(ImapSettings settings)`
- `ConnectAndLoginAsync(ImapSettings settings)`
- `LoginAsync(string username, string password)`
- `LogoutAsync()`

**Mailbox & Messages**

- `SelectMailboxAsync(string mailbox = "INBOX") -> MailboxInfo`
- `FetchHeadersAsync(int start, int count) -> List<EmailHeader>`
- `FetchMessageAsync(int messageId) -> EmailMessage`

### SMTP (Email Sending)

**Connection & Sending**

- `ConnectAsync(string host, int port=465)`
- `EhloAsync(string domain) -> IList<string>`
- `AuthenticateAsync(string username, string password)`
- `MailFromAsync(string sender)`
- `RcptToAsync(string recipient)`
- `DataAsync(string content)`
- `QuitAsync()`
- `SendEmailAsync(string host, int port, string domain, string username, string password, MailMessageDetails details)`

## Usage Examples

### IMAP Example

...existing IMAP example...

### SMTP Example

```csharp
using AbriMail.Transport;
using AbriMail.Transport.Models;

var smtpClient = new SmtpClient();
var message = new MailMessageDetails
{
    From = "user@example.com",
    To = new List<string> { "recipient@example.com" },
    Subject = "Test Email",
    Body = "Hello, this is a test.",
    ContentType = "text/plain"
};

await smtpClient.SendEmailAsync(
    host: "smtp.server.com",
    port: 465,
    domain: "example.com",
    username: "user@example.com",
    password: "password",
    details: message
);
```

## Error Handling

Transport methods throw `ImapException` on protocol errors (`NO`/`BAD` responses). The exception includes:

- `ServerResponse` — the raw server reply causing the error
- `FailedCommand` — the IMAP command sent

Clients should catch `ImapException` to present meaningful messages to the user.

## Classes

### ImapClient

Main client class implementing `IImapClient` interface.

**Methods:**

- `ConnectAsync(host, port)` - Establish TLS connection
- `LoginAsync(username, password)` - Authenticate
- `SelectMailboxAsync(mailbox)` - Select mailbox
- `FetchHeadersAsync(start, count)` - Get message headers
- `FetchMessageAsync(messageId)` - Get full message
- `LogoutAsync()` - Disconnect

### Models

**MailboxInfo** - Information about selected mailbox

- `MessageCount` - Total messages
- `RecentCount` - New messages
- `Name` - Mailbox name
- `IsReadOnly` - Write permission

**EmailHeader** - Message header information

- `SequenceNumber` - Message ID
- `Subject`, `From`, `To` - Basic headers
- `Date` - Send date
- `Size` - Message size
- `Flags` - IMAP flags

**EmailMessage** - Complete message

- `Id` - Message sequence number
- `RawContent` - Full RFC822 content
- `Headers` - Parsed headers dictionary
- `Body` - Message body
- `Subject`, `From`, `To`, `Date` - Extracted fields

**MailMessageDetails** - Email message details for sending

- `From` - Sender's email address
- `To` - Recipient's email addresses
- `Subject` - Email subject
- `Body` - Email body content
- `ContentType` - MIME content type (e.g., "text/plain", "text/html")

### Exceptions

**ImapException** - IMAP-specific errors

- `ServerResponse` - Server error response
- `FailedCommand` - Command that failed

## Limitations

This is a minimal implementation focused on basic email retrieval:

- **Read-only**: No support for marking messages, moving, or deleting
- **No Attachments**: Simple text/HTML message handling only
- **Basic MIME**: Limited MIME multipart support
- **No Search**: No IMAP SEARCH command support
- **No IDLE**: No real-time notifications
- **Single Mailbox**: Focused on INBOX access

## Protocol Details

- Uses IMAP4rev1 (RFC 3501)
- Commands are tagged (A001, A002, etc.)
- Handles literal data blocks `{size}`
- Parses untagged (`*`) and tagged responses
- ASCII encoding for protocol communication
- TLS/SSL required (no plain text)

## Dependencies

- .NET 9.0
- System.Net.Security (SslStream)
- System.Net.Sockets (TcpClient)

## Thread Safety

The ImapClient is **not thread-safe**. Create separate instances for concurrent operations or use proper synchronization.
