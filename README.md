# AbriMail <img src="AbriMail.Web/wwwroot/favicon.png" alt="AbriMail" width="32" height="32">

.Net | Blazor Server Email Client (IMAP/SMTP)

## About

**AbriMail is a minimal, open-source Blazor Server email client built on raw IMAP4rev1 and SMTP protocols over implicit TLS. It demonstrates how to implement both receiving and sending email functionality without relying on third-party libraries.**

**A simple implementation of a Blazor Server web-based email client.**

- The client will use the IMAP4rev1 and SMTP protocols over implicit TLS/SSL to fetch and send emails.
- The solution is structured as three separate .NET projects to enforce a clean separation of concerns.
- At the beggining only the basic email features will be implemented, focusing on clarity and simplicity while acknowledging limitations.

### Who Can Use It

- Developers learning how IMAP and SMTP work under the hood
- Teams needing a simple web-based email client prototype
- Anyone curious about building secure networked applications in .NET and Blazor

### Key Features

- **Server-side Blazor UI** with asynchronous operations and responsive design
- **Settings** page with email provider presets (Gmail, Outlook, Yahoo) and connection testing
- **Inbox** page with email list, loading states, and refresh functionality
- **Message** viewer with formatted display and navigation controls
- **Compose** page with multi-recipient support, validation, and user feedback
- **Professional styling** with Bootstrap integration and custom CSS enhancements
- **Error handling** with user-friendly messages and retry mechanisms
- **Session-based state** management without persistent storage
- **Clean architecture** with proper separation of concerns across three layers

### What’s Included

1. **AbriMail.Transport** – Low-level protocol implementation
   - `ImapClient` for secure IMAP4rev1 connections and message retrieval
   - `SmtpClient` for secure SMTP (implicit TLS) and email sending
   - Data models: `EmailHeader`, `EmailMessage`, `MailMessageDetails`, and settings
   - Exception handling with `ImapException` for protocol errors
2. **AbriMail.App** – Business logic layer
   - `IMailboxService` interface for high-level email operations
   - `MailboxService` implementation with session-based state management
   - DTOs for clean data transfer: `MailHeaderDto`, `MailMessageDto`, `SendEmailDto`
   - Connection state tracking and validation
3. **AbriMail.Web** – Blazor Server application
   - Razor components for Settings, Mailbox (inbox), Message view, and Compose
   - Professional UI with Bootstrap styling and loading states
   - Email provider presets (Gmail, Outlook, Yahoo) for quick setup
   - Comprehensive error handling and user feedback
   - Dependency injection setup in `Program.cs`

### Getting Started

1. Clone the repository and open **AbriEmailClient.sln** in Visual Studio or VS Code.
2. Update your IMAP/SMTP server details in the **Settings** page at runtime.
3. Navigate to **Inbox** to fetch new messages.
4. Use **Compose** to send emails immediately.

### Limitations

- No persistent storage or caching of messages
- No support for attachments or advanced MIME parts
- Read-only message retrieval (no flags, search, or folder management)
- No OAuth or modern token-based authentication (plain username/password only)

### License

This project is licensed under the **MIT License**. See [LICENSE](LICENSE) for details.

---

### Project Structure & Responsibilities

**AbriMail.Transport (Class Library):**

- handles low-level email protocol communication.
- contains classes to connect to mail servers and execute raw IMAP and SMTP commands over TLS.
- establishing SslStream connections, sending protocol commands, and reading server responses.
- isolates the complexity of the IMAP/SMTP protocols from the rest of the app.
- **Documentation:** [AbriMail.Transport README](AbriMail.Transport/README.md)

**AbriMail.App (Class Library):**

- business logic layer that uses _Mailbox.Transport_.
- provides methods to the UI for common operations: e.g. “fetch inbox messages” or “send email”.
- calls Transport to perform IMAP/SMTP actions and then parses the results into simple C# models.
- handles any necessary logic like selecting the mailbox or formatting an email for sending.
- acts as a service that the Blazor UI can consume (via dependency injection) without dealing with protocol details.
- **Documentation:** [AbriMail.App README](AbriMail.App/README.md)

**AbriMail.Web (Blazor Server App):**

- Blazor Server web application providing the user interface
- contains Razor pages/components for settings, inbox list, email viewing, and composition.
- uses dependency injection to get the AbriMail.App services
- wires up configuration and registers the services from AbriMail.App and AbriMail.Transport in the DI container
