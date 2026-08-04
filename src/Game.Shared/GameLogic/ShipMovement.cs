// Explicit `using System;` and a block-scoped namespace are used here (instead of relying on
// ImplicitUsings/file-scoped namespaces) because Unity compiles this file directly as part of
// the com.voidkargo.shared local package, ignoring this project's .csproj SDK settings, and
// Unity's compiler is pinned to C# 9.0 (file-scoped namespaces need C# 10+).
using System;

namespace Game.Shared.GameLogic
{
    /// <summary>
    /// Pure game-math helpers shared between the authoritative server simulation
    /// (Game.Backend) and the client's optimistic/predictive rendering (Game.Client).
    /// Keep this dependency-free and deterministic so both sides always agree.
    /// </summary>
    public static class ShipMovement
    {
        /// <summary>
        /// Computes the new progress (0.0-1.0) of a ship along its current hop after
        /// <paramref name="elapsed"/> has passed, given the hop's total travel time.
        /// There is no track/route to lay - a ship's hop distance is one of its stats
        /// (see <see cref="Game.Shared.Dtos.ShipTypeDto"/>), so this only advances a
        /// linear progress fraction between two Nodes.
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
}
