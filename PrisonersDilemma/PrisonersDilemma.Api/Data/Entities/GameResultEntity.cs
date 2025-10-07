using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PrisonersDilemma.Api.Models;

namespace PrisonersDilemma.Api.Data.Entities;

public class GameResultEntity
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public Guid GameSessionId { get; set; }
    
    public int Player1Score { get; set; }
    public int Player2Score { get; set; }
    
    [Required]
    public Choice Player1Choice { get; set; }
    
    [Required]
    public Choice Player2Choice { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Outcome { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    [ForeignKey(nameof(GameSessionId))]
    public virtual GameSessionEntity GameSession { get; set; } = null!;
}