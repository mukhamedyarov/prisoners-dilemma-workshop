namespace PrisonersDilemma.Api.Application.DTOs;

public class RoundInfoResponse
{
	public Guid SessionId { get; set; }
	public int RoundNumber { get; set; }
	public string Status { get; set; } = string.Empty;
	public Dictionary<string, string>? Outcome { get; set; }
	public Dictionary<string, int>? Summary { get; set; }
}