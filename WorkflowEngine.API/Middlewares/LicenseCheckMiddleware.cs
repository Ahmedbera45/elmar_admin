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
        // Skip license check for Swagger UI or other non-business endpoints if necessary
        // But the requirement says "Her istekte lisansı kontrol etsin" (Check on every request).
        // We might want to skip basic static files or heartbeat, but let's stick to strict requirement for now
        // or just allow Swagger for dev experience?
        // Let's enforce it strictly as requested.

        if (!licenseValidator.Validate())
        {
            context.Response.StatusCode = 402; // Payment Required
            await context.Response.WriteAsync("License validation failed. Please contact support.");
            return;
        }

        await _next(context);
    }
}
