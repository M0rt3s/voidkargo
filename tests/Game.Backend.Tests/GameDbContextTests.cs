using Game.Backend.Data;
using Game.Backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace Game.Backend.Tests;

public class GameDbContextTests
{
    private static GameDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new GameDbContext(options);
    }

    [Fact]
    public async Task AddingNode_CanBeReadBack()
    {
        await using var db = CreateContext();
        var node = new NodeEntity { Id = Guid.NewGuid(), Name = "Test Depot", X = 1, Y = 2, ResourceType = "Grain", Stock = 100 };

        db.Nodes.Add(node);
        await db.SaveChangesAsync();

        var stored = await db.Nodes.SingleAsync(n => n.Id == node.Id);
        Assert.Equal("Test Depot", stored.Name);
        Assert.Equal(100, stored.Stock);
    }

    [Fact]
    public async Task AddingTrain_ReferencesOwnerAndNodes()
    {
        await using var db = CreateContext();
        var from = new NodeEntity { Id = Guid.NewGuid(), Name = "A", X = 0, Y = 0, ResourceType = "Grain", Stock = 0 };
        var to = new NodeEntity { Id = Guid.NewGuid(), Name = "B", X = 1, Y = 1, ResourceType = "Ore", Stock = 0 };
        var player = new PlayerEntity { Id = Guid.NewGuid(), DisplayName = "tester", Cash = 500m };
        var train = new TrainEntity { Id = Guid.NewGuid(), OwnerPlayerId = player.Id, FromNodeId = from.Id, ToNodeId = to.Id, ProgressPercent = 0.25 };

        db.Nodes.AddRange(from, to);
        db.Players.Add(player);
        db.Trains.Add(train);
        await db.SaveChangesAsync();

        var stored = await db.Trains.SingleAsync(t => t.Id == train.Id);
        Assert.Equal(player.Id, stored.OwnerPlayerId);
        Assert.Equal(0.25, stored.ProgressPercent);
    }
}
