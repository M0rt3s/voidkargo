using Game.Backend.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Game.Backend.Data;

/// <summary>
/// The authoritative EF Core context for game state, backed by PostgreSQL. Also the ASP.NET
/// Core Identity store (users, roles, claims) — see docs/01-architecture/data-model.md.
/// </summary>
public sealed class GameDbContext(DbContextOptions<GameDbContext> options)
    : IdentityDbContext<UserEntity, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<NodeEntity> Nodes => Set<NodeEntity>();

    public DbSet<PlayerEntity> Players => Set<PlayerEntity>();

    public DbSet<ShipEntity> Ships => Set<ShipEntity>();

    public DbSet<ShipTypeEntity> ShipTypes => Set<ShipTypeEntity>();

    public DbSet<FactionEntity> Factions => Set<FactionEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configures the Identity (AspNetUsers/AspNetRoles/...) tables first.
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<NodeEntity>().HasKey(n => n.Id);
        modelBuilder.Entity<PlayerEntity>().HasKey(p => p.Id);
        modelBuilder.Entity<ShipEntity>().HasKey(s => s.Id);
        modelBuilder.Entity<ShipTypeEntity>().HasKey(st => st.Id);
        modelBuilder.Entity<FactionEntity>().HasKey(f => f.Id);
    }
}
