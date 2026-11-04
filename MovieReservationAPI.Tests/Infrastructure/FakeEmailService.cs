using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using BusinessLayer.Common;
using BusinessLayer.Interfaces;

namespace MovieReservationAPI.Tests.Infrastructure;

public partial class FakeEmailService : IEmailService
{
    private readonly ConcurrentQueue<EmailMessage> _sent = new();

    public IReadOnlyCollection<EmailMessage> Sent => _sent;

    public Task SendAsync(string toAddress, string subject, string htmlBody)
    {
        _sent.Enqueue(new EmailMessage(toAddress, subject, htmlBody));
        return Task.CompletedTask;
    }

    // Emails are sent by a background service, so the test waits until the expected one arrives.
    public async Task<EmailMessage> WaitForAsync(string toAddress, string subject)
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            var message = _sent.LastOrDefault(m => m.ToAddress == toAddress && m.Subject == subject);
            if (message is not null) return message;
            await Task.Delay(50);
        }

        throw new TimeoutException($"No '{subject}' email was sent to {toAddress}.");
    }

    public static string ExtractToken(EmailMessage message) => TokenPattern().Match(message.HtmlBody).Groups[1].Value;

    [GeneratedRegex("token=([0-9A-F]+)")]
    private static partial Regex TokenPattern();
}
