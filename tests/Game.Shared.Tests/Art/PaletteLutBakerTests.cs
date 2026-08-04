using Game.Shared.Art.Palette;
using Game.Shared.Tests.Art.TestSupport;

namespace Game.Shared.Tests.Art;

/// <summary>
/// <see cref="PaletteLutBaker"/> is what turns a set of faction/cosmetic palettes into the
/// single combined lookup texture a URP shader samples at render time (see ADR 0006) - its row
/// layout is the contract the future shader phase depends on, so it's worth pinning down here.
/// </summary>
public class PaletteLutBakerTests
{
    private static Palette MakeSolidPalette(string id, byte r, byte g, byte b)
    {
        var colors = Enumerable.Range(0, Palette.ColorCount).Select(_ => new RgbColor(r, g, b)).ToList();
        return new Palette(id, colors);
    }

    [Fact]
    public void BuildRgbaBuffer_LaysOutOnePaletteRowPerPalette()
    {
        var red = MakeSolidPalette("red", 255, 0, 0);
        var blue = MakeSolidPalette("blue", 0, 0, 255);

        var buffer = PaletteLutBaker.BuildRgbaBuffer(new[] { red, blue });

        Assert.Equal(Palette.ColorCount * 2 * 4, buffer.Length);

        // Row 0 (red) - check first and last column.
        Assert.Equal(255, buffer[0]); // R of column 0
        Assert.Equal(0, buffer[1]); // G
        Assert.Equal(0, buffer[2]); // B
        Assert.Equal(255, buffer[3]); // A

        // Row 1 (blue) starts at offset ColorCount * 4.
        var row1Offset = Palette.ColorCount * 4;
        Assert.Equal(0, buffer[row1Offset]);
        Assert.Equal(0, buffer[row1Offset + 1]);
        Assert.Equal(255, buffer[row1Offset + 2]);
        Assert.Equal(255, buffer[row1Offset + 3]);
    }

    [Fact]
    public void BuildPng_RoundTripsThroughTheMinimalDecoder()
    {
        var red = MakeSolidPalette("red", 255, 0, 0);
        var green = MakeSolidPalette("green", 0, 255, 0);

        var png = PaletteLutBaker.BuildPng(new[] { red, green });
        var decoded = MinimalPngDecoder.Decode(png);

        Assert.Equal(Palette.ColorCount, decoded.Width);
        Assert.Equal(2, decoded.Height);

        // Row 0 = red palette: R plane (which the decoder calls "IndexPlane" for the ship
        // format, but here is genuinely the R channel) should be 255 across the whole row.
        for (var x = 0; x < Palette.ColorCount; x++)
        {
            Assert.Equal(255, decoded.IndexPlane[x]);
            Assert.Equal(0, decoded.GlowPlane[x]);
        }

        // Row 1 = green palette: R channel should be 0, G channel (decoder's "GlowPlane") should be 255.
        for (var x = 0; x < Palette.ColorCount; x++)
        {
            var pixelIndex = Palette.ColorCount + x;
            Assert.Equal(0, decoded.IndexPlane[pixelIndex]);
            Assert.Equal(255, decoded.GlowPlane[pixelIndex]);
        }
    }
}
