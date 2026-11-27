using System.Net;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Api.Exceptions;

namespace TaskManagement.Api.Middleware;

/// <summary>
/// Catches unhandled exceptions, logs them and returns an RFC 7807 ProblemDetails body.
/// Known <see cref="AppException"/>s map to their own status code; anything else is a 500
/// whose details are only shown in Development.
/// </summary>
public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            logger.LogError(exception, "Unhandled exception after the response started; cannot write error body");
            throw exception;
        }

        var (status, title, detail) = exception switch
        {
            AppException appEx => (appEx.StatusCode, appEx.StatusCode.ToString(), appEx.Message),
            _ => (HttpStatusCode.InternalServerError,
                  "An unexpected error occurred.",
                  environment.IsDevelopment() ? exception.ToString() : null)
        };

        if (status == HttpStatusCode.InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
        }
        else
        {
            logger.LogWarning("Request {Method} {Path} failed with {StatusCode}: {Message}",
                context.Request.Method, context.Request.Path, (int)status, exception.Message);
        }

        var problem = new ProblemDetails
        {
            Status = (int)status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };
        problem.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.Clear();
        context.Response.StatusCode = (int)status;
        await context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json");
    }
}
