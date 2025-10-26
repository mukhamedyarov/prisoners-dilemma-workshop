using Microsoft.EntityFrameworkCore;
using PrisonersDilemma.Api.Domain.Entities;

namespace PrisonersDilemma.Api.Data;

public class GameDbContext : DbContext
{
	public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
	{
	}

	public DbSet<GameSession> GameSessions { get; set; }
	public DbSet<Player> Players { get; set; }
	public DbSet<Round> Rounds { get; set; }
	public DbSet<PlayerChoice> PlayerChoices { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<GameSession>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.Status).HasConversion<string>();
			entity.HasIndex(e => e.Status);
			entity.HasMany(e => e.Players).WithOne(e => e.GameSession).HasForeignKey(e => e.GameSessionId);
			entity.HasMany(e => e.Rounds).WithOne(e => e.GameSession).HasForeignKey(e => e.GameSessionId);
		});

		modelBuilder.Entity<Player>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
			entity.HasMany(e => e.Choices).WithOne(e => e.Player).HasForeignKey(e => e.PlayerId);
		});

		modelBuilder.Entity<Round>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.Status).HasConversion<string>();
			entity.HasIndex(e => new { e.Number, e.GameSessionId }).IsUnique();
			entity.HasMany(e => e.Choices).WithOne(e => e.Round).HasForeignKey(e => e.RoundId);
		});

		modelBuilder.Entity<PlayerChoice>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.Choice).HasConversion<string>();
		});
	}
}