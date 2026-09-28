using Microsoft.AspNetCore.Diagnostics;   // IExceptionHandler
using Microsoft.AspNetCore.Mvc;           // ProblemDetails
using Microsoft.EntityFrameworkCore;      // DbUpdateException
using Microsoft.Data.SqlClient;           // SqlException

namespace SupportDesk.Api.Middleware;

public class GlobalExceptionHandler: IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> logger;
    private readonly IHostEnvironment env;
    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
    {
        this.logger = logger;
        this.env = env;
    }

    public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
    {
        var exceptionMessage = exception.Message;
        logger.LogError(exception, "Unhandled exception on path {Path}", httpContext.Request.Path);

        if (exception is DbUpdateException { InnerException: SqlException { Number: 2601 or 2627 } sqlEx })
        {
            httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Duplicate value",
                Detail = env.IsDevelopment() ? exceptionMessage + "\n" + sqlEx.Message : "A record with this value already exists.",
                Instance = httpContext.Request.Path
            };
            await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
            return true;
        }
    
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Detail = env.IsDevelopment() ? exceptionMessage : "Something went wrong.",
            Instance = httpContext.Request.Path 
        }, cancellationToken);
        return true;
    }
}