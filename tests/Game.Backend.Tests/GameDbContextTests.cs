using Game.Backend.Data;
using Game.Backend.Entities;
using Game.Shared.Dtos;
using Microsoft.AspNetCore.Identity;
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
        var node = new NodeEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test Depot",
            X = 1,
            Y = 2,
            Kind = NodeKind.Station,
            ResourceType = "Grain",
            Stock = 100,
            Level = 1,
            StressLevel = 0.0,
        };

        db.Nodes.Add(node);
        await db.SaveChangesAsync();

        var stored = await db.Nodes.SingleAsync(n => n.Id == node.Id);
        Assert.Equal("Test Depot", stored.Name);
        Assert.Equal(100, stored.Stock);
        Assert.Equal(NodeKind.Station, stored.Kind);
    }

    [Fact]
    public async Task AddingShip_ReferencesOwnerNodesAndShipType()
    {
        await using var db = CreateContext();
        var faction = new FactionEntity { Id = "test-faction", DisplayName = "Test Faction", PaletteId = "test-faction-default" };
        var shipType = new ShipTypeEntity
        {
            Id = "vk.ship.light.test",
            DisplayName = "Test Hauler",
            Class = ShipClass.LightHauler,
            FactionId = faction.Id,
            Epoch = 1,
            LoadCapacity = 10,
            Speed = 5,
            Acceleration = 2,
            HopDistance = 3,
        };
        var from = new NodeEntity { Id = Guid.NewGuid(), Name = "A", X = 0, Y = 0, Kind = NodeKind.Station, ResourceType = "Grain", Stock = 0 };
        var to = new NodeEntity { Id = Guid.NewGuid(), Name = "B", X = 1, Y = 1, Kind = NodeKind.ProductionSite, ResourceType = "Ore", Stock = 0 };
        var player = new PlayerEntity { Id = Guid.NewGuid(), DisplayName = "tester", Cash = 500m };
        var ship = new ShipEntity
        {
            Id = Guid.NewGuid(),
            OwnerPlayerId = player.Id,
            ShipTypeId = shipType.Id,
            FromNodeId = from.Id,
            ToNodeId = to.Id,
            ProgressPercent = 0.25,
        };

        db.Factions.Add(faction);
        db.ShipTypes.Add(shipType);
        db.Nodes.AddRange(from, to);
        db.Players.Add(player);
        db.Ships.Add(ship);
        await db.SaveChangesAsync();

        var stored = await db.Ships.SingleAsync(s => s.Id == ship.Id);
        Assert.Equal(player.Id, stored.OwnerPlayerId);
        Assert.Equal(0.25, stored.ProgressPercent);
        Assert.Equal(shipType.Id, stored.ShipTypeId);
    }

    [Fact]
    public async Task AddingUserWithRole_CanBeReadBack()
    {
        await using var db = CreateContext();
        var role = new IdentityRole<Guid> { Id = Guid.NewGuid(), Name = "Player", NormalizedName = "PLAYER" };
        var user = new UserEntity { Id = Guid.NewGuid(), UserName = "player", NormalizedUserName = "PLAYER", DisplayName = "Dev Player" };
        var userRole = new IdentityUserRole<Guid> { UserId = user.Id, RoleId = role.Id };

        db.Roles.Add(role);
        db.Users.Add(user);
        db.UserRoles.Add(userRole);
        await db.SaveChangesAsync();

        var storedUser = await db.Users.SingleAsync(u => u.Id == user.Id);
        Assert.Equal("Dev Player", storedUser.DisplayName);
        Assert.Single(await db.UserRoles.Where(ur => ur.UserId == user.Id).ToListAsync());
    }
}
