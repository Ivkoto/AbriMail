# AbriMail

.Net | Blazor Server Email Client (IMAP/SMTP)

**A simple implementation of a Blazor Server web-based email client.**

- The client will use the IMAP4rev1 and SMTP protocols over implicit TLS/SSL to fetch and send emails.
- The solution is structured as three separate .NET projects to enforce a clean separation of concerns.
- At the beggining only the basic email features will be implemented, focusing on clarity and simplicity while acknowledging limitations.
- No third-party email libraries (e.g. MailKit) will be used – all IMAP/SMTP interactions are handled with raw sockets and SslStream.

---

### Main Scope:

**Protocols:**

- IMAP for receiving.
- SMTP for sending.
- using implicit TLS on the standard secure ports (IMAP on 993, SMTP on 465).

**Authentication:**

- plain username/password.
- no OAuth or token-based auth.

**No Offline Storage:**

- The client fetches emails on demand (no local caching or offline mode).

**Simplified IMAP Commands:**

- support only LOGIN, selecting the INBOX, and FETCH for message headers or full bodies.
- skipping advanced commands like SEARCH, IDLE, flags, or folder management.

**Simplified SMTP Commands:**

- support only basic SMTP handshake and send commands (EHLO, AUTH LOGIN, MAIL FROM, RCPT TO, DATA, QUIT).
- no support for attachments or complex MIME parts in outgoing mail.

**Configuration:**

- UI provided to user to input server settings - IMAP/SMTP host, ports, credentials.

**Basic UI Pages:**

- pages for configuring settings.
- viewing the inbox - list of messages.
- reading a message.
- composing/sending a new email.

---

### Project Structure & Responsibilities

**AbriMail.Transport (Class Library):**

- handles low-level email protocol communication.
- contains classes to connect to mail servers and execute raw IMAP and SMTP commands over TLS.
- establishing SslStream connections, sending protocol commands, and reading server responses.
- isolates the complexity of the IMAP/SMTP protocols from the rest of the app.

**AbriMail.App (Class Library):**

- business logic layer that uses _Mailbox.Transport_.
- provides methods to the UI for common operations: e.g. “fetch inbox messages” or “send email”.
- calls Transport to perform IMAP/SMTP actions and then parses the results into simple C# models.
- handles any necessary logic like selecting the mailbox or formatting an email for sending.
- acts as a service that the Blazor UI can consume (via dependency injection) without dealing with protocol details.

**AbriMail.Web (Blazor Server App):**

- Blazor Server web application providing the user interface
- contains Razor pages/components for settings, inbox list, email viewing, and composition.
- uses dependency injection to get the Mailbox.App services.
- wires up configuration and registers the services from Mailbox.App and Mailbox.Transport in the DI container.
