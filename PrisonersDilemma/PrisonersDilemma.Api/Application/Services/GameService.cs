using Microsoft.Extensions.Options;
using PrisonersDilemma.Api.Application.DTOs;
using PrisonersDilemma.Api.Application.Interfaces;
using PrisonersDilemma.Api.Configuration;
using PrisonersDilemma.Api.Domain.Entities;
using PrisonersDilemma.Api.Domain.Enums;
using PrisonersDilemma.Api.Domain.Services;

namespace PrisonersDilemma.Api.Application.Services;

public class GameService : IGameService
{
	private readonly IGameSessionRepository _gameSessionRepository;
	private readonly int _maxRounds;

	public GameService(IGameSessionRepository gameSessionRepository, IConfiguration configuration)
	{
		_gameSessionRepository = gameSessionRepository;
		_maxRounds = int.Parse(configuration.GetValue<string>("GAME_ROUNDS_COUNT") ?? "10");
	}

	public async Task<StartGameResponse> StartGameAsync(StartGameRequest request)
	{
		var existingSession = await _gameSessionRepository.GetLookingForPlayerSessionAsync();
		
		if (existingSession != null)
		{
			var player2 = new Player
			{
				Id = Guid.NewGuid(),
				Name = request.PlayerName,
				Score = 0,
				GameSessionId = existingSession.Id
			};
			
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
			existingSession.Rounds.Add(firstRound);
			
			await _gameSessionRepository.UpdateAsync(existingSession);
			
			return new StartGameResponse
			{
				SessionId = existingSession.Id,
				PlayerId = player2.Id,
				PlayerName = player2.Name
			};
		}
		
		var newSession = new GameSession
		{
			Id = Guid.NewGuid(),
			Status = GameSessionStatus.LookingForPlayer,
			CurrentRound = 0,
			MaxRounds = _maxRounds,
			CreatedAt = DateTime.UtcNow
		};
		
		var player1 = new Player
		{
			Id = Guid.NewGuid(),
			Name = request.PlayerName,
			Score = 0,
			GameSessionId = newSession.Id
		};
		
		newSession.Players.Add(player1);
		
		await _gameSessionRepository.CreateAsync(newSession);
		
		return new StartGameResponse
		{
			SessionId = newSession.Id,
			PlayerId = player1.Id,
			PlayerName = player1.Name
		};
	}

	public async Task<RoundInfoResponse> SubmitChoiceAsync(SubmitChoiceRequest request)
	{
		var session = await _gameSessionRepository.GetByIdAsync(request.SessionId);
		if (session == null)
			throw new ArgumentException("Game session not found");

		if (session.Status != GameSessionStatus.Active)
			throw new InvalidOperationException("Game session is not active");

		var player = session.Players.FirstOrDefault(p => p.Id == request.PlayerId);
		if (player == null)
			throw new ArgumentException("Player not found in this session");

		if (request.RoundNumber != session.CurrentRound)
			throw new ArgumentException("Invalid round number");

		var round = session.Rounds.FirstOrDefault(r => r.Number == request.RoundNumber);
		if (round == null)
		{
			round = new Round
			{
				Id = Guid.NewGuid(),
				Number = request.RoundNumber,
				Status = RoundStatus.InProgress,
				GameSessionId = session.Id,
				CreatedAt = DateTime.UtcNow
			};
			session.Rounds.Add(round);
		}

		if (round.Choices.Any(c => c.PlayerId == request.PlayerId))
			throw new InvalidOperationException("Player has already made a choice for this round");

		if (!Enum.TryParse<Choice>(request.Choice, true, out var choice))
			throw new ArgumentException("Invalid choice");

		var playerChoice = new PlayerChoice
		{
			Id = Guid.NewGuid(),
			PlayerId = request.PlayerId,
			RoundId = round.Id,
			Choice = choice,
			CreatedAt = DateTime.UtcNow
		};

		round.Choices.Add(playerChoice);
		player.Choices.Add(playerChoice);

		if (round.Choices.Count == 2)
		{
			GameLogicService.ProcessRoundCompletion(round, session);
		}

		await _gameSessionRepository.UpdateAsync(session);

		var response = new RoundInfoResponse
		{
			SessionId = session.Id,
			RoundNumber = round.Number,
			Status = round.Status.ToString()
		};

		if (round.Status == RoundStatus.Completed)
		{
			response.Outcome = new Dictionary<string, string>();
			response.Summary = new Dictionary<string, int>();
			
			foreach (var c in round.Choices)
			{
				response.Outcome[c.Player.Name] = c.Choice.ToString();
			}
			
			foreach (var p in session.Players)
			{
				response.Summary[p.Name] = p.Score;
			}
		}

		return response;
	}

	public async Task<GameInfoResponse> GetGameInfoAsync(Guid sessionId)
	{
		var session = await _gameSessionRepository.GetByIdAsync(sessionId);
		if (session == null)
			throw new ArgumentException("Game session not found");

		var response = new GameInfoResponse
		{
			SessionId = session.Id,
			Status = session.Status.ToString(),
			CurrentRound = session.CurrentRound
		};

		if (session.Players.Count > 0)
		{
			response.Player1Name = session.Players[0].Name;
			response.Summary[session.Players[0].Name] = session.Players[0].Score;
		}

		if (session.Players.Count > 1)
		{
			response.Player2Name = session.Players[1].Name;
			response.Summary[session.Players[1].Name] = session.Players[1].Score;
		}

		return response;
	}

	public async Task<RoundInfoResponse> GetRoundInfoAsync(Guid sessionId, int roundNumber)
	{
		var session = await _gameSessionRepository.GetByIdAsync(sessionId);
		if (session == null)
			throw new ArgumentException("Game session not found");

		var round = await _gameSessionRepository.GetRoundAsync(sessionId, roundNumber);
		if (round == null)
			throw new ArgumentException("Round not found");

		var response = new RoundInfoResponse
		{
			SessionId = sessionId,
			RoundNumber = roundNumber,
			Status = round.Status.ToString()
		};

		if (round.Status == RoundStatus.Completed)
		{
			response.Outcome = new Dictionary<string, string>();
			response.Summary = new Dictionary<string, int>();
			
			foreach (var choice in round.Choices)
			{
				response.Outcome[choice.Player.Name] = choice.Choice.ToString();
			}
			
			foreach (var player in session.Players)
			{
				response.Summary[player.Name] = player.Score;
			}
		}

		return response;
	}
}