using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
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
        _logger.LogError(context.Exception, "Unhandled exception occurred");

        var statusCode = context.Exception is ArgumentException || context.Exception is InvalidOperationException
            ? StatusCodes.Status400BadRequest
            : StatusCodes.Status500InternalServerError;

        var message = _env.IsDevelopment()
            ? context.Exception.ToString()
            : "An unexpected error occurred. Please try again later.";

        context.Result = new JsonResult(ApiResponse.Fail(message, statusCode))
        {
            StatusCode = statusCode
        };

        context.ExceptionHandled = true;
    }
}
