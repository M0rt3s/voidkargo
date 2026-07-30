namespace Game.Shared.GameLogic;

/// <summary>
/// Pure game-math helpers shared between the authoritative server simulation
/// (Game.Backend) and the client's optimistic/predictive rendering (Game.Client).
/// Keep this dependency-free and deterministic so both sides always agree.
/// </summary>
public static class TrainMovement
{
    /// <summary>
    /// Computes the new progress (0.0-1.0) of a train along its route after
    /// <paramref name="elapsed"/> has passed, given the route's total travel time.
    /// </summary>
    public static double AdvanceProgress(double currentProgress, TimeSpan elapsed, TimeSpan totalTravelTime)
    {
        if (totalTravelTime <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(totalTravelTime), "Travel time must be positive.");
        }

        var delta = elapsed.TotalSeconds / totalTravelTime.TotalSeconds;
        return Math.Clamp(currentProgress + delta, 0.0, 1.0);
    }
}
