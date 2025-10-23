namespace PrisonersDilemma.Api.Domain.Entities;

public class Player
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public int Score { get; set; }
	public Guid GameSessionId { get; set; }
	public GameSession GameSession { get; set; } = null!;
	public List<PlayerChoice> Choices { get; set; } = [];
}