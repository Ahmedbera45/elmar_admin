using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using WorkflowEngine.Core.Interfaces;

namespace WorkflowEngine.API.Middlewares;

public class LicenseCheckMiddleware
{
    private readonly RequestDelegate _next;

    public LicenseCheckMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ILicenseValidator licenseValidator)
    {
        if (!licenseValidator.Validate())
        {
            context.Response.StatusCode = 402; // Payment Required
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                Status = 402,
                Error = "License validation failed. Please contact support.",
                MachineId = "Hidden" // Optional: could expose for debugging
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
            return;
        }

        await _next(context);
    }
}
