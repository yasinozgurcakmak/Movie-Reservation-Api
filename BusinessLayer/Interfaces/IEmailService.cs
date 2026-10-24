namespace BusinessLayer.Interfaces;

public interface IEmailService
{
    Task SendAsync(string toAddress, string subject, string htmlBody);
}
