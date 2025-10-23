using PrisonersDilemma.Api.Domain.Enums;

namespace PrisonersDilemma.Api.Domain.Entities;

public class GameSession
{
	public Guid Id { get; set; }
	public GameSessionStatus Status { get; set; }
	public int CurrentRound { get; set; }
	public int MaxRounds { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? CompletedAt { get; set; }
	public List<Player> Players { get; set; } = [];
	public List<Round> Rounds { get; set; } = [];
}