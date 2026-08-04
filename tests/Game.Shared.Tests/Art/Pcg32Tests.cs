using Game.Shared.Art.Rng;

namespace Game.Shared.Tests.Art;

/// <summary>
/// <see cref="Pcg32"/> is the deterministic RNG the whole art pipeline depends on for
/// reproducible output (see ADR 0006) - if this drifts, every downstream genome->pixel guarantee
/// breaks, so its determinism is worth pinning down directly.
/// </summary>
public class Pcg32Tests
{
    [Fact]
    public void SameSeed_ProducesIdenticalSequence()
    {
        var a = new Pcg32(42);
        var b = new Pcg32(42);

        for (var i = 0; i < 100; i++)
        {
            Assert.Equal(a.NextUInt32(), b.NextUInt32());
        }
    }

    [Fact]
    public void DifferentSeeds_ProduceDifferentSequences()
    {
        var a = new Pcg32(1);
        var b = new Pcg32(2);

        var aValues = new uint[10];
        var bValues = new uint[10];
        for (var i = 0; i < 10; i++)
        {
            aValues[i] = a.NextUInt32();
            bValues[i] = b.NextUInt32();
        }

        Assert.NotEqual(aValues, bValues);
    }

    [Fact]
    public void NextDouble_StaysWithinUnitRange()
    {
        var rng = new Pcg32(7);
        for (var i = 0; i < 1000; i++)
        {
            var value = rng.NextDouble();
            Assert.InRange(value, 0.0, 1.0 - double.Epsilon);
        }
    }

    [Fact]
    public void NextInt_StaysWithinRequestedRange()
    {
        var rng = new Pcg32(99);
        for (var i = 0; i < 1000; i++)
        {
            var value = rng.NextInt(5, 10);
            Assert.InRange(value, 5, 9);
        }
    }

    [Fact]
    public void HashToUnit_IsDeterministicForSamePosition()
    {
        var first = Pcg32.HashToUnit(123, 4, 5);
        var second = Pcg32.HashToUnit(123, 4, 5);

        Assert.Equal(first, second);
    }

    [Fact]
    public void HashToUnit_DiffersAcrossPositions()
    {
        var a = Pcg32.HashToUnit(123, 4, 5);
        var b = Pcg32.HashToUnit(123, 4, 6);
        var c = Pcg32.HashToUnit(123, 5, 5);

        Assert.NotEqual(a, b);
        Assert.NotEqual(a, c);
    }

    [Fact]
    public void HashToUnit_StaysWithinUnitRange()
    {
        for (var x = 0; x < 20; x++)
        {
            for (var y = 0; y < 20; y++)
            {
                var value = Pcg32.HashToUnit(555, x, y);
                Assert.InRange(value, 0.0, 1.0);
            }
        }
    }
}
