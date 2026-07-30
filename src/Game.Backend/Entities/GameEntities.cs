namespace Game.Backend.Entities;

/// <summary>
/// EF Core entity for a map Node. Deliberately separate from
/// <see cref="Game.Shared.Dtos.NodeDto"/> — the DTO is the wire contract,
/// this is the persistence shape. Map explicitly between them.
/// </summary>
public sealed class NodeEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public required string ResourceType { get; set; }
    public int Stock { get; set; }
}

/// <summary>EF Core entity for a Player.</summary>
public sealed class PlayerEntity
{
    public Guid Id { get; set; }
    public required string DisplayName { get; set; }
    public decimal Cash { get; set; }
}

/// <summary>EF Core entity for a Train moving between two Nodes.</summary>
public sealed class TrainEntity
{
    public Guid Id { get; set; }
    public Guid OwnerPlayerId { get; set; }
    public Guid FromNodeId { get; set; }
    public Guid ToNodeId { get; set; }
    public double ProgressPercent { get; set; }
}
