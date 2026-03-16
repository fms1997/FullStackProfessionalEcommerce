using Serilog;

namespace FulSpectrum.Api.Services;

public sealed class LoggingEmailNotificationService : IEmailNotificationService
{
    public Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        Log.Information("EMAIL -> To:{To} Subject:{Subject} Body:{Body}", to, subject, body);
        return Task.CompletedTask;
    }
}
