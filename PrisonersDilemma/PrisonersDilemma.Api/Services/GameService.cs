using Microsoft.EntityFrameworkCore;
using PrisonersDilemma.Api.Data;
using PrisonersDilemma.Api.Data.Entities;
using PrisonersDilemma.Api.Exceptions;
using PrisonersDilemma.Api.Models;

namespace PrisonersDilemma.Api.Services;

public interface IGameService
{
    Task<CreateGameResult> CreateGameAsync(string player1Name, string player2Name);
    Task<GameSession?> GetGameAsync(Guid sessionId);
    Task<GameResult?> SetPlayerChoiceAsync(Guid sessionId, Guid playerId, Choice choice);
    Task<GameStatus> GetGameStatusAsync(Guid sessionId);
    Task<IEnumerable<GameSession>> GetAllGamesAsync();
}

public class GameService : IGameService
{
    private const int MaxRounds = 157;
    private readonly GameDbContext _dbContext;
    
    public GameService(GameDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<CreateGameResult> CreateGameAsync(string player1Name, string player2Name)
    {
        if (string.IsNullOrWhiteSpace(player1Name))
            throw new PlayerValidationException("Player 1 name cannot be empty");
        
        if (string.IsNullOrWhiteSpace(player2Name))
            throw new PlayerValidationException("Player 2 name cannot be empty");
        
        if (player1Name.Length > 100)
            throw new PlayerValidationException("Player 1 name cannot exceed 100 characters");
        
        if (player2Name.Length > 100)
            throw new PlayerValidationException("Player 2 name cannot exceed 100 characters");
        
        var entity = new GameSessionEntity
        {
            Id = Guid.NewGuid(),
            Player1Id = Guid.NewGuid(),
            Player1Name = player1Name.Trim(),
            Player2Id = Guid.NewGuid(),
            Player2Name = player2Name.Trim(),
            CreatedAt = DateTime.UtcNow,
            PendingPlayer1Choice = null,
            PendingPlayer2Choice = null
        };
        
        _dbContext.GameSessions.Add(entity);
        await _dbContext.SaveChangesAsync();

        return new CreateGameResult(
            entity.Id, 
            new Player(entity.Player1Id, entity.Player1Name),
            new Player(entity.Player2Id, entity.Player2Name));
    }
    
    public async Task<GameSession?> GetGameAsync(Guid sessionId)
    {
        if (sessionId == Guid.Empty)
            throw new ArgumentException("Session ID cannot be empty", nameof(sessionId));
        
        var entity = await _dbContext.GameSessions
            .Include(s => s.Results.OrderBy(r => r.CreatedAt))
            .FirstOrDefaultAsync(s => s.Id == sessionId);
        
        return entity == null ? null : MapToGameSession(entity);
    }

    public async Task<GameResult?> SetPlayerChoiceAsync(Guid sessionId, Guid playerId, Choice choice)
    {
        if (sessionId == Guid.Empty)
            throw new ArgumentException("Session ID cannot be empty", nameof(sessionId));
        
        if (playerId == Guid.Empty)
            throw new ArgumentException("Player ID cannot be empty", nameof(playerId));
        
        var session = await _dbContext.GameSessions
            .Include(s => s.Results)
            .FirstOrDefaultAsync(s => s.Id == sessionId);
        if (session == null)
            throw new GameNotFoundException(sessionId);
        
        var currentRound = session.Results.Count + 1;
        if (currentRound > MaxRounds)
            throw new InvalidGameStateException($"Maximum number of rounds ({MaxRounds}) has been reached for this game session");
        
        if (playerId == session.Player1Id)
        {
            if (session.PendingPlayer1Choice.HasValue)
                throw new InvalidGameStateException("Player 1 has already made a choice for this round");
            session.PendingPlayer1Choice = choice;
        }
        else if (playerId == session.Player2Id)
        {
            if (session.PendingPlayer2Choice.HasValue)
                throw new InvalidGameStateException("Player 2 has already made a choice for this round");
            session.PendingPlayer2Choice = choice;
        }
        else
        {
            throw new PlayerValidationException("Player ID does not match any player in this game session");
        }
        
        if (session.PendingPlayer1Choice.HasValue && session.PendingPlayer2Choice.HasValue)
        {
            var result = CalculateResult(session.PendingPlayer1Choice.Value, session.PendingPlayer2Choice.Value);
            
            var resultEntity = new GameResultEntity
            {
                GameSessionId = sessionId,
                Player1Score = result.Player1Score,
                Player2Score = result.Player2Score,
                Player1Choice = result.Player1Choice,
                Player2Choice = result.Player2Choice,
                Outcome = result.Outcome,
                CreatedAt = DateTime.UtcNow
            };
            
            session.PendingPlayer1Choice = null;
            session.PendingPlayer2Choice = null;
            
            _dbContext.GameResults.Add(resultEntity);
            await _dbContext.SaveChangesAsync();
            
            return result;
        }
        
        await _dbContext.SaveChangesAsync();
        return null;
    }
    
    public async Task<GameStatus> GetGameStatusAsync(Guid sessionId)
    {
        if (sessionId == Guid.Empty)
            throw new ArgumentException("Session ID cannot be empty", nameof(sessionId));
        
        var session = await _dbContext.GameSessions
            .Include(s => s.Results)
            .FirstOrDefaultAsync(s => s.Id == sessionId);
        
        if (session == null)
            throw new GameNotFoundException(sessionId);
        
        var currentRound = session.Results.Count + 1;
        return new GameStatus(
            session.Id,
            session.Player1Name,
            session.Player2Name,
            session.Results.Count,
            currentRound,
            !session.PendingPlayer1Choice.HasValue,
            !session.PendingPlayer2Choice.HasValue,
            session.PendingPlayer1Choice.HasValue && session.PendingPlayer2Choice.HasValue);
    }
    
    public async Task<IEnumerable<GameSession>> GetAllGamesAsync()
    {
        var entities = await _dbContext.GameSessions
            .Include(s => s.Results.OrderBy(r => r.CreatedAt))
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
        
        return entities.Select(MapToGameSession);
    }
    
    private static GameResult CalculateResult(Choice player1Choice, Choice player2Choice)
    {
        var (player1Score, player2Score, outcome) = (player1Choice, player2Choice) switch
        {
            (Choice.Cooperate, Choice.Cooperate) => (3, 3, "Both cooperated - mutual benefit"),
            (Choice.Cooperate, Choice.Defect) => (0, 5, "Player 1 cooperated, Player 2 defected - Player 2 wins"),
            (Choice.Defect, Choice.Cooperate) => (5, 0, "Player 1 defected, Player 2 cooperated - Player 1 wins"),
            (Choice.Defect, Choice.Defect) => (1, 1, "Both defected - mutual punishment"),
            _ => throw new InvalidOperationException("Invalid choice combination")
        };
        
        return new GameResult(player1Score, player2Score, player1Choice, player2Choice, outcome);
    }
    
    private static GameSession MapToGameSession(GameSessionEntity entity)
    {
        var roundNumber = entity.Results.Count + 1;
        var pendingRound = entity.PendingPlayer1Choice.HasValue || entity.PendingPlayer2Choice.HasValue
            ? new PendingRound(roundNumber, entity.PendingPlayer1Choice.HasValue && entity.PendingPlayer2Choice.HasValue)
            : null;
        
        var results = entity.Results.Select(r => new GameResult(
            r.Player1Score,
            r.Player2Score,
            r.Player1Choice,
            r.Player2Choice,
            r.Outcome)).ToList();
        
        return new GameSession(
            entity.Id,
            entity.Player1Name,
            entity.Player2Name,
            results,
            pendingRound,
            entity.CreatedAt);
    }
}