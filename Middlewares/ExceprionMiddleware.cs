using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace WebLibraryApi.Middlewares;

public class ExceptionHandler(ILogger<ExceptionHandler> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (SqliteException ex)
        {
            logger.LogError(ex, "Database error");
            context.Response.StatusCode = 503;
            await WriteProblemDetails(context, "Service Unavailable", "Database service unavailable");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = 500;
            await WriteProblemDetails(context, "Error", "Internal Error");
        }
    }

    private static async Task WriteProblemDetails(HttpContext context, string title, string detail)
    {
        int statusCode = context.Response.StatusCode;
        var problem = new ProblemDetails
        {
            Title = title,
            Status = statusCode,
            Detail = detail,
            Instance = context.Request.Path
        };

        await context.Response.WriteAsJsonAsync(problem);
    }
}

public static class ExceptionHandlerExtension
{
    public static IApplicationBuilder UseExceptionHandlerMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandler>();
    }
}