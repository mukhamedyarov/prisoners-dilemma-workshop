using Microsoft.AspNetCore.Diagnostics;
using PrisonersDilemma.Api.Exceptions;
using System.Net;

namespace PrisonersDilemma.Api.Middleware;

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
		_logger.LogError(exception, "An unhandled exception occurred");

		var (statusCode, message) = MapException(exception);

		var problemDetails = new
		{
			title = GetTitle(statusCode),
			status = (int)statusCode,
			detail = message
		};

		httpContext.Response.StatusCode = (int)statusCode;
		httpContext.Response.ContentType = "application/json";

		await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

		return true;
	}

	private static (HttpStatusCode StatusCode, string Message) MapException(Exception exception)
	{
		return exception switch
		{
			MasterKeyValidationException e => (HttpStatusCode.Unauthorized, e.Message),
			GameNotFoundException e => (HttpStatusCode.NotFound, e.Message),
			RoundNotFoundException e => (HttpStatusCode.NotFound, e.Message),
			InvalidGameStateException e => (HttpStatusCode.BadRequest, e.Message),
			InvalidRequestException e => (HttpStatusCode.BadRequest, e.Message),
			AuthenticationException e => (HttpStatusCode.Unauthorized, e.Message),
			ConcurrencyConflictException e => (HttpStatusCode.Conflict, e.Message),
			ArgumentNullException e => (HttpStatusCode.BadRequest, e.Message),
			ArgumentException e => (HttpStatusCode.BadRequest, e.Message),
			_ => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
		};
	}

	private static string GetTitle(HttpStatusCode statusCode)
	{
		return statusCode switch
		{
			HttpStatusCode.BadRequest => "Bad Request",
			HttpStatusCode.Unauthorized => "Unauthorized",
			HttpStatusCode.NotFound => "Not Found",
			HttpStatusCode.Conflict => "Conflict",
			HttpStatusCode.InternalServerError => "Internal Server Error",
			_ => "Error"
		};
	}
}