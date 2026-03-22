using System.Diagnostics;

namespace FulSpectrum.Api.Middlewares;

public sealed class AuditTrailMiddleware
{
    private static readonly HashSet<string> SensitivePaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/v1/auth/login",
        "/api/v1/auth/register",
        "/api/v1/auth/refresh",
        "/api/v1/auth/logout",
        "/api/v1/auth/forgot-password",
        "/api/v1/auth/reset-password"
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<AuditTrailMiddleware> _logger;

    public AuditTrailMiddleware(RequestDelegate next, ILogger<AuditTrailMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        await _next(context);

        var isSensitivePath = SensitivePaths.Contains(context.Request.Path.Value ?? string.Empty);
        var isSensitiveMethod = HttpMethods.IsPost(context.Request.Method)
            || HttpMethods.IsPut(context.Request.Method)
            || HttpMethods.IsDelete(context.Request.Method)
            || HttpMethods.IsPatch(context.Request.Method);

        if (!isSensitivePath && !isSensitiveMethod)
        {
            return;
        }

        var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;

        _logger.LogInformation(
            "AuditEvent Action={Action} Path={Path} Method={Method} StatusCode={StatusCode} UserId={UserId} Ip={Ip} TraceId={TraceId}",
            "http_request",
            context.Request.Path.Value,
            context.Request.Method,
            context.Response.StatusCode,
            userId,
            ip,
            traceId);
    }
}
