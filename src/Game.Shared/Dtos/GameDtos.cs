// Explicit usings and a block-scoped namespace are used here (instead of relying on
// ImplicitUsings/file-scoped namespaces) because Unity compiles this file directly as part of
// the com.voidkargo.shared local package, ignoring this project's .csproj SDK settings, and
// Unity's compiler is pinned to C# 9.0 (file-scoped namespaces need C# 10+).
using System;
using System.Collections.Generic;

namespace Game.Shared.Dtos
{
    /// <summary>
    /// A map location that can produce, consume, or transfer resources.
    /// See docs/00-overview/glossary.md for the "Node" domain term.
    /// </summary>
    public sealed record NodeDto(Guid Id, string Name, double X, double Y, string ResourceType, int Stock);

    /// <summary>A registered player controlling one or more companies/fleets.</summary>
    public sealed record PlayerDto(Guid Id, string DisplayName, decimal Cash);

    /// <summary>A unit moving resources between two Nodes along a Route.</summary>
    public sealed record TrainDto(Guid Id, Guid OwnerPlayerId, Guid FromNodeId, Guid ToNodeId, double ProgressPercent);

    /// <summary>
    /// The initial state a client downloads over REST after login.
    /// See docs/01-architecture/networking-strategy.md.
    /// </summary>
    public sealed record GameStateSnapshotDto(IReadOnlyList<NodeDto> Nodes, IReadOnlyList<PlayerDto> Players, IReadOnlyList<TrainDto> Trains);

    /// <summary>
    /// A real-time update pushed over SignalR when the server-side economy tick runs.
    /// </summary>
    public sealed record EconomyTickDto(DateTimeOffset OccurredAtUtc, IReadOnlyDictionary<string, decimal> PriceByResourceType);
}
