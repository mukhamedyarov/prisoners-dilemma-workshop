using PrisonersDilemma.Api.Domain.Enums;

namespace PrisonersDilemma.Api.Domain.Entities;

public class Round
{
	public List<PlayerChoice> Choices { get; set; } = [];
	public DateTime? CompletedAt { get; set; }
	public DateTime CreatedAt { get; set; }
	public GameSession GameSession { get; set; } = null!;
	public Guid GameSessionId { get; set; }
	public Guid Id { get; set; }
	public int Number { get; set; }
	public RoundStatus Status { get; set; }
}