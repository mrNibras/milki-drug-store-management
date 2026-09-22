using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MilkiDrugStore.Api.Models;

namespace MilkiDrugStore.Api.Middleware;

public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;
    private readonly IWebHostEnvironment _env;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public void OnException(ExceptionContext context)
    {
        var correlationId = context.HttpContext?.TraceIdentifier ?? "unknown";

        var innerException = context.Exception.InnerException ?? context.Exception;

        var isClientError = context.Exception is ArgumentException ||
                            context.Exception is InvalidOperationException ||
                            context.Exception is Microsoft.EntityFrameworkCore.DbUpdateException &&
                            (innerException is ArgumentException || innerException is InvalidOperationException);

        var statusCode = isClientError
            ? StatusCodes.Status400BadRequest
            : StatusCodes.Status500InternalServerError;

        // Always log the full exception server-side for diagnostics.
        _logger.LogError(context.Exception,
            "Unhandled exception (CorrelationId: {CorrelationId}, Status: {StatusCode})",
            correlationId, statusCode);

        // Determine the safe user-facing message.
        string message;
        if (_env.IsDevelopment())
        {
            message = context.Exception.ToString();
        }
        else if (isClientError)
        {
            var clientException = innerException is ArgumentException or InvalidOperationException
                ? innerException
                : context.Exception;
            message = clientException.Message;
        }
        else
        {
            // Server errors (including DbUpdateException from FK violations,
            // connection issues, etc.) are masked to avoid leaking infrastructure
            // details such as table names, constraint names, or SQL.
            message = "An unexpected error occurred. Please try again later.";
        }

        var response = ApiResponse.Fail(message, statusCode);
        response.CorrelationId = correlationId;

        context.Result = new JsonResult(response)
        {
            StatusCode = statusCode
        };

        context.ExceptionHandled = true;
    }
}
