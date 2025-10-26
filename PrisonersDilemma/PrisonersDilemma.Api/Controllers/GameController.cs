using Microsoft.AspNetCore.Mvc;

using PrisonersDilemma.Api.Application.DTOs;
using PrisonersDilemma.Api.Application.Interfaces;
using PrisonersDilemma.Api.Authorization;

namespace PrisonersDilemma.Api.Controllers;

[ApiController]
[Route("api/game")]
[ConditionalAuthorize]
public class GameController : ControllerBase
{
	private readonly IGameService _gameService;

	public GameController(IGameService gameService)
	{
		_gameService = gameService;
	}

	[HttpGet("{sessionId:guid}")]
	public async Task<ActionResult<GameInfoResponse>> GetGameInfo(Guid sessionId)
	{
		var response = await _gameService.GetGameInfoAsync(sessionId);
		return Ok(response);
	}

	[HttpGet("{sessionId:guid}/round/{roundNumber:int}")]
	public async Task<ActionResult<RoundInfoResponse>> GetRoundInfo(Guid sessionId, int roundNumber)
	{
		var response = await _gameService.GetRoundInfoAsync(sessionId, roundNumber);
		return Ok(response);
	}

	[HttpPost("start")]
	public async Task<ActionResult<StartGameResponse>> StartGame([FromBody] StartGameRequest request)
	{
		var response = await _gameService.StartGameAsync(request);
		return Ok(response);
	}

	[HttpPost("choice")]
	public async Task<ActionResult<RoundInfoResponse>> SubmitChoice([FromBody] SubmitChoiceRequest request)
	{
		var response = await _gameService.SubmitChoiceAsync(request);
		return Ok(response);
	}
}