using PrisonersDilemma.Api.Domain.Entities;
using PrisonersDilemma.Api.Domain.Enums;

namespace PrisonersDilemma.Api.Application.Interfaces;

public interface IGameSessionRepository
{
	Task<GameSession?> GetByIdAsync(Guid id);
	Task<Player?> GetPlayerByIdAsync(Guid playerId);
	Task<GameSession> CreateAsync(GameSession gameSession);
	Task UpdateAsync(GameSession gameSession);
	Task<Round?> GetRoundAsync(Guid sessionId, int roundNumber);
	Task<(GameSession session, Player player)> StartGameAtomicAsync(Guid playerId, string playerName, int maxRounds);
}