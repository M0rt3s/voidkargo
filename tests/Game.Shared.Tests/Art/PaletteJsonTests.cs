using Game.Shared.Art.Palette;

namespace Game.Shared.Tests.Art;

public class PaletteJsonTests
{
    [Fact]
    public void RoundTrip_PreservesIdAndAllSixteenColors()
    {
        var colors = Enumerable.Range(0, 16).Select(i => new RgbColor((byte)(i * 15), (byte)(255 - i * 10), (byte)(i * 5))).ToList();
        var palette = new Palette("round-trip-test", colors);

        var json = PaletteJson.ToJsonString(palette);
        var parsed = PaletteJson.Parse(json);

        Assert.Equal(palette.Id, parsed.Id);
        for (var i = 0; i < Palette.ColorCount; i++)
        {
            Assert.Equal(palette[i].R, parsed[i].R);
            Assert.Equal(palette[i].G, parsed[i].G);
            Assert.Equal(palette[i].B, parsed[i].B);
        }
    }
}
