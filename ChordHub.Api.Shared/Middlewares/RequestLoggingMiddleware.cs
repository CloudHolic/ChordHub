using System.Diagnostics;

using Microsoft.AspNetCore.Http;

using Serilog;
using Serilog.Context;
using Serilog.Events;

namespace ChordHub.Api.Shared.Middlewares;

public class RequestLoggingMiddleware(RequestDelegate next)
{
    private readonly ILogger _logger = Log.ForContext<RequestLoggingMiddleware>();

    public async Task InvokeAsync(HttpContext context)
    {
        var stopWatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString();

        using (LogContext.PushProperty("RequestId", requestId))
        using (LogContext.PushProperty("UserAgent", context.Request.Headers.UserAgent.ToString()))
        using (LogContext.PushProperty("RemoteIP", context.Connection.RemoteIpAddress?.ToString()))
        {
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                using (LogContext.PushProperty("UserId", context.User.FindFirst("sub")?.Value ?? "unknown"))
                using (LogContext.PushProperty("UserEmail", context.User.FindFirst("email")?.Value ?? "unknown"))
                {
                    await ProcessRequest(context, stopWatch, requestId);
                }
            }
            else
                await ProcessRequest(context, stopWatch, requestId);
        }
    }

    private async Task ProcessRequest(HttpContext context, Stopwatch stopwatch, string requestId)
    {
        var request = context.Request;

        _logger.Information("HTTP {Method} {Path} started", request.Method, request.Path);

        try
        {
            await next(context);
        }
        catch (Exception e)
        {
            _logger.Error(e, "HTTP {Method} {Path} failed with exception", request.Method, request.Path);
            throw;
        }
        finally
        {
            stopwatch.Stop();

            var statusCode = context.Response.StatusCode;
            var level = statusCode >= 500
                ? LogEventLevel.Error
                : statusCode >= 400
                    ? LogEventLevel.Warning
                    : LogEventLevel.Information;

            _logger.Write(level, "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
                request.Method, request.Path, statusCode, stopwatch.ElapsedMilliseconds);
        }
    }
}
