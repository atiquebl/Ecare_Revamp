using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using ECare_Revamp.Models;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly GlobalActivityLogger _logger;

    public RequestLoggingMiddleware(RequestDelegate next, GlobalActivityLogger logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.ToString().ToLower();

        // Skip static file requests by extension
        string[] staticExtensions = new[] { ".css", ".js", ".png", ".jpg", ".jpeg", ".gif", ".woff", ".woff2", ".ttf", ".svg", ".ico", ".map" };
        if (staticExtensions.Any(ext => path.EndsWith(ext)))
        {
            await _next(context);
            return;
        }

        var userName = context.User.Identity?.IsAuthenticated == true
            ? context.User.Identity.Name
            : "Anonymous";

        var method = context.Request.Method;
        var ip = context.Connection.RemoteIpAddress?.ToString();

        string details = $"Request {method} {path} from {ip}";

        await _logger.LogAsync(userName ?? "Anonymous", "REQUEST", details);

        await _next(context);
    }

}
