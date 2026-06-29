using Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Web.Middlewares
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            Log.Error(exception, "An unexpected error occurred during the request.");

            if (httpContext.Request.Headers.Accept.ToString().Contains("text/html"))
            {
                return false;
            }

            var isDomainException = exception is DomainException;
            var statusCode = isDomainException ? StatusCodes.Status400BadRequest : StatusCodes.Status500InternalServerError;

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = isDomainException ? "Domain Rule Violation" : "Internal Server Error",
                Detail = isDomainException ? exception.Message : "An unexpected fault happened. Try again later.",
                Instance = httpContext.Request.Path
            };

            problemDetails.Extensions.Add("traceId", httpContext.TraceIdentifier);

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}