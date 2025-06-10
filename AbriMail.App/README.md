# AbriMail.App

This library provides the business logic layer for the AbriMail email client, acting as an intermediary between the UI (AbriMail.Web) and the transport layer (AbriMail.Transport).

## Features

- **High-level Email Operations**: Simplified interface for common email tasks
- **Session-based State Management**: Tracks connection state and user settings
- **Data Transfer Objects**: Clean DTOs for UI communication
- **Connection State Tracking**: Smart handling of login state across components
- **Error Recovery**: Graceful degradation and retry mechanisms
- **Clean Architecture**: Proper separation between UI and protocol layers

## Core Components

### IMailboxService Interface

The main service contract that defines all email operations available to the UI layer.

**Methods:**

- `LoginAsync(ServerSettings settings)` - Establishes connections to IMAP/SMTP servers
- `FetchInboxHeadersAsync()` - Retrieves email headers from INBOX
- `FetchEmailAsync(int sequenceNumber)` - Gets full email content by sequence number
- `SendEmailAsync(SendEmailDto message)` - Sends email via SMTP

**Properties:**

- `Settings` - Current server configuration (throws if not logged in)
- `IsLoggedIn` - Indicates whether user is authenticated

### MailboxService Implementation

Session-scoped service that implements `IMailboxService` using the Transport layer.

**Key Features:**

- **Connection Management**: Handles IMAP/SMTP client lifecycle
- **State Validation**: Ensures operations are performed on authenticated connections
- **Data Transformation**: Converts transport models to UI-friendly DTOs
- **Error Handling**: Provides meaningful exceptions for UI consumption

## Data Models

### ServerSettings

Configuration model for IMAP and SMTP server connections.

```csharp
public class ServerSettings
{
    public string ImapServer { get; set; }
    public int ImapPort { get; set; }
    public string ImapUsername { get; set; }
    public string ImapPassword { get; set; }

    public string SmtpServer { get; set; }
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; }
    public string SmtpPassword { get; set; }
}
```

### Data Transfer Objects (DTOs)

**MailHeaderDto** - Email header information for inbox display

- `Id` - Message sequence number
- `Subject` - Email subject line
- `From` - Sender's email address
- `Date` - Send date/time

**MailMessageDto** - Complete email message

- `Id` - Message sequence number
- `Subject` - Email subject line
- `From` - Sender's email address
- `To` - Recipient's email address
- `Date` - Send date/time
- `Body` - Email message body

**SendEmailDto** - Email composition data

- `To` - List of recipient email addresses
- `Subject` - Email subject line
- `Body` - Email message content

## Usage Example

```csharp
// Dependency injection setup (in Program.cs)
builder.Services.AddScoped<IMailboxService, MailboxService>();

// In a Razor component
@inject IMailboxService MailService

// Login to servers
var settings = new ServerSettings
{
    ImapServer = "imap.gmail.com",
    ImapPort = 993,
    ImapUsername = "user@gmail.com",
    ImapPassword = "app-password",
    SmtpServer = "smtp.gmail.com",
    SmtpPort = 465,
    SmtpUsername = "user@gmail.com",
    SmtpPassword = "app-password"
};

await MailService.LoginAsync(settings);

// Fetch inbox headers
var headers = await MailService.FetchInboxHeadersAsync();

// Read a specific message
var message = await MailService.FetchEmailAsync(1);

// Send a new email
var newEmail = new SendEmailDto
{
    To = new List<string> { "recipient@example.com" },
    Subject = "Test Message",
    Body = "Hello from AbriMail!"
};

await MailService.SendEmailAsync(newEmail);
```

## Architecture Benefits

### Clean Separation

- **No UI Logic**: App layer contains no Blazor or UI-specific code
- **No Protocol Details**: UI layer doesn't need to understand IMAP/SMTP
- **Testable**: Business logic can be unit tested independently

### Session Management

- **Stateful Service**: Maintains connection state during user session
- **Connection Reuse**: Efficient resource management
- **State Validation**: Prevents operations on disconnected clients

### Error Handling

- **Protocol Translation**: Converts low-level exceptions to user-friendly messages
- **State Validation**: Clear error messages for authentication issues
- **Recovery Guidance**: Provides actionable error information

## Dependencies

- **.NET 9.0** - Target framework
- **AbriMail.Transport** - Low-level protocol implementation
- **System.ComponentModel.DataAnnotations** - Validation attributes

## Limitations

- **Session-based**: No persistent storage of emails or settings
- **Single User**: Designed for single-user sessions
- **Basic Validation**: Minimal input validation (relies on Transport layer)
- **No Caching**: Fresh data retrieved on each request

## Integration

This library is designed to be consumed by:

- **AbriMail.Web** - Blazor Server UI application
- **Future UI Projects** - Could support other UI frameworks
- **Testing Projects** - Easy to mock for unit testing

The service is registered in the DI container with scoped lifetime, ensuring proper session management in Blazor Server applications.
