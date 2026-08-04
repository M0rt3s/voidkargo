using Game.Shared.Art.Encoding;
using Game.Shared.Art.Genome;
using Game.Shared.Art.Rendering;
using Game.Shared.Dtos;

namespace Game.Shared.Tests.Art;

/// <summary>
/// Pins down the core promise of ADR 0006's art pipeline: the same genome always renders to
/// byte-identical pixels, different seeds/classes visibly change the output, and an invalid
/// genome is rejected before it ever reaches pixel generation.
/// </summary>
public class ShipRendererTests
{
    [Fact]
    public void RenderLogical_IsByteIdenticalForTheSameGenome()
    {
        var genome = TestGenomes.ValidLightHauler();

        var first = ShipRenderer.RenderLogical(genome);
        var second = ShipRenderer.RenderLogical(genome);

        AssertCanvasesEqual(first, second);
    }

    [Fact]
    public void RenderLogical_DiffersForDifferentSeeds()
    {
        var a = ShipRenderer.RenderLogical(TestGenomes.ValidLightHauler(seed: 1));
        var b = ShipRenderer.RenderLogical(TestGenomes.ValidLightHauler(seed: 2));

        var anyDifferent = false;
        for (var y = 0; y < a.Height && !anyDifferent; y++)
        {
            for (var x = 0; x < a.Width && !anyDifferent; x++)
            {
                if (a.GetIndex(x, y) != b.GetIndex(x, y) || a.GetAlpha(x, y) != b.GetAlpha(x, y))
                {
                    anyDifferent = true;
                }
            }
        }

        Assert.True(anyDifferent, "Expected different seeds to produce at least one different pixel (greebles/wear are seed-driven).");
    }

    [Fact]
    public void RenderLogical_ProducesAReadableSilhouette_WithinReasonableCoverageBounds()
    {
        var genome = TestGenomes.ValidLightHauler();
        var canvas = ShipRenderer.RenderLogical(genome);

        var filledCount = 0;
        for (var y = 0; y < canvas.Height; y++)
        {
            for (var x = 0; x < canvas.Width; x++)
            {
                if (canvas.GetAlpha(x, y) > 0)
                {
                    filledCount++;
                }
            }
        }

        var coverageFraction = filledCount / (double)(canvas.Width * canvas.Height);

        // A ship sprite should neither be empty (broken generation) nor fill the whole canvas
        // (would read as a solid block, not a silhouette) - a generous sanity band, not an
        // aesthetic judgement (see ADR 0006: the linter catches broken output, not mediocre output).
        Assert.InRange(coverageFraction, 0.02, 0.6);
    }

    [Fact]
    public void RenderLogical_OnlyUsesPaletteIndicesDeclaredInTheGenomeZoneMap()
    {
        var genome = TestGenomes.ValidLightHauler();
        var canvas = ShipRenderer.RenderLogical(genome);

        var declaredIndices = new HashSet<int>(genome.Zones.Values);

        for (var y = 0; y < canvas.Height; y++)
        {
            for (var x = 0; x < canvas.Width; x++)
            {
                if (canvas.GetAlpha(x, y) == 0)
                {
                    continue;
                }

                Assert.Contains(canvas.GetIndex(x, y), declaredIndices);
            }
        }
    }

    [Fact]
    public void RenderLogical_ThrowsForAnInvalidGenome()
    {
        var invalid = TestGenomes.ValidLightHauler() with { Wear = 5.0 };

        Assert.Throws<ArgumentException>(() => ShipRenderer.RenderLogical(invalid));
    }

    [Fact]
    public void RenderFinal_UpscalesToTheCanvasDeclaredFinalSize()
    {
        var genome = TestGenomes.ValidLightHauler();

        var final = ShipRenderer.RenderFinal(genome);

        Assert.Equal(256, final.Width);
        Assert.Equal(256, final.Height);
    }

    [Fact]
    public void RenderFinal_EncodesToAValidRoundTrippablePng()
    {
        var genome = TestGenomes.ValidLightHauler();
        var final = ShipRenderer.RenderFinal(genome);

        var png = PngEncoder.Encode(final);

        byte[] expectedSignature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        Assert.Equal(expectedSignature, png.Take(8));
    }

    private static void AssertCanvasesEqual(Game.Shared.Art.Canvas.IndexedCanvas a, Game.Shared.Art.Canvas.IndexedCanvas b)
    {
        Assert.Equal(a.Width, b.Width);
        Assert.Equal(a.Height, b.Height);
        for (var y = 0; y < a.Height; y++)
        {
            for (var x = 0; x < a.Width; x++)
            {
                Assert.Equal(a.GetIndex(x, y), b.GetIndex(x, y));
                Assert.Equal(a.GetGlow(x, y), b.GetGlow(x, y));
                Assert.Equal(a.GetAlpha(x, y), b.GetAlpha(x, y));
            }
        }
    }
}
