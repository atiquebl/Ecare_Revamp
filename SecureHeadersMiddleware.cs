namespace ECare_Revamp.Models
{
    public class SecureHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecureHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext context)
        {
            var headers = context.Response.Headers;

            headers["X-Frame-Options"] = "SAMEORIGIN";
            headers["X-Content-Type-Options"] = "nosniff";
            headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";
            headers["Content-Security-Policy-Report-Only"] = "default-src 'self'; script-src 'self'; style-src 'self'";

            headers.Remove("X-Powered-By");
            headers.Remove("Server");
            headers.Remove("User-Agent");

            await _next(context);
        }
    }

}
