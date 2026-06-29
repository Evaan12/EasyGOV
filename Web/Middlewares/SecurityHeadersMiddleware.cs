using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Web.Middlewares
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey("X-Frame-Options"))
                    context.Response.Headers.Append("X-Frame-Options", "DENY");
                
                if (!context.Response.Headers.ContainsKey("X-Content-Type-Options"))
                    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

                if (!context.Response.Headers.ContainsKey("X-XSS-Protection"))
                    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

                if (!context.Response.Headers.ContainsKey("Strict-Transport-Security"))
                    context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");

                return Task.CompletedTask;
            });

            await _next(context);
        }
    }
}