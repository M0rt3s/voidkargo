using System.Text;
using Game.Backend.Auth;
using Game.Backend.Data;
using Game.Backend.Entities;
using Game.Backend.Hubs;
using Game.Shared.Dtos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Aspire: service discovery, resilience, health checks, OpenTelemetry.
builder.AddServiceDefaults();

builder.Services.AddOpenApi();
builder.Services.AddSignalR();

// Aspire Npgsql/EF Core integration: resolves the "gamedb" connection string
// from the AppHost-provided PostgreSQL resource (see Game.AppHost/AppHost.cs).
builder.AddNpgsqlDbContext<GameDbContext>(connectionName: "gamedb");

// JWT signing options; ValidateOnStart fails the app fast at boot if Jwt:Key/Issuer/Audience
// are missing, too short, or (outside Development) still the local placeholder — see
// JwtOptionsValidation.
builder.Services.AddSingleton<IValidateOptions<JwtOptions>, JwtOptionsValidation>();
builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .ValidateOnStart();
builder.Services.AddSingleton<JwtTokenService>();

builder.Services
    .AddIdentityCore<UserEntity>(options =>
    {
        options.User.RequireUniqueEmail = true;

        if (builder.Environment.IsDevelopment())
        {
            // Relaxed only in Development so the seeded admin/admin, player/player and
            // gamemaster/gamemaster credentials (see DevelopmentDataSeeder) work verbatim for
            // testing authorization levels locally. Never relaxed outside Development.
            options.Password.RequireDigit = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 5;
            options.Password.RequiredUniqueChars = 1;
        }
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<GameDbContext>()
    .AddDefaultTokenProviders();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
                  ?? throw new InvalidOperationException("Jwt configuration section is missing.");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

builder.Services.AddAuthorization();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<GameHub>("/hubs/game");

app.MapAuthEndpoints();

// REST: client-initiated, state-heavy actions (see docs/01-architecture/networking-strategy.md).
app.MapGet("/api/game-state", (GameDbContext db) =>
{
    var nodes = db.Nodes
        .Select(n => new NodeDto(n.Id, n.Name, n.X, n.Y, n.Kind, n.ResourceType, n.Stock, n.Level, n.StressLevel, n.FactionId))
        .ToList();
    var players = db.Players.Select(p => new PlayerDto(p.Id, p.DisplayName, p.Cash)).ToList();
    var ships = db.Ships
        .Select(s => new ShipDto(s.Id, s.OwnerPlayerId, s.ShipTypeId, s.FromNodeId, s.ToNodeId, s.ProgressPercent))
        .ToList();
    var shipTypes = db.ShipTypes
        .Select(st => new ShipTypeDto(st.Id, st.DisplayName, st.Class, st.FactionId, st.Epoch, st.LoadCapacity, st.Speed, st.Acceleration, st.HopDistance))
        .ToList();
    var factions = db.Factions.Select(f => new FactionDto(f.Id, f.DisplayName, f.PaletteId)).ToList();

    return Results.Ok(new GameStateSnapshotDto(nodes, players, ships, shipTypes, factions));
})
.WithName("GetGameState")
.RequireAuthorization();

app.Run();

/// <summary>Entry point class, exposed for WebApplicationFactory-based integration tests.</summary>
public partial class Program;
