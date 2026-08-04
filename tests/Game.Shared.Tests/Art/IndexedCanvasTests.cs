using Game.Shared.Art.Canvas;

namespace Game.Shared.Tests.Art;

public class IndexedCanvasTests
{
    [Fact]
    public void PaintAndErase_RoundTripThroughAllPlanes()
    {
        var canvas = new IndexedCanvas(4, 4);

        canvas.Paint(1, 2, 7);
        canvas.SetGlow(1, 2, 200);

        Assert.Equal(7, canvas.GetIndex(1, 2));
        Assert.Equal(255, canvas.GetAlpha(1, 2));
        Assert.Equal(200, canvas.GetGlow(1, 2));

        canvas.Erase(1, 2);

        Assert.Equal(0, canvas.GetIndex(1, 2));
        Assert.Equal(0, canvas.GetAlpha(1, 2));
        Assert.Equal(0, canvas.GetGlow(1, 2));
    }

    [Fact]
    public void OutOfBoundsAccess_Throws()
    {
        var canvas = new IndexedCanvas(4, 4);

        Assert.Throws<ArgumentOutOfRangeException>(() => canvas.GetIndex(4, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => canvas.GetIndex(0, -1));
    }

    [Fact]
    public void Constructor_RejectsNonPositiveDimensions()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new IndexedCanvas(0, 4));
        Assert.Throws<ArgumentOutOfRangeException>(() => new IndexedCanvas(4, -1));
    }

    [Fact]
    public void UpscaleNearestNeighbor_ReplicatesEachPixelIntoABlock()
    {
        var canvas = new IndexedCanvas(2, 2);
        canvas.Paint(0, 0, 3);
        canvas.SetGlow(0, 0, 50);
        canvas.Paint(1, 1, 9);

        var upscaled = canvas.UpscaleNearestNeighbor(2);

        Assert.Equal(4, upscaled.Width);
        Assert.Equal(4, upscaled.Height);

        // The 2x2 block replicated from source pixel (0, 0) should be uniform.
        for (var y = 0; y < 2; y++)
        {
            for (var x = 0; x < 2; x++)
            {
                Assert.Equal(3, upscaled.GetIndex(x, y));
                Assert.Equal(255, upscaled.GetAlpha(x, y));
                Assert.Equal(50, upscaled.GetGlow(x, y));
            }
        }

        // The 2x2 block replicated from source pixel (1, 1) should carry that pixel's index/alpha.
        for (var y = 2; y < 4; y++)
        {
            for (var x = 2; x < 4; x++)
            {
                Assert.Equal(9, upscaled.GetIndex(x, y));
                Assert.Equal(255, upscaled.GetAlpha(x, y));
            }
        }

        // A source pixel that was never painted stays fully transparent after upscaling.
        Assert.Equal(0, upscaled.GetAlpha(2, 0));
    }
}
