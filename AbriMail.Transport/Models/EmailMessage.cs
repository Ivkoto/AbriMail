namespace AbriMail.Transport.Models;

/// <summary>
/// Represents a complete email message retrieved from IMAP.
/// </summary>
public class EmailMessage
{
    /// <summary>
    /// Message sequence number (1-based) in the mailbox.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Raw RFC822 message content including headers and body.
    /// </summary>
    public string RawContent { get; set; } = string.Empty;

    /// <summary>
    /// Parsed email headers.
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Email body content (text or HTML).
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Subject extracted from headers.
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Sender information extracted from headers.
    /// </summary>
    public string From { get; set; } = string.Empty;

    /// <summary>
    /// Recipient information extracted from headers.
    /// </summary>
    public string To { get; set; } = string.Empty;

    /// <summary>
    /// Date when the email was sent.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Content type of the message (text/plain, text/html, etc.).
    /// </summary>
    public string ContentType { get; set; } = "text/plain";

    /// <summary>
    /// Parses the raw content to extract headers and body.
    /// </summary>
    public void ParseRawContent()
    {
        if (string.IsNullOrEmpty(RawContent))
            return;

        var lines = RawContent.Split('\n');
        var headerSection = true;
        var bodyBuilder = new System.Text.StringBuilder();

        foreach (var line in lines)
        {
            if (headerSection)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    headerSection = false;
                    continue;
                }

                // Parse header line
                var colonIndex = line.IndexOf(':');
                if (colonIndex > 0)
                {
                    var headerName = line.Substring(0, colonIndex).Trim().ToLowerInvariant();
                    var headerValue = line.Substring(colonIndex + 1).Trim();

                    Headers[headerName] = headerValue;

                    // Extract common headers
                    switch (headerName)
                    {
                        case "subject":
                            Subject = headerValue;
                            break;
                        case "from":
                            From = headerValue;
                            break;
                        case "to":
                            To = headerValue;
                            break;
                        case "date":
                            if (DateTime.TryParse(headerValue, out var date))
                                Date = date;
                            break;
                        case "content-type":
                            ContentType = headerValue.Split(';')[0].Trim();
                            break;
                    }
                }
            }
            else
            {
                bodyBuilder.AppendLine(line);
            }
        }

        Body = bodyBuilder.ToString().Trim();
    }
}
