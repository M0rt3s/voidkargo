using Game.Backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace Game.Backend.Data;

/// <summary>
/// The authoritative EF Core context for game state, backed by PostgreSQL.
/// See docs/01-architecture/data-model.md.
/// </summary>
public sealed class GameDbContext(DbContextOptions<GameDbContext> options) : DbContext(options)
{
    public DbSet<NodeEntity> Nodes => Set<NodeEntity>();

    public DbSet<PlayerEntity> Players => Set<PlayerEntity>();

    public DbSet<TrainEntity> Trains => Set<TrainEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NodeEntity>().HasKey(n => n.Id);
        modelBuilder.Entity<PlayerEntity>().HasKey(p => p.Id);
        modelBuilder.Entity<TrainEntity>().HasKey(t => t.Id);
    }
}
