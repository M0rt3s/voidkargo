using Game.Shared.Dtos;

namespace Game.Backend.Entities;

/// <summary>
/// EF Core entity for a Faction. Deliberately separate from
/// <see cref="FactionDto"/> — the DTO is the wire contract, this is the persistence shape.
/// </summary>
public sealed class FactionEntity
{
    public required string Id { get; set; }
    public required string DisplayName { get; set; }
    public required string PaletteId { get; set; }
}

/// <summary>
/// EF Core entity for a catalog Ship Type. Deliberately separate from
/// <see cref="ShipTypeDto"/> — the DTO is the wire contract, this is the persistence shape.
/// </summary>
public sealed class ShipTypeEntity
{
    public required string Id { get; set; }
    public required string DisplayName { get; set; }
    public ShipClass Class { get; set; }
    public required string FactionId { get; set; }
    public int Epoch { get; set; }
    public double LoadCapacity { get; set; }
    public double Speed { get; set; }
    public double Acceleration { get; set; }
    public double HopDistance { get; set; }
}

/// <summary>
/// EF Core entity for a map Node. Deliberately separate from
/// <see cref="NodeDto"/> — the DTO is the wire contract,
/// this is the persistence shape. Map explicitly between them.
/// </summary>
/// <remarks>
/// <see cref="NodeKind"/> and <see cref="Game.Shared.Dtos.ShipClass"/> are reused directly from
/// Game.Shared rather than re-declared here: they're closed, low-churn value enums (not evolving
/// wire-contract shapes), so duplicating them would only invite drift. This is narrower than
/// sharing full DTOs and doesn't conflict with the entity/DTO separation convention above.
/// </remarks>
public sealed class NodeEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public NodeKind Kind { get; set; }
    public required string ResourceType { get; set; }
    public int Stock { get; set; }
    public int Level { get; set; } = 1;
    public double StressLevel { get; set; }
    public string? FactionId { get; set; }
}

/// <summary>EF Core entity for a Player.</summary>
public sealed class PlayerEntity
{
    public Guid Id { get; set; }
    public required string DisplayName { get; set; }
    public decimal Cash { get; set; }
}

/// <summary>
/// EF Core entity for a Ship moving between two Nodes. There is no track/route to persist -
/// hop distance is a stat of the ship's <see cref="ShipTypeEntity"/>.
/// </summary>
public sealed class ShipEntity
{
    public Guid Id { get; set; }
    public Guid OwnerPlayerId { get; set; }
    public required string ShipTypeId { get; set; }
    public Guid FromNodeId { get; set; }
    public Guid ToNodeId { get; set; }
    public double ProgressPercent { get; set; }
}
