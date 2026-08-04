using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Game.Backend.Data;

/// <summary>
/// Design-time factory so `dotnet ef migrations add` can construct <see cref="GameDbContext"/>
/// without running the full Aspire-orchestrated app host (which otherwise needs a live
/// service-discovery connection string from Game.AppHost). The connection string below is
/// never used to actually connect — EF's migration generation only needs to know the target
/// provider (Npgsql) to produce the right SQL. The real connection string is supplied at
/// runtime by Aspire (see Program.cs's <c>AddNpgsqlDbContext</c> call).
/// </summary>
public sealed class GameDbContextFactory : IDesignTimeDbContextFactory<GameDbContext>
{
    public GameDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GameDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=gamedb;Username=postgres;Password=postgres");

        return new GameDbContext(optionsBuilder.Options);
    }
}
