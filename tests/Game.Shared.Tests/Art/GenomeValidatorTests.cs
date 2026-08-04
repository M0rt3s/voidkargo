using Game.Shared.Art.Genome;
using Game.Shared.Dtos;

namespace Game.Shared.Tests.Art;

/// <summary>
/// <see cref="GenomeValidator"/> is the style linter described in ADR 0006 - it must catch
/// structurally broken genomes (out-of-range values, incomplete zone maps, epoch-gated content
/// used too early) before they ever reach the renderer.
/// </summary>
public class GenomeValidatorTests
{
    [Fact]
    public void Validate_AcceptsWellFormedGenome()
    {
        var result = GenomeValidator.Validate(TestGenomes.ValidLightHauler());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_RejectsCanvasThatDoesNotMultiplyTo256()
    {
        var genome = TestGenomes.ValidLightHauler() with { Canvas = new CanvasSpec(Logical: 64, Scale: 3) };

        var result = GenomeValidator.Validate(genome);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Logical * Scale"));
    }

    [Fact]
    public void Validate_RejectsTooFewSpinePoints()
    {
        var genome = TestGenomes.ValidLightHauler() with
        {
            Silhouette = new SilhouetteSpec(new[] { new SpinePoint(32, 32, 3) }, AsymmetryBudget: 0),
        };

        var result = GenomeValidator.Validate(genome);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("at least 2"));
    }

    [Fact]
    public void Validate_RejectsThicknessOutsideClassRange()
    {
        // A HeavyHauler's average thickness must be a much larger fraction of the canvas than
        // these thin, LightHauler-scale spine points provide.
        var genome = TestGenomes.ValidLightHauler() with { Class = ShipClass.HeavyHauler };

        var result = GenomeValidator.Validate(genome);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("thickness fraction"));
    }

    [Fact]
    public void Validate_RejectsModuleUsedBeforeItsMinimumEpoch()
    {
        var genome = TestGenomes.ValidLightHauler();
        var modules = new List<ModuleSpec>(genome.Modules) { new(ModuleKind.Weapon, 32, 40, 1, 2, false) };
        genome = genome with { Epoch = 0, Modules = modules };

        var result = GenomeValidator.Validate(genome);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Weapon") && e.Contains("epoch"));
    }

    [Fact]
    public void Validate_RejectsIncompleteZoneMap()
    {
        var genome = TestGenomes.ValidLightHauler();
        var zones = new Dictionary<PaletteRole, int>(genome.Zones);
        zones.Remove(PaletteRole.Outline);
        genome = genome with { Zones = zones };

        var result = GenomeValidator.Validate(genome);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Outline"));
    }

    [Fact]
    public void Validate_RejectsZoneIndexOutOfPaletteRange()
    {
        var genome = TestGenomes.ValidLightHauler();
        var zones = new Dictionary<PaletteRole, int>(genome.Zones) { [PaletteRole.Hull] = 16 };
        genome = genome with { Zones = zones };

        var result = GenomeValidator.Validate(genome);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("[0, 15]"));
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void Validate_RejectsWearOutOfRange(double wear)
    {
        var genome = TestGenomes.ValidLightHauler() with { Wear = wear };

        var result = GenomeValidator.Validate(genome);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Wear"));
    }
}
