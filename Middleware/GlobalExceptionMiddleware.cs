using System.Net;
using System.Text.Json;

namespace SecureEmployeeManagement.Middleware;

/// <summary>
/// Catches every unhandled exception in the pipeline.
/// SECURITY (OWASP A05 - Security Misconfiguration):
/// full exception detail is LOGGED SERVER-SIDE, but the client only ever
/// receives a generic message. No stack traces, no table names, no
/// framework versions - nothing that helps an attacker map the system.
/// </summary>
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
            await _next(context);   // pass the request down the pipeline
        }
        catch (Exception ex)
        {
            // Server-side: full detail for debugging.
            // NOTE: never log request bodies here - they may contain
            // passwords or tokens once we add auth in Phase 6.
            _logger.LogError(ex, "Unhandled exception on {Method} {Path}",
                context.Request.Method, context.Request.Path);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // Client-side: deliberately vague.
            var response = new
            {
                status = 500,
                message = "An unexpected error occurred. Please try again later."
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}