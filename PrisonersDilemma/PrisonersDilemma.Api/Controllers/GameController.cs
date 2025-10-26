using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using PrisonersDilemma.Api.Application.DTOs;
using PrisonersDilemma.Api.Application.Interfaces;
using PrisonersDilemma.Api.Authorization;
using PrisonersDilemma.Api.Configuration;
using PrisonersDilemma.Api.Exceptions;

namespace PrisonersDilemma.Api.Controllers;

[ApiController]
[Route("api/game")]
[ConditionalAuthorize]
public class GameController : ControllerBase
{
	private readonly ApiKeySettings _apiKeySettings;
	private readonly IGameService _gameService;

	public GameController(IOptions<ApiKeySettings> apiKeySettings, IGameService gameService)
	{
		_apiKeySettings = apiKeySettings.Value;
		_gameService = gameService;
	}

	[HttpGet("{sessionId:guid}")]
	public async Task<ActionResult<GameInfoResponse>> GetGameInfo(Guid sessionId)
	{
		CheckMasterKey();

		try
		{
			var response = await _gameService.GetGameInfoAsync(sessionId);
			return Ok(response);
		}
		catch (ArgumentException ex)
		{
			return NotFound(new
			{
				message = ex.Message
			});
		}
		catch (Exception ex)
		{
			return StatusCode(500, new
			{
				message = ex.Message
			});
		}
	}

	[HttpGet("{sessionId:guid}/round/{roundNumber:int}")]
	public async Task<ActionResult<RoundInfoResponse>> GetRoundInfo(Guid sessionId, int roundNumber)
	{
		CheckMasterKey();

		try
		{
			var response = await _gameService.GetRoundInfoAsync(sessionId, roundNumber);
			return Ok(response);
		}
		catch (ArgumentException ex)
		{
			return NotFound(new
			{
				message = ex.Message
			});
		}
		catch (Exception ex)
		{
			return StatusCode(500, new
			{
				message = ex.Message
			});
		}
	}

	[HttpPost("start")]
	public async Task<ActionResult<StartGameResponse>> StartGame([FromBody] StartGameRequest request)
	{
		CheckMasterKey();

		try
		{
			var response = await _gameService.StartGameAsync(request);
			return Ok(response);
		}
		catch (Exception ex)
		{
			return BadRequest(new
			{
				message = ex.Message
			});
		}
	}

	[HttpPost("choice")]
	public async Task<ActionResult<RoundInfoResponse>> SubmitChoice([FromBody] SubmitChoiceRequest request)
	{
		CheckMasterKey();

		try
		{
			var response = await _gameService.SubmitChoiceAsync(request);
			return Ok(response);
		}
		catch (ArgumentException ex)
		{
			return BadRequest(new
			{
				message = ex.Message
			});
		}
		catch (InvalidOperationException ex)
		{
			return BadRequest(new
			{
				message = ex.Message
			});
		}
		catch (Exception ex)
		{
			return StatusCode(500, new
			{
				message = ex.Message
			});
		}
	}

	private void CheckMasterKey()
	{

		// Validate X-MasterKey header
		if (!Request.Headers.TryGetValue("X-MasterKey", out var masterKeyHeader) ||
		    string.IsNullOrEmpty(masterKeyHeader.FirstOrDefault()))
		{
			throw new MasterKeyValidationException("X-MasterKey header is required");
		}

		var providedKey = masterKeyHeader.First()!;
		if (providedKey != _apiKeySettings.MasterKey)
		{
			throw new MasterKeyValidationException("Invalid master key");
		}
	}
}