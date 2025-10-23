namespace PrisonersDilemma.Api.Application.DTOs;

public class StartGameResponse
{
	public Guid SessionId { get; set; }
	public Guid PlayerId { get; set; }
	public string PlayerName { get; set; } = string.Empty;
}