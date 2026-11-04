using System.Threading.Channels;
using BusinessLayer.Common;
using BusinessLayer.Interfaces;

namespace BusinessLayer.Concrete;

// Emails are sent in the background so a request never waits for SMTP, and response times do not reveal
// whether an email was sent (e.g. whether an account exists). The queue is in memory: pending emails are lost on restart.
public class EmailQueue : IEmailQueue
{
    private readonly Channel<EmailMessage> _channel =
        Channel.CreateUnbounded<EmailMessage>(new UnboundedChannelOptions { SingleReader = true });

    public void Enqueue(EmailMessage message) => _channel.Writer.TryWrite(message);

    public IAsyncEnumerable<EmailMessage> ReadAllAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}
