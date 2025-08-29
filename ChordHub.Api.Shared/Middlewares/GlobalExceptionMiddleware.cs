using System.Net;
using System.Text.Json;

using ChordHub.Api.Shared.Models;
using ChordHub.Core.Exceptions;

using Microsoft.AspNetCore.Http;

using Serilog;

namespace ChordHub.Api.Shared.Middlewares;

public class GlobalExceptionMiddleware(RequestDelegate next)
{
    private readonly ILogger _logger = Log.ForContext<GlobalExceptionMiddleware>();

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception e)
        {
            await HandleExceptionAsync(context, e);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message, shouldLogAsError) = GetErrorDetails(exception);
        context.Response.StatusCode = statusCode;

        var response = new ApiResponse<object>
        {
            Success = false,
            Message = message,
            Errors = [exception.Message]
        };

        if (shouldLogAsError)
            _logger.Error(exception,
                "Unhandled exception occurred. StatusCode: {StatusCode}, RequestPath: {RequestPath}",
                statusCode, context.Request.Path);
        else
            _logger.Warning(exception,
                "Expected exception occurred. StatusCode: {StatusCode}, RequestPath: {RequestPath}",
                statusCode, context.Request.Path);

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private static (int statusCode, string message, bool shouldLogAsError) GetErrorDetails(Exception exception)
    {
        return exception switch
        {
            ValidationException => ((int)HttpStatusCode.BadRequest, "Wrong input data", false),
            UnauthorizedException => ((int)HttpStatusCode.Unauthorized, "Need authorization", false),
            NotFoundException => ((int)HttpStatusCode.NotFound, "Can't find resources", false),
            BusinessRuleException => ((int)HttpStatusCode.BadRequest, "Violate business rules", false),
            ArgumentException => ((int)HttpStatusCode.BadRequest, "Wrong request", false),
            NotImplementedException => ((int)HttpStatusCode.NotImplemented, "Not implemented feature", false),
            InvalidOperationException => ((int)HttpStatusCode.BadRequest, "Wrong works", false),
            _ => ((int)HttpStatusCode.InternalServerError, "Internal server error", true)
        };
    }
}
