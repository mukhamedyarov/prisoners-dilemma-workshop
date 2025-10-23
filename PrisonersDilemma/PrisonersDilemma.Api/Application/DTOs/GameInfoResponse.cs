namespace PrisonersDilemma.Api.Application.DTOs;

public class GameInfoResponse
{
	public Guid SessionId { get; set; }
	public string Status { get; set; } = string.Empty;
	public string? Player1Name { get; set; }
	public string? Player2Name { get; set; }
	public int CurrentRound { get; set; }
	public Dictionary<string, int> Summary { get; set; } = new();
}