using Game.Backend.Data;
using Game.Backend.Hubs;
using Game.Shared.Dtos;

var builder = WebApplication.CreateBuilder(args);

// Aspire: service discovery, resilience, health checks, OpenTelemetry.
builder.AddServiceDefaults();

builder.Services.AddOpenApi();
builder.Services.AddSignalR();

// Aspire Npgsql/EF Core integration: resolves the "gamedb" connection string
// from the AppHost-provided PostgreSQL resource (see Game.AppHost/AppHost.cs).
builder.AddNpgsqlDbContext<GameDbContext>(connectionName: "gamedb");

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHostedService<Game.Backend.Data.Seed.DevelopmentDataSeeder>();
}

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapHub<GameHub>("/hubs/game");

// REST: client-initiated, state-heavy actions (see docs/01-architecture/networking-strategy.md).
app.MapGet("/api/game-state", (GameDbContext db) =>
{
    var nodes = db.Nodes.Select(n => new NodeDto(n.Id, n.Name, n.X, n.Y, n.ResourceType, n.Stock)).ToList();
    var players = db.Players.Select(p => new PlayerDto(p.Id, p.DisplayName, p.Cash)).ToList();
    var trains = db.Trains.Select(t => new TrainDto(t.Id, t.OwnerPlayerId, t.FromNodeId, t.ToNodeId, t.ProgressPercent)).ToList();

    return Results.Ok(new GameStateSnapshotDto(nodes, players, trains));
})
.WithName("GetGameState");

app.Run();

/// <summary>Entry point class, exposed for WebApplicationFactory-based integration tests.</summary>
public partial class Program;
