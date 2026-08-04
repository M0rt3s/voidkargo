using Game.Shared.Art.Palette;

namespace Game.Shared.Tests.Art;

/// <summary>
/// <see cref="PaletteValidator"/> enforces this project's accessibility baseline (contrast
/// against the game's dark backdrop, separability under simulated colour-blindness) - see ADR
/// 0006. These tests use small, deliberately constructed palettes rather than a real faction
/// palette so each failure mode is isolated.
/// </summary>
public class PaletteValidatorTests
{
    private static Palette MakePalette(string id, params (byte R, byte G, byte B)[] colors)
    {
        Assert.Equal(Palette.ColorCount, colors.Length);
        var rgb = new List<RgbColor>();
        foreach (var (r, g, b) in colors)
        {
            rgb.Add(new RgbColor(r, g, b));
        }

        return new Palette(id, rgb);
    }

    [Fact]
    public void Validate_AcceptsAWellSpreadHighContrastPalette()
    {
        // Sixteen colours chosen by a greedy farthest-point search (offline, not part of the
        // product code) that maximizes the minimum pairwise distance across the *normal* view
        // and all three simulated dichromacy views simultaneously, while also satisfying the
        // contrast floor - i.e. an existence proof that a real 16-colour palette can pass every
        // check at once, not just a hand-picked "looks fine" guess.
        var palette = MakePalette(
            "accessible-test",
            (125, 114, 71), (255, 255, 249), (13, 255, 255), (254, 238, 50),
            (1, 92, 192), (153, 226, 164), (0, 114, 12), (253, 86, 2),
            (88, 119, 185), (189, 11, 4), (176, 204, 66), (46, 102, 106),
            (117, 249, 252), (253, 217, 161), (11, 145, 251), (189, 250, 218));

        var result = PaletteValidator.Validate(palette);

        Assert.True(result.IsValid, string.Join("; ", result.Errors));
    }

    [Fact]
    public void Validate_RejectsPaletteWithLowContrastDarkColors()
    {
        var colors = new (byte, byte, byte)[Palette.ColorCount];
        for (var i = 0; i < colors.Length; i++)
        {
            // Every colour is a near-black navy, well below the contrast floor against the dark backdrop.
            colors[i] = ((byte)(10 + i), (byte)(10 + i), (byte)(20 + i));
        }

        var palette = MakePalette("too-dark", colors);

        var result = PaletteValidator.Validate(palette);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("contrast ratio"));
    }

    [Fact]
    public void Validate_RejectsPaletteWithNearDuplicateColors()
    {
        var colors = new (byte, byte, byte)[Palette.ColorCount];
        for (var i = 0; i < colors.Length; i++)
        {
            // All 16 slots are the same bright colour (contrast is fine, but nothing is separable from anything else).
            colors[i] = (200, 200, 60);
        }

        var palette = MakePalette("all-identical", colors);

        var result = PaletteValidator.Validate(palette);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("indistinguishable"));
    }

    [Fact]
    public void Validate_ThrowsWhenGivenTheWrongNumberOfColors()
    {
        var tooFew = Enumerable.Range(0, 4).Select(i => new RgbColor((byte)i, (byte)i, (byte)i)).ToList();

        Assert.Throws<ArgumentException>(() => new Palette("too-few", tooFew));
    }
}
