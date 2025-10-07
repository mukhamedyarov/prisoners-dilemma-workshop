using System.ComponentModel.DataAnnotations;
using PrisonersDilemma.Api.Models;

namespace PrisonersDilemma.Api.Data.Entities;

public class GameSessionEntity
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid Player1Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Player1Name { get; set; } = string.Empty;
    
    [Required]
    public Guid Player2Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Player2Name { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    public Choice? PendingPlayer1Choice { get; set; }
    public Choice? PendingPlayer2Choice { get; set; }
    
    public virtual ICollection<GameResultEntity> Results { get; set; } = new List<GameResultEntity>();
}