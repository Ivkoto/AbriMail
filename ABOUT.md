## About AbriMail

AbriMail is a minimal, open-source Blazor Server email client built on raw IMAP4rev1 and SMTP protocols over implicit TLS. It demonstrates how to implement both receiving and sending email functionality without relying on third-party libraries.

### Who Can Use It

- Developers learning how IMAP and SMTP work under the hood
- Teams needing a simple web-based email client prototype
- Anyone curious about building secure networked applications in .NET and Blazor

### Key Features

- **Server-side Blazor UI** with asynchronous operations to avoid blocking the server circuit
- **Settings** page for user-driven configuration of IMAP/SMTP servers, ports, and credentials
- **Inbox** page showing message headers (From, Subject, Date)
- **Message** viewer for reading full email bodies (plain-text and HTML)
- **Compose** page for drafting and sending new emails (AUTH LOGIN, MAIL FROM, RCPT TO, DATA)
- Full transport layer in `AbriMail.Transport` project: low-level IMAP and SMTP clients using `TcpClient`, `SslStream`, and plain text commands

### What’s Included

1. **AbriMail.Transport** – Protocol implementation
   - `ImapClient` for secure IMAP4rev1 connections and message retrieval
   - `SmtpClient` for secure SMTP (implicit TLS) and email sending
   - Data models: `EmailHeader`, `EmailMessage`, `MailMessageDetails`, and settings
2. **AbriMail.App** – (Placeholder project for future business logic or shared services)
3. **AbriMail.Web** – Blazor Server application
   - Razor components for Settings, Mailbox (inbox), Message view, and Compose
   - In-memory `MailService` implementing `IMailService` to tie UI to transport layer
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
