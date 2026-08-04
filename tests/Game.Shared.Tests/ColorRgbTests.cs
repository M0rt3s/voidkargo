using Game.Shared.Design;

namespace Game.Shared.Tests;

/// <summary>
/// <see cref="ColorRgb"/> is the parsing entry point for the design tokens that the website and
/// the Unity client both consume, so its hex handling is worth pinning down.
/// </summary>
public class ColorRgbTests
{
    [Theory]
    [InlineData("#E08A1E", 224, 138, 30, 255)]
    [InlineData("E08A1E", 224, 138, 30, 255)]
    [InlineData("#e08a1e", 224, 138, 30, 255)]
    [InlineData("#FFF", 255, 255, 255, 255)]
    [InlineData("#06080A80", 6, 8, 10, 128)]
    public void FromHex_ParsesSupportedFormats(string hex, byte r, byte g, byte b, byte a)
    {
        var color = ColorRgb.FromHex(hex);

        Assert.Equal(new ColorRgb(r, g, b, a), color);
    }

    [Theory]
    [InlineData("#FFFF")]
    [InlineData("#GGGGGG")]
    [InlineData("not-a-colour")]
    public void FromHex_RejectsMalformedInput(string hex)
    {
        Assert.ThrowsAny<FormatException>(() => ColorRgb.FromHex(hex));
    }

    [Fact]
    public void FromHex_RejectsEmptyInput()
    {
        Assert.Throws<ArgumentException>(() => ColorRgb.FromHex("  "));
    }

    [Fact]
    public void ToHex_OmitsAlphaWhenOpaque()
    {
        Assert.Equal("#E08A1E", new ColorRgb(224, 138, 30).ToHex());
        Assert.Equal("#E08A1E80", new ColorRgb(224, 138, 30, 128).ToHex());
    }

    [Fact]
    public void ToRgbTriplet_MatchesBootstrapVariableFormat()
    {
        Assert.Equal("224, 138, 30", VoidKargoPalette.Accent.ToRgbTriplet());
    }

    [Fact]
    public void SemanticAliases_ResolveToRampValues()
    {
        // Guards against an alias silently drifting away from the ramp it documents.
        Assert.Equal(VoidKargoPalette.Void900, VoidKargoPalette.Background);
        Assert.Equal(VoidKargoPalette.Ember500, VoidKargoPalette.Accent);
        Assert.Equal(VoidKargoPalette.Bone100, VoidKargoPalette.Text);
    }
}
