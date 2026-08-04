using Game.Backend.Entities;
using Game.Shared.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Game.Backend.Data.Seed;

/// <summary>
/// Development-only hosted service that populates a fresh database with a
/// playable slice of the game (map, dummy players, resource nodes) on startup,
/// so features can be tested immediately. See ADR 0001 and
/// docs/04-workflows/local-dev-setup.md.
///
/// Also seeds the three authorization roles (Admin, GameMaster, Player) and one dev-only
/// account per role (admin/admin, player/player, gamemaster/gamemaster) so authorization
/// levels can be tested locally without hand-registering accounts. This service is only
/// registered when <c>IsDevelopment()</c> is true (see Program.cs) and Identity's password
/// policy is only relaxed to allow these simple passwords in that same environment — see
/// Program.cs's <c>AddIdentityCore</c> configuration.
/// </summary>
public sealed class DevelopmentDataSeeder(IServiceProvider services, ILogger<DevelopmentDataSeeder> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GameDbContext>();

        await db.Database.MigrateAsync(cancellationToken);

        await SeedRolesAndUsersAsync(scope.ServiceProvider, cancellationToken);

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

    /// <summary>
    /// Ensures the Admin/GameMaster/Player roles and one credential-testing account per role
    /// exist. Idempotent — safe to run on every startup against an already-seeded database.
    /// </summary>
    private async Task SeedRolesAndUsersAsync(IServiceProvider provider, CancellationToken cancellationToken)
    {
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = provider.GetRequiredService<UserManager<UserEntity>>();

        foreach (var role in GameRoles.All)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        await EnsureDevUserAsync(userManager, "admin", "admin@voidkargo.local", "admin", "Dev Admin", GameRoles.Admin);
        await EnsureDevUserAsync(userManager, "player", "player@voidkargo.local", "player", "Dev Player", GameRoles.Player);
        await EnsureDevUserAsync(userManager, "gamemaster", "gamemaster@voidkargo.local", "gamemaster", "Dev GameMaster", GameRoles.GameMaster);
    }

    private async Task EnsureDevUserAsync(
        UserManager<UserEntity> userManager,
        string userName,
        string email,
        string password,
        string displayName,
        string role)
    {
        if (await userManager.FindByNameAsync(userName) is not null)
        {
            return;
        }

        var user = new UserEntity
        {
            UserName = userName,
            Email = email,
            DisplayName = displayName,
            EmailConfirmed = true,
        };

        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            logger.LogWarning(
                "Failed to seed development user '{UserName}': {Errors}",
                userName,
                string.Join("; ", createResult.Errors.Select(e => e.Description)));
            return;
        }

        await userManager.AddToRoleAsync(user, role);
        logger.LogInformation("Seeded development user '{UserName}' with role '{Role}'.", userName, role);
    }
}
