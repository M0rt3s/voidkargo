using Game.Shared.Art.Genome;

namespace Game.Shared.Tests.Art;

/// <summary>Genomes are the artifact LLM agents author and the pipeline commits to disk (see ADR 0006), so their JSON round-trip must be lossless.</summary>
public class GenomeJsonTests
{
    [Fact]
    public void RoundTrip_PreservesAllFields()
    {
        var genome = TestGenomes.ValidLightHauler();

        var json = GenomeJson.ToJsonString(genome);
        var parsed = GenomeJson.Parse(json);

        Assert.Equal(genome.Id, parsed.Id);
        Assert.Equal(genome.Class, parsed.Class);
        Assert.Equal(genome.FactionId, parsed.FactionId);
        Assert.Equal(genome.Epoch, parsed.Epoch);
        Assert.Equal(genome.Seed, parsed.Seed);
        Assert.Equal(genome.Canvas, parsed.Canvas);
        Assert.Equal(genome.Silhouette.AsymmetryBudget, parsed.Silhouette.AsymmetryBudget);
        Assert.Equal(genome.Silhouette.Spine, parsed.Silhouette.Spine);
        Assert.Equal(genome.Modules, parsed.Modules);
        Assert.Equal(genome.Greebles, parsed.Greebles);
        Assert.Equal(genome.Zones, parsed.Zones);
        Assert.Equal(genome.Wear, parsed.Wear);
    }

    [Fact]
    public void ToJsonString_UsesSnakeCaseZoneKeys()
    {
        var genome = TestGenomes.ValidLightHauler();

        var json = GenomeJson.ToJsonString(genome);

        Assert.Contains("\"hull_shadow\"", json);
        Assert.Contains("\"hull_light\"", json);
    }

    [Fact]
    public void Parse_RejectsUnknownShipClass()
    {
        var genome = TestGenomes.ValidLightHauler();
        var json = GenomeJson.ToJsonString(genome).Replace("\"LightHauler\"", "\"SuperHauler\"");

        Assert.Throws<FormatException>(() => GenomeJson.Parse(json));
    }

    [Fact]
    public void Parse_RejectsUnknownZoneKey()
    {
        var genome = TestGenomes.ValidLightHauler();
        var json = GenomeJson.ToJsonString(genome).Replace("\"hull_shadow\"", "\"hull_glow\"");

        Assert.Throws<FormatException>(() => GenomeJson.Parse(json));
    }
}
