using PrisonersDilemma.Api.Application.DTOs;
using PrisonersDilemma.Api.Application.Interfaces;
using PrisonersDilemma.Api.Domain.Entities;
using PrisonersDilemma.Api.Domain.Enums;
using PrisonersDilemma.Api.Domain.Services;
using PrisonersDilemma.Api.Exceptions;

namespace PrisonersDilemma.Api.Application.Services;

public class GameService : IGameService
{
	private readonly IGameSessionRepository _gameSessionRepository;
	private readonly int _maxRounds;

	public GameService(IGameSessionRepository gameSessionRepository, IConfiguration configuration)
	{
		_gameSessionRepository = gameSessionRepository;
		_maxRounds = int.Parse(configuration.GetValue<string>("GAME_ROUNDS_COUNT") ?? "150");
	}

	public async Task<GameInfoResponse> GetGameInfoAsync(Guid sessionId)
	{
		var session = await _gameSessionRepository.GetByIdAsync(sessionId);
		if (session == null)
			throw new GameNotFoundException("Game session not found");

		var response = new GameInfoResponse
		{
			SessionId = session.Id,
			Status = session.Status.ToString(),
			CurrentRound = session.CurrentRound,
			MaxRounds = session.MaxRounds
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
			throw new GameNotFoundException("Game session not found");

		var round = await _gameSessionRepository.GetRoundAsync(sessionId, roundNumber);
		if (round == null)
			throw new RoundNotFoundException("Round not found");

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

	public async Task<StartGameResponse> StartGameAsync(StartGameRequest request)
	{
		var existingPlayer = await _gameSessionRepository.GetPlayerByIdAsync(request.PlayerId);
		if (existingPlayer != null)
		{
			return new StartGameResponse
			{
				SessionId = existingPlayer.GameSessionId,
				PlayerId = existingPlayer.Id,
				PlayerName = existingPlayer.Name
			};
		}

		var (session, player) = await _gameSessionRepository.StartGameAtomicAsync(request.PlayerId, request.PlayerName, _maxRounds);

		return new StartGameResponse
		{
			SessionId = session.Id,
			PlayerId = player.Id,
			PlayerName = player.Name
		};
	}

	public async Task<RoundInfoResponse> SubmitChoiceAsync(SubmitChoiceRequest request)
	{
		var session = await _gameSessionRepository.GetByIdAsync(request.SessionId);
		if (session == null)
			throw new GameNotFoundException("Game session not found");

		if (session.Status != GameSessionStatus.Active)
			throw new InvalidGameStateException("Game session is not active");

		var player = session.Players.FirstOrDefault(p => p.Id == request.PlayerId);
		if (player == null)
			throw new InvalidRequestException("Player not found in this session");

		if (request.RoundNumber != session.CurrentRound)
			throw new InvalidRequestException("Invalid round number");

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

			_gameSessionRepository.AddRound(round);
		}

		if (round.Choices.Any(c => c.PlayerId == request.PlayerId))
			throw new InvalidGameStateException("Player has already made a choice for this round");

		if (!Enum.TryParse<Choice>(request.Choice, true, out var choice))
			throw new InvalidRequestException("Invalid choice");

		var playerChoice = new PlayerChoice
		{
			Id = Guid.NewGuid(),
			PlayerId = request.PlayerId,
			RoundId = round.Id,
			Choice = choice,
			CreatedAt = DateTime.UtcNow
		};

		_gameSessionRepository.AddPlayerChoice(playerChoice);

		if (round.Choices.Count == 2)
		{
			GameLogicService.ProcessRoundCompletion(round, session);
			_gameSessionRepository.AddRound(new Round
			{
				Id = Guid.NewGuid(),
				Number = round.Number + 1,
				Status = RoundStatus.InProgress,
				GameSessionId = session.Id,
				CreatedAt = DateTime.UtcNow
			});
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
}