using BusinessLayer.Common;

namespace BusinessLayer.Interfaces;

public interface IEmailQueue
{
    void Enqueue(EmailMessage message);
    IAsyncEnumerable<EmailMessage> ReadAllAsync(CancellationToken cancellationToken);
}
