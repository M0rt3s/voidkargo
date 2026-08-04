using Game.Shared.Art.Canvas;
using Game.Shared.Art.Encoding;
using Game.Shared.Tests.Art.TestSupport;

namespace Game.Shared.Tests.Art;

/// <summary>
/// Round-trips <see cref="PngEncoder"/> output through a minimal test-only decoder, verifying
/// pixel *content* is preserved exactly - not raw file bytes, since two different DEFLATE
/// implementations can compress the same bytes differently (see the determinism remark on
/// <see cref="PngEncoder"/>).
/// </summary>
public class PngEncoderTests
{
    [Fact]
    public void Encode_ProducesAValidPngSignature()
    {
        var canvas = new IndexedCanvas(2, 2);
        var png = PngEncoder.Encode(canvas);

        byte[] expectedSignature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        Assert.Equal(expectedSignature, png.Take(8));
    }

    [Fact]
    public void RoundTrip_PreservesEveryPlaneExactly()
    {
        var canvas = new IndexedCanvas(5, 3);
        var rng = new Random(1);
        for (var y = 0; y < canvas.Height; y++)
        {
            for (var x = 0; x < canvas.Width; x++)
            {
                canvas.SetIndex(x, y, (byte)rng.Next(0, 16));
                canvas.SetGlow(x, y, (byte)rng.Next(0, 256));
                canvas.SetAlpha(x, y, (byte)(rng.Next(0, 2) * 255));
            }
        }

        var png = PngEncoder.Encode(canvas);
        var decoded = MinimalPngDecoder.Decode(png);

        Assert.Equal(canvas.Width, decoded.Width);
        Assert.Equal(canvas.Height, decoded.Height);

        for (var y = 0; y < canvas.Height; y++)
        {
            for (var x = 0; x < canvas.Width; x++)
            {
                var pixelIndex = y * canvas.Width + x;
                Assert.Equal(canvas.GetIndex(x, y), decoded.IndexPlane[pixelIndex]);
                Assert.Equal(canvas.GetGlow(x, y), decoded.GlowPlane[pixelIndex]);
                Assert.Equal(canvas.GetAlpha(x, y), decoded.AlphaPlane[pixelIndex]);
            }
        }
    }

    [Fact]
    public void Encode_IsDeterministicForIdenticalInput()
    {
        var canvasA = new IndexedCanvas(4, 4);
        var canvasB = new IndexedCanvas(4, 4);
        canvasA.Paint(1, 1, 5);
        canvasB.Paint(1, 1, 5);

        var pngA = PngEncoder.Encode(canvasA);
        var pngB = PngEncoder.Encode(canvasB);

        Assert.Equal(pngA, pngB);
    }
}
