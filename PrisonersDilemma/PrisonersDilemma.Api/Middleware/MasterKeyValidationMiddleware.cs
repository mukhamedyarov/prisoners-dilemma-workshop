using Microsoft.Extensions.Options;
using PrisonersDilemma.Api.Configuration;
using PrisonersDilemma.Api.Exceptions;

namespace PrisonersDilemma.Api.Middleware;

public class MasterKeyValidationMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ApiKeySettings _apiKeySettings;

	public MasterKeyValidationMiddleware(RequestDelegate next, IOptions<ApiKeySettings> apiKeySettings)
	{
		_next = next;
		_apiKeySettings = apiKeySettings.Value;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		if (!context.Request.Headers.TryGetValue("X-MasterKey", out var masterKeyHeader) ||
		    string.IsNullOrEmpty(masterKeyHeader.FirstOrDefault()))
		{
			throw new MasterKeyValidationException("X-MasterKey header is required");
		}

		var providedKey = masterKeyHeader.First()!;
		if (providedKey != _apiKeySettings.MasterKey)
		{
			throw new MasterKeyValidationException("Invalid master key");
		}

		await _next(context);
	}
}