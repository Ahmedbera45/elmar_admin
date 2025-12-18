using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace WorkflowEngine.API.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var statusCode = (int)HttpStatusCode.InternalServerError;
        var message = "An internal server error occurred.";

        if (exception is UnauthorizedAccessException)
        {
            statusCode = (int)HttpStatusCode.Unauthorized;
            message = exception.Message;
        }
        else if (exception is ArgumentException || exception is System.ComponentModel.DataAnnotations.ValidationException)
        {
            statusCode = (int)HttpStatusCode.BadRequest;
            message = exception.Message;
        }
        else if (exception.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)) // Heuristic
        {
             // statusCode = (int)HttpStatusCode.NotFound; // Optional
             // keeping 500 or 400 depending on taste, but let's stick to safe defaults.
             // If manual "throw new Exception('Not found')" was used.
             if (exception.Message.StartsWith("Process Request not found") || exception.Message.StartsWith("Process not found"))
             {
                 statusCode = (int)HttpStatusCode.NotFound;
                 message = exception.Message;
             }
             else
             {
                 message = exception.Message; // Pass through safe messages
             }
        }
        else
        {
            // For generic exceptions, we might want to expose message if it's "safe" business logic exception
            // Assuming most thrown exceptions are business logic in this context
            message = exception.Message;
        }

        context.Response.StatusCode = statusCode;

        var response = new
        {
            error = true,
            message = message
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
