using BusinessLayer.Interfaces;

namespace MovieReservationAPI.BackgroundServices;

public class EmailDispatcher : BackgroundService
{
    private readonly IEmailQueue _queue;
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailDispatcher> _logger;

    public EmailDispatcher(IEmailQueue queue, IEmailService emailService, ILogger<EmailDispatcher> logger)
    {
        _queue = queue;
        _emailService = emailService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in _queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                await _emailService.SendAsync(message.ToAddress, message.Subject, message.HtmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email '{Subject}'", message.Subject);
            }
        }
    }
}
