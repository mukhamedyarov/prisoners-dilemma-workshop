using Microsoft.EntityFrameworkCore;
using PrisonersDilemma.Api.Application.Interfaces;
using PrisonersDilemma.Api.Data;
using PrisonersDilemma.Api.Domain.Entities;
using PrisonersDilemma.Api.Domain.Enums;

namespace PrisonersDilemma.Api.Infrastructure.Repositories;

public class GameSessionRepository : IGameSessionRepository
{
	private readonly GameDbContext _context;

	public GameSessionRepository(GameDbContext context)
	{
		_context = context;
	}

	public async Task<GameSession?> GetByIdAsync(Guid id)
	{
		return await _context.GameSessions
			.Include(gs => gs.Players)
				.ThenInclude(p => p.Choices)
			.Include(gs => gs.Rounds)
				.ThenInclude(r => r.Choices)
					.ThenInclude(c => c.Player)
			.FirstOrDefaultAsync(gs => gs.Id == id);
	}

	public async Task<GameSession?> GetLookingForPlayerSessionAsync()
	{
		return await _context.GameSessions
			.Include(gs => gs.Players)
			.Include(gs => gs.Rounds)
			.FirstOrDefaultAsync(gs => gs.Status == GameSessionStatus.LookingForPlayer);
	}

	public async Task<Player?> GetPlayerByIdAsync(Guid playerId)
	{
		return await _context.Players
			.FirstOrDefaultAsync(p => p.Id == playerId);
	}

	public async Task<GameSession> CreateAsync(GameSession gameSession)
	{
		_context.GameSessions.Add(gameSession);
		await _context.SaveChangesAsync();
		return gameSession;
	}

	public async Task UpdateAsync(GameSession gameSession)
	{
		_context.GameSessions.Update(gameSession);
		await _context.SaveChangesAsync();
	}

	public async Task<Round?> GetRoundAsync(Guid sessionId, int roundNumber)
	{
		return await _context.Rounds
			.Include(r => r.Choices)
				.ThenInclude(c => c.Player)
			.FirstOrDefaultAsync(r => r.GameSessionId == sessionId && r.Number == roundNumber);
	}
}