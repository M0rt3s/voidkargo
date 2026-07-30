using Game.Shared.GameLogic;

namespace Game.Shared.Tests;

public class TrainMovementTests
{
    [Fact]
    public void AdvanceProgress_HalfwayThroughTravelTime_ReturnsHalfProgress()
    {
        var result = TrainMovement.AdvanceProgress(
            currentProgress: 0.0,
            elapsed: TimeSpan.FromMinutes(5),
            totalTravelTime: TimeSpan.FromMinutes(10));

        Assert.Equal(0.5, result, precision: 5);
    }

    [Fact]
    public void AdvanceProgress_ElapsedExceedsTravelTime_ClampsToOne()
    {
        var result = TrainMovement.AdvanceProgress(
            currentProgress: 0.9,
            elapsed: TimeSpan.FromMinutes(30),
            totalTravelTime: TimeSpan.FromMinutes(10));

        Assert.Equal(1.0, result);
    }

    [Fact]
    public void AdvanceProgress_ZeroTravelTime_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            TrainMovement.AdvanceProgress(0.0, TimeSpan.FromMinutes(1), TimeSpan.Zero));
    }
}
