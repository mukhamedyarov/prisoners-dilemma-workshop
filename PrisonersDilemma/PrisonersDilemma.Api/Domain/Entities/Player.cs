namespace PrisonersDilemma.Api.Domain.Entities;

public class Player
{
	public List<PlayerChoice> Choices { get; set; } = [];
	public GameSession GameSession { get; set; } = null!;
	public Guid GameSessionId { get; set; }
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public int Score { get; set; }
}