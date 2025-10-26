using PrisonersDilemma.Api.Domain.Enums;

namespace PrisonersDilemma.Api.Domain.Entities;

public class PlayerChoice
{
	public Choice Choice { get; set; }
	public DateTime CreatedAt { get; set; }
	public Guid Id { get; set; }
	public Player Player { get; set; } = null!;
	public Guid PlayerId { get; set; }
	public Round Round { get; set; } = null!;
	public Guid RoundId { get; set; }
}