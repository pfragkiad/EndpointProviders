using CsvReaderAdvanced;
using CsvReaderAdvanced.Files;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace EndpointProviders;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _logger = logger;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong: {ex}");
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        List<ValidationFailure> failures = new List<ValidationFailure>();
        if (exception.InnerException is not null)
            failures.Add(new ValidationFailure("application/json", exception.InnerException.Message));


        failures.Add(new ValidationFailure("application/json", exception.Message));

        ReaderReport readerReport = new ReaderReport()
        {
            Validation = new ValidationResult() { Errors = failures }
        };

        await context.Response.WriteAsJsonAsync(readerReport);
    }
}

public static class ExceptionMiddlewareExtensions
{
    public static WebApplication AddExceptionMiddleware(this WebApplication app)
    {
        app.UseMiddleware<ExceptionMiddleware>();
        return app;
    }
}
