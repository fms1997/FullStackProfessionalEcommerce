using System.Diagnostics;
using Serilog.Context;

namespace FulSpectrum.Api.Middlewares;

public sealed class RequestLogEnrichmentMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLogEnrichmentMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var userId = context.User?.Identity?.IsAuthenticated == true
            ? context.User.FindFirst("sub")?.Value ?? context.User.FindFirst("id")?.Value ?? "authenticated"
            : "anonymous";

        var activity = Activity.Current;

        using (LogContext.PushProperty("UserId", userId))
        using (LogContext.PushProperty("RequestPath", context.Request.Path.Value))
        using (LogContext.PushProperty("HttpMethod", context.Request.Method))
        using (LogContext.PushProperty("TraceId", activity?.TraceId.ToString() ?? string.Empty))
        using (LogContext.PushProperty("SpanId", activity?.SpanId.ToString() ?? string.Empty))
        {
            await _next(context);
        }
    }
}
