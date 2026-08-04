using Game.Shared.Art.Genome;
using Game.Shared.Dtos;

namespace Game.Shared.Tests.Art;

/// <summary>Shared, hand-crafted genomes used across the art pipeline test suite so each test focuses on one behaviour.</summary>
public static class TestGenomes
{
    /// <summary>A small, valid <see cref="ShipClass.LightHauler"/> genome: thin hull, one engine module, moderate greebles/wear.</summary>
    public static ShipGenome ValidLightHauler(ulong seed = 1234) => new ShipGenome(
        Id: "test-light-hauler",
        Class: ShipClass.LightHauler,
        FactionId: "test-faction",
        Epoch: 0,
        Seed: seed,
        Canvas: new CanvasSpec(Logical: 64, Scale: 4),
        Silhouette: new SilhouetteSpec(
            Spine: new[]
            {
                new SpinePoint(32, 6, 2),
                new SpinePoint(30, 20, 3),
                new SpinePoint(32, 32, 4),
                new SpinePoint(30, 46, 3),
                new SpinePoint(32, 58, 2),
            },
            AsymmetryBudget: 1.0),
        Modules: new[]
        {
            new ModuleSpec(ModuleKind.Engine, AnchorX: 32, AnchorY: 60, Count: 2, Size: 3, Emissive: true),
            new ModuleSpec(ModuleKind.Cargo, AnchorX: 32, AnchorY: 30, Count: 1, Size: 4, Emissive: false),
        },
        Greebles: new GreebleSpec(Density: 0.15, Style: "slavic-industrial"),
        Zones: new Dictionary<PaletteRole, int>
        {
            [PaletteRole.Hull] = 5,
            [PaletteRole.HullShadow] = 4,
            [PaletteRole.HullLight] = 6,
            [PaletteRole.Trim] = 7,
            [PaletteRole.Accent] = 8,
            [PaletteRole.Glass] = 9,
            [PaletteRole.Emissive] = 10,
            [PaletteRole.Outline] = 1,
        },
        Wear: 0.2);
}
