using Game.Backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace Game.Backend.Data.Seed;

/// <summary>
/// Development-only hosted service that populates a fresh database with a
/// playable slice of the game (map, dummy players, resource nodes) on startup,
/// so features can be tested immediately. See ADR 0001 and
/// docs/04-workflows/local-dev-setup.md.
/// </summary>
public sealed class DevelopmentDataSeeder(IServiceProvider services, ILogger<DevelopmentDataSeeder> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GameDbContext>();

        await db.Database.MigrateAsync(cancellationToken);

        if (await db.Nodes.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Development seed data already present; skipping seed.");
            return;
        }

        logger.LogInformation("Seeding development database with a playable slice of the game...");

        var nodeA = new NodeEntity { Id = Guid.NewGuid(), Name = "Riverside Depot", X = 0, Y = 0, ResourceType = "Grain", Stock = 500 };
        var nodeB = new NodeEntity { Id = Guid.NewGuid(), Name = "Ironhold Yard", X = 120, Y = 40, ResourceType = "Ore", Stock = 300 };
        var player = new PlayerEntity { Id = Guid.NewGuid(), DisplayName = "dev-player", Cash = 10_000m };
        var train = new TrainEntity
        {
            Id = Guid.NewGuid(),
            OwnerPlayerId = player.Id,
            FromNodeId = nodeA.Id,
            ToNodeId = nodeB.Id,
            ProgressPercent = 0.0,
        };

        db.Nodes.AddRange(nodeA, nodeB);
        db.Players.Add(player);
        db.Trains.Add(train);

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Development seed complete.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
