// Explicit usings and a block-scoped namespace are used here (instead of relying on
// ImplicitUsings/file-scoped namespaces) because Unity compiles this file directly as part of
// the com.voidkargo.shared local package, ignoring this project's .csproj SDK settings, and
// Unity's compiler is pinned to C# 9.0 (file-scoped namespaces need C# 10+).
using System;
using System.Collections.Generic;

namespace Game.Shared.Dtos
{
    /// <summary>
    /// The kind of map location a <see cref="NodeDto"/> represents. See
    /// docs/00-overview/glossary.md for the "Node" domain term.
    /// </summary>
    public enum NodeKind
    {
        /// <summary>
        /// A player-facing settlement that consumes delivered resources over time and levels up
        /// once consumption targets are met - the "town" equivalent from the Rail Nation-style
        /// progression loop. See docs/01-architecture/data-model.md.
        /// </summary>
        Station,

        /// <summary>A planetary body node; may host a Station and/or one or more ProductionSites.</summary>
        Planet,

        /// <summary>A late-epoch mega-structure node, unlocked via a faction's research tree.</summary>
        DysonSphere,

        /// <summary>
        /// A resource-producing node with finite output. Overloading it raises
        /// <see cref="NodeDto.StressLevel"/>; investing materials relieves stress and raises
        /// <see cref="NodeDto.Level"/>, which increases its production ceiling.
        /// </summary>
        ProductionSite,
    }

    /// <summary>
    /// The broad performance archetype of a Ship. Concrete Ship *types* (~20 per class, gated by
    /// faction/epoch) are data - see <see cref="ShipTypeDto"/> - not one hardcoded type per class.
    /// </summary>
    public enum ShipClass
    {
        /// <summary>Fast, low load capacity, long hop distance.</summary>
        LightHauler,

        /// <summary>
        /// Balanced load/speed/acceleration/hop-distance trade-off; only some epochs introduce
        /// new Medium Hauler ship types.
        /// </summary>
        MediumHauler,

        /// <summary>Slow, high load capacity, short hop distance.</summary>
        HeavyHauler,
    }

    /// <summary>
    /// A faction identity. Factions are catalog data (not a fixed enum) so new factions can be
    /// added without a Game.Shared contract change; the id also keys into the art pipeline's
    /// palette system (see ADR 0004) so a faction's look and its game-design identity share one
    /// source of truth.
    /// </summary>
    public sealed record FactionDto(string Id, string DisplayName, string PaletteId);

    /// <summary>
    /// A catalog entry describing one concrete ship type (e.g. one of the ~20-per-class designs).
    /// Instances of a type in play are <see cref="ShipDto"/>; this is the shared definition that
    /// drives both game math and the procedural art genome (matched by <see cref="Id"/>).
    /// </summary>
    public sealed record ShipTypeDto(
        string Id,
        string DisplayName,
        ShipClass Class,
        string FactionId,
        int Epoch,
        double LoadCapacity,
        double Speed,
        double Acceleration,
        double HopDistance);

    /// <summary>A registered player controlling one or more companies/fleets.</summary>
    public sealed record PlayerDto(Guid Id, string DisplayName, decimal Cash);

    /// <summary>
    /// A map location that can produce, consume, or transfer resources.
    /// See docs/00-overview/glossary.md for the "Node" domain term.
    /// </summary>
    public sealed record NodeDto(
        Guid Id,
        string Name,
        double X,
        double Y,
        NodeKind Kind,
        string ResourceType,
        int Stock,
        int Level,
        double StressLevel,
        string? FactionId);

    /// <summary>
    /// A ship instance moving resources between two Nodes. There is no track/route to lay -
    /// hop distance is a stat of the ship's <see cref="ShipTypeDto"/>; this only tracks progress
    /// along the current hop. See docs/00-overview/glossary.md.
    /// </summary>
    public sealed record ShipDto(Guid Id, Guid OwnerPlayerId, string ShipTypeId, Guid FromNodeId, Guid ToNodeId, double ProgressPercent);

    /// <summary>
    /// The initial state a client downloads over REST after login.
    /// See docs/01-architecture/networking-strategy.md.
    /// </summary>
    public sealed record GameStateSnapshotDto(
        IReadOnlyList<NodeDto> Nodes,
        IReadOnlyList<PlayerDto> Players,
        IReadOnlyList<ShipDto> Ships,
        IReadOnlyList<ShipTypeDto> ShipTypes,
        IReadOnlyList<FactionDto> Factions);

    /// <summary>
    /// A real-time update pushed over SignalR when the server-side economy tick runs.
    /// </summary>
    public sealed record EconomyTickDto(DateTimeOffset OccurredAtUtc, IReadOnlyDictionary<string, decimal> PriceByResourceType);
}
