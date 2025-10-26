using PrisonersDilemma.Api.Domain.Enums;

namespace PrisonersDilemma.Api.Domain.Entities;

public class GameSession
{
	public DateTime? CompletedAt { get; set; }
	public DateTime CreatedAt { get; set; }
	public int CurrentRound { get; set; }
	public Guid Id { get; set; }
	public int MaxRounds { get; set; }
	public List<Player> Players { get; set; } = [];
	public List<Round> Rounds { get; set; } = [];
	public GameSessionStatus Status { get; set; }
}