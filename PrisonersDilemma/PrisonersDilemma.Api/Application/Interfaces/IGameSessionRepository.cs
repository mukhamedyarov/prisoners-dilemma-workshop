using PrisonersDilemma.Api.Domain.Entities;

namespace PrisonersDilemma.Api.Application.Interfaces;

public interface IGameSessionRepository
{
	Task<GameSession> CreateAsync(GameSession gameSession);
	Task<GameSession?> GetByIdAsync(Guid id);
	Task<Player?> GetPlayerByIdAsync(Guid playerId);
	Task<Round?> GetRoundAsync(Guid sessionId, int roundNumber);
	Task<(GameSession session, Player player)> StartGameAtomicAsync(Guid playerId, string playerName, int maxRounds);
	Task UpdateAsync(GameSession gameSession);
}