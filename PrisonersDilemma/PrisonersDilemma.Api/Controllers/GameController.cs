using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PrisonersDilemma.Api.Authorization;
using PrisonersDilemma.Api.Configuration;
using PrisonersDilemma.Api.Exceptions;
using PrisonersDilemma.Api.Models;
using PrisonersDilemma.Api.Services;
using PrisonersDilemma.Api.Validation;

namespace PrisonersDilemma.Api.Controllers;

[ApiController]
[Route("api/games")]
[ConditionalAuthorize]
public class GameController : ControllerBase
{
    private readonly IGameService _gameService;
    private readonly ApiKeySettings _apiKeySettings;
    
    public GameController(IGameService gameService, IOptions<ApiKeySettings> apiKeySettings)
    {
        _gameService = gameService;
        _apiKeySettings = apiKeySettings.Value;
    }
    
    [HttpPost("")]
    public async Task<ActionResult<CreateGameResult>> CreateGame(CreateGameRequest request)
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

        var session = await _gameService.CreateGameAsync(request.Player1Name, request.Player2Name);
        return Ok(session);
    }
    
    [HttpGet("{sessionId}")]
    public async Task<ActionResult<GameSession>> GetGame(Guid sessionId)
    {
        var session = await _gameService.GetGameAsync(sessionId);
        if (session == null)
        {
            return NotFound();
        }
        
        return Ok(session);
    }
    
    [HttpGet("{sessionId}/status")]
    public async Task<ActionResult<GameStatus>> GetGameStatus(Guid sessionId)
    {
        var status = await _gameService.GetGameStatusAsync(sessionId);
        return Ok(status);
    }
    
    [HttpPost("{sessionId}/choice")]
    public async Task<ActionResult<object>> SetPlayerChoice([NotEmptyGuid] Guid sessionId, [FromBody] SetPlayerChoiceRequest request)
    {
        var result = await _gameService.SetPlayerChoiceAsync(sessionId, request.PlayerId, request.Choice);
        
        if (result != null)
        {
            return Ok(new { RoundComplete = true, Result = result });
        }
        
        var status = await _gameService.GetGameStatusAsync(sessionId);
        return Ok(new { RoundComplete = false, Status = status });
    }
    
    [HttpGet("")]
    public async Task<ActionResult<IEnumerable<GameSession>>> GetAllGames()
    {
        var games = await _gameService.GetAllGamesAsync();
        return Ok(games);
    }
    
    [HttpGet("{sessionId}/summary")]
    public async Task<ActionResult<object>> GetGameSummary(Guid sessionId)
    {
        var session = await _gameService.GetGameAsync(sessionId);
        if (session == null)
        {
            return NotFound();
        }
        
        var totalPlayer1Score = session.Results.Sum(r => r.Player1Score);
        var totalPlayer2Score = session.Results.Sum(r => r.Player2Score);
        var roundsPlayed = session.Results.Count;
        
        var summary = new
        {
            SessionId = session.SessionId,
            Player1Name = session.Player1Name,
            Player2Name = session.Player2Name,
            RoundsPlayed = roundsPlayed,
            TotalPlayer1Score = totalPlayer1Score,
            TotalPlayer2Score = totalPlayer2Score,
            Winner = totalPlayer1Score > totalPlayer2Score ? session.Player1Name :
                     totalPlayer2Score > totalPlayer1Score ? session.Player2Name : "Tie",
            Results = session.Results
        };
        
        return Ok(summary);
    }
}