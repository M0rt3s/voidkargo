var builder = DistributedApplication.CreateBuilder(args);

// Local PostgreSQL container with a persistent volume. See ADR 0001.
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume();

var gamedb = postgres.AddDatabase("gamedb");

var backend = builder.AddProject<Projects.Game_Backend>("game-backend")
    .WithReference(gamedb)
    .WaitFor(gamedb);

builder.AddProject<Projects.Game_Website>("game-website")
    .WithReference(backend)
    .WaitFor(backend);

builder.Build().Run();
