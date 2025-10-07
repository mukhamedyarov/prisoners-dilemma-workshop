using Microsoft.EntityFrameworkCore;
using PrisonersDilemma.Api.Data.Entities;
using PrisonersDilemma.Api.Models;

namespace PrisonersDilemma.Api.Data;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
    {
    }
    
    public DbSet<GameSessionEntity> GameSessions { get; set; }
    public DbSet<GameResultEntity> GameResults { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<GameSessionEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Player1Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Player2Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedAt).IsRequired();
            
            entity.Property(e => e.PendingPlayer1Choice)
                .HasConversion<string>()
                .HasMaxLength(50);
            
            entity.Property(e => e.PendingPlayer2Choice)
                .HasConversion<string>()
                .HasMaxLength(50);
            
            entity.HasMany(e => e.Results)
                .WithOne(r => r.GameSession)
                .HasForeignKey(r => r.GameSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<GameResultEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.GameSessionId).IsRequired();
            entity.Property(e => e.Player1Score).IsRequired();
            entity.Property(e => e.Player2Score).IsRequired();
            entity.Property(e => e.Outcome).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CreatedAt).IsRequired();
            
            entity.Property(e => e.Player1Choice)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50);
            
            entity.Property(e => e.Player2Choice)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50);
        });
    }
}