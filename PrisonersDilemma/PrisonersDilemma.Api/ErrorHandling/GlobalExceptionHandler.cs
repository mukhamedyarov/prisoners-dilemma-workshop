using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PrisonersDilemma.Api.Exceptions;
using System.Diagnostics;

namespace PrisonersDilemma.Api.ErrorHandling;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, 
        Exception exception, 
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        
        var problemDetails = exception switch
        {
            GameNotFoundException ex => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Game Not Found",
                Detail = ex.Message,
                Instance = httpContext.Request.Path,
                Extensions = { ["traceId"] = traceId, ["sessionId"] = ex.SessionId }
            },
            InvalidGameStateException ex => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid Game State",
                Detail = ex.Message,
                Instance = httpContext.Request.Path,
                Extensions = { ["traceId"] = traceId }
            },
            PlayerValidationException ex => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Player Validation Error",
                Detail = ex.Message,
                Instance = httpContext.Request.Path,
                Extensions = { ["traceId"] = traceId }
            },
            MasterKeyValidationException ex => new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = ex.Message,
                Instance = httpContext.Request.Path,
                Extensions = { ["traceId"] = traceId }
            },
            ArgumentException ex => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid Argument",
                Detail = ex.Message,
                Instance = httpContext.Request.Path,
                Extensions = { ["traceId"] = traceId }
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while processing your request.",
                Instance = httpContext.Request.Path,
                Extensions = { ["traceId"] = traceId }
            }
        };

        var logLevel = exception switch
        {
            GameNotFoundException => LogLevel.Warning,
            InvalidGameStateException => LogLevel.Warning,
            PlayerValidationException => LogLevel.Warning,
            MasterKeyValidationException => LogLevel.Warning,
            ArgumentException => LogLevel.Warning,
            _ => LogLevel.Error
        };

        _logger.Log(logLevel, exception, 
            "Exception occurred: {ExceptionType} - {Message} - TraceId: {TraceId}", 
            exception.GetType().Name, exception.Message, traceId);

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";
        
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}