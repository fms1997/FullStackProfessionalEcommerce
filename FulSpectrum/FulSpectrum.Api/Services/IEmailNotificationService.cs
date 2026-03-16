namespace FulSpectrum.Api.Services;

public interface IEmailNotificationService
{
    Task SendAsync(string to, string subject, string body, CancellationToken ct = default);
}
