using PrisonersDilemma.Api.Domain.Entities;
using PrisonersDilemma.Api.Domain.Enums;

namespace PrisonersDilemma.Api.Domain.Services;

public static class GameLogicService
{
	public static (int player1Score, int player2Score) CalculateRoundScore(Choice player1Choice, Choice player2Choice)
	{
		return (player1Choice, player2Choice) switch
		{
			(Choice.Cooperate, Choice.Cooperate) => (3, 3),
			(Choice.Defect, Choice.Defect) => (1, 1),
			(Choice.Cooperate, Choice.Defect) => (0, 5),
			(Choice.Defect, Choice.Cooperate) => (5, 0),
			_ => throw new ArgumentException("Invalid choice combination")
		};
	}

	public static void ProcessRoundCompletion(Round round, GameSession gameSession)
	{
		if (round.Choices.Count != 2)
			throw new InvalidOperationException("Round must have exactly 2 choices to be completed");

		var player1Choice = round.Choices.First();
		var player2Choice = round.Choices.Last();

		var (player1Score, player2Score) = CalculateRoundScore(player1Choice.Choice, player2Choice.Choice);

		player1Choice.Player.Score += player1Score;
		player2Choice.Player.Score += player2Score;

		round.Status = RoundStatus.Completed;
		round.CompletedAt = DateTime.UtcNow;

		if (gameSession.CurrentRound >= gameSession.MaxRounds)
		{
			gameSession.Status = GameSessionStatus.Completed;
			gameSession.CompletedAt = DateTime.UtcNow;
		}
		else
		{
			gameSession.CurrentRound++;
		}
	}
}