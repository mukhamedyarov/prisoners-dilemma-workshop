using System.Data;

using Microsoft.EntityFrameworkCore;

using PrisonersDilemma.Api.Application.Interfaces;
using PrisonersDilemma.Api.Data;
using PrisonersDilemma.Api.Domain.Entities;
using PrisonersDilemma.Api.Domain.Enums;
using PrisonersDilemma.Api.Exceptions;

namespace PrisonersDilemma.Api.Infrastructure.Repositories;

public class GameSessionRepository : IGameSessionRepository
{
	private readonly GameDbContext _context;

	public GameSessionRepository(GameDbContext context)
	{
		_context = context;
	}

	public async Task<GameSession> CreateAsync(GameSession gameSession)
	{
		_context.GameSessions.Add(gameSession);
		await _context.SaveChangesAsync();
		return gameSession;
	}
	
	public Round AddRound(Round round)
	{
		_context.Rounds.Add(round);
		return round;
	}

	public PlayerChoice AddPlayerChoice(PlayerChoice playerChoice)
	{
		_context.PlayerChoices.Add(playerChoice);
		return playerChoice;
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


	public async Task<Player?> GetPlayerByIdAsync(Guid playerId)
	{
		return await _context.Players
			.FirstOrDefaultAsync(p => p.Id == playerId);
	}

	public async Task<Round?> GetRoundAsync(Guid sessionId, int roundNumber)
	{
		return await _context.Rounds
			.Include(r => r.Choices)
			.ThenInclude(c => c.Player)
			.FirstOrDefaultAsync(r => r.GameSessionId == sessionId && r.Number == roundNumber);
	}

	public async Task<(GameSession session, Player player)> StartGameAtomicAsync(Guid playerId, string playerName, int maxRounds)
	{
		await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
		try
		{
			var existingSession = await _context.GameSessions
				.Include(gs => gs.Players)
				.Include(gs => gs.Rounds)
				.FirstOrDefaultAsync(gs => gs.Status == GameSessionStatus.LookingForPlayer);

			if (existingSession != null)
			{
				var player2 = new Player
				{
					Id = playerId,
					Name = playerName,
					Score = 0,
					GameSessionId = existingSession.Id
				};

				_context.Players.Add(player2);

				existingSession.Players.Add(player2);
				existingSession.Status = GameSessionStatus.Active;
				existingSession.CurrentRound = 1;

				var firstRound = new Round
				{
					Id = Guid.NewGuid(),
					Number = 1,
					Status = RoundStatus.InProgress,
					GameSessionId = existingSession.Id,
					CreatedAt = DateTime.UtcNow
				};
				_context.Rounds.Add(firstRound);
				
				existingSession.Rounds.Add(firstRound);

				await _context.SaveChangesAsync();
				await transaction.CommitAsync();
				return (existingSession, player2);
			}

			var newSession = new GameSession
			{
				Id = Guid.NewGuid(),
				Status = GameSessionStatus.LookingForPlayer,
				CurrentRound = 0,
				MaxRounds = maxRounds,
				CreatedAt = DateTime.UtcNow
			};

			var player1 = new Player
			{
				Id = playerId,
				Name = playerName,
				Score = 0,
				GameSessionId = newSession.Id
			};

			newSession.Players.Add(player1);
			_context.GameSessions.Add(newSession);

			await _context.SaveChangesAsync();
			await transaction.CommitAsync();
			return (newSession, player1);
		}
		catch (Exception ex) when (IsConcurrencyException(ex))
		{
			await transaction.RollbackAsync();
			throw new ConcurrencyConflictException("Concurrent operation detected. Please retry.", ex);
		}
		catch
		{
			await transaction.RollbackAsync();
			throw;
		}
	}

	public async Task UpdateAsync(GameSession gameSession)
	{
		_context.GameSessions.Update(gameSession);
		await _context.SaveChangesAsync();
	}

	private static bool IsConcurrencyException(Exception ex) => ex is DbUpdateConcurrencyException ||
		ex is DbUpdateException && ex.InnerException?.GetType().Name.Contains("Serializable") == true ||
		ex.InnerException?.Message?.Contains("serializable") == true ||
		ex.InnerException?.Message?.Contains("deadlock") == true ||
		ex.Message?.Contains("serializable") == true ||
		ex.Message?.Contains("deadlock") == true;
}