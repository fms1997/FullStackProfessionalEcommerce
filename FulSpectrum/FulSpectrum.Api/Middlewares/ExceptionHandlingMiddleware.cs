using System.Text.Json;
using System.Diagnostics;
 namespace FulSpectrum.Api.Middlewares;

public sealed class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly IHostEnvironment _env;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(IHostEnvironment env, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
            var correlationId = context.TraceIdentifier;

            _logger.LogError(
                ex,
                "Unhandled exception. CorrelationId={CorrelationId} TraceId={TraceId}",
                correlationId,
                traceId);

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var problem = new
            {
                type = "https://httpstatuses.com/500",
                title = "Unexpected error",
                status = 500,
                detail = _env.IsDevelopment() ? ex.ToString() : "An unexpected error occurred.",
                traceId,
                correlationId
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
    }
}