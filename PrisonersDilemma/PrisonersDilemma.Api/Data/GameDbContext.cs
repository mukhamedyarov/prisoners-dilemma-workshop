using Microsoft.EntityFrameworkCore;

namespace PrisonersDilemma.Api.Data;

public class GameDbContext : DbContext
{
	public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
	{
	}


	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
	}
}