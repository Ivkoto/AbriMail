using AbriMail.Transport.Models;

namespace AbriMail.Transport.Example;

/// <summary>
/// Simple example demonstrating how to use the IMAP client.
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("AbriMail IMAP Client Example");
        Console.WriteLine("=============================\n");

        // Example using settings configuration
        await ExampleWithSettings();

        Console.WriteLine("\n" + new string('-', 50) + "\n");

        // Example using direct connection
        await ExampleDirectConnection();
    }

    private static async Task ExampleWithSettings()
    {
        Console.WriteLine("Example 1: Using ImapSettings configuration\n");

        // Create settings for a mail provider (example with Gmail)
        var settings = ImapSettings.Presets.Gmail("your-email@gmail.com", "your-password");

        // For testing purposes, you could also create custom settings:
        // var settings = new ImapSettings
        // {
        //     Host = "mail.your-provider.com",
        //     Port = 993,
        //     Username = "your-email@domain.com",
        //     Password = "your-password"
        // };

        using var imapClient = new ImapClient();

        try
        {
            Console.WriteLine($"Connecting to {settings.Host}:{settings.Port}...");
            await imapClient.ConnectAndLoginAsync(settings);
            Console.WriteLine("✓ Connected and authenticated successfully!");

            await DisplayMailboxInfo(imapClient);
            await DisplayRecentMessages(imapClient);
        }
        catch (ImapException ex)
        {
            Console.WriteLine($"❌ IMAP Error: {ex.Message}");
            if (!string.IsNullOrEmpty(ex.ServerResponse))
            {
                Console.WriteLine($"Server Response: {ex.ServerResponse}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }

    private static async Task ExampleDirectConnection()
    {
        Console.WriteLine("Example 2: Direct connection method\n");

        using var imapClient = new ImapClient();

        try
        {
            Console.WriteLine("Connecting to imap.fastmail.com:993...");
            await imapClient.ConnectAsync("imap.fastmail.com", 993);
            Console.WriteLine("✓ Connected to server!");

            Console.WriteLine("Authenticating...");
            await imapClient.LoginAsync("your-email@fastmail.com", "your-password");
            Console.WriteLine("✓ Authenticated successfully!");

            await DisplayMailboxInfo(imapClient);
        }
        catch (ImapException ex)
        {
            Console.WriteLine($"❌ IMAP Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }

    private static async Task DisplayMailboxInfo(IImapClient imapClient)
    {
        Console.WriteLine("\nSelecting INBOX...");
        var mailboxInfo = await imapClient.SelectMailboxAsync("INBOX");

        Console.WriteLine($"✓ INBOX selected:");
        Console.WriteLine($"  • Total messages: {mailboxInfo.MessageCount}");
        Console.WriteLine($"  • Recent messages: {mailboxInfo.RecentCount}");
        Console.WriteLine($"  • Read-only: {mailboxInfo.IsReadOnly}");
    }

    private static async Task DisplayRecentMessages(IImapClient imapClient)
    {
        Console.WriteLine("\nFetching recent message headers...");

        try
        {
            var headers = await imapClient.FetchHeadersAsync(1, Math.Min(5, 10)); // Fetch first 5 messages

            if (headers.Count == 0)
            {
                Console.WriteLine("No messages found in INBOX.");
                return;
            }

            Console.WriteLine($"✓ Found {headers.Count} message(s):\n");

            for (int i = 0; i < headers.Count; i++)
            {
                var header = headers[i];
                Console.WriteLine($"Message {i + 1}:");
                Console.WriteLine($"  From: {header.From}");
                Console.WriteLine($"  Subject: {header.Subject}");
                Console.WriteLine($"  Date: {header.Date}");
                Console.WriteLine($"  Size: {header.Size} bytes");
                Console.WriteLine();
            }

            // Fetch full content of first message as example
            if (headers.Count > 0)
            {
                Console.WriteLine("Fetching full content of first message...");
                var message = await imapClient.FetchMessageAsync(1);

                Console.WriteLine("✓ Message content retrieved:");
                Console.WriteLine($"  Subject: {message.Subject}");
                Console.WriteLine($"  From: {message.From}");
                Console.WriteLine($"  To: {message.To}");
                Console.WriteLine($"  Content Type: {message.ContentType}");
                Console.WriteLine($"  Body Length: {message.Body.Length} characters");

                if (message.Body.Length > 200)
                {
                    Console.WriteLine($"  Body Preview: {message.Body.Substring(0, 200)}...");
                }
                else
                {
                    Console.WriteLine($"  Body: {message.Body}");
                }
            }
        }
        catch (ImapException ex)
        {
            Console.WriteLine($"❌ Failed to fetch messages: {ex.Message}");
        }
    }
}
