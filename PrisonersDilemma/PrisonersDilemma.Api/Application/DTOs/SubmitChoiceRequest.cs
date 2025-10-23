namespace PrisonersDilemma.Api.Application.DTOs;

public class SubmitChoiceRequest
{
	public Guid SessionId { get; set; }
	public Guid PlayerId { get; set; }
	public int RoundNumber { get; set; }
	public string Choice { get; set; } = string.Empty;
}