namespace PrisonersDilemma.Api.Application.DTOs;

public class StartGameRequest
{
	public Guid PlayerId { get; set; }
	public string PlayerName { get; set; } = string.Empty;
}