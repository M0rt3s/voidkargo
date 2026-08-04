// Explicit `using System;` and a block-scoped namespace are used here (instead of relying on
// ImplicitUsings/file-scoped namespaces) because Unity compiles this file directly as part of
// the com.voidkargo.shared local package, ignoring this project's .csproj SDK settings, and
// Unity's compiler is pinned to C# 9.0 (file-scoped namespaces need C# 10+).
using System;

namespace Game.Shared.Art.Rng
{
    /// <summary>
    /// A small, dependency-free, deterministic PRNG (PCG XSH-RR 32-bit, one of the "PCG32"
    /// family: https://www.pcg-random.org/). Used everywhere in the art pipeline instead of
    /// <see cref="System.Random"/>, whose exact output sequence is not documented/guaranteed
    /// stable across .NET versions or between the .NET runtime and Unity's Mono/IL2CPP
    /// runtime. The same seed must always produce the same sequence on both, because the same
    /// genome must render to byte-identical art wherever it's baked (see ADR 0006).
    /// </summary>
    public sealed class Pcg32
    {
        // Fixed, arbitrary odd increment (must be odd for PCG's LCG stream to have full period).
        // Value borrowed from the reference PCG implementation's default stream.
        private const ulong Increment = 1442695040888963407UL;
        private const ulong Multiplier = 6364136223846793005UL;

        private ulong _state;

        public Pcg32(ulong seed)
        {
            _state = 0UL;
            NextUInt32(); // advance once per PCG init convention
            _state += seed;
            NextUInt32();
        }

        /// <summary>Returns the next pseudo-random 32-bit unsigned integer in the sequence.</summary>
        public uint NextUInt32()
        {
            var oldState = _state;
            _state = unchecked(oldState * Multiplier + Increment);

            // XSH-RR: xorshift high bits down, then rotate right by the top 5 bits.
            var xorShifted = (uint)(((oldState >> 18) ^ oldState) >> 27);
            var rotation = (int)(oldState >> 59);
            return (xorShifted >> rotation) | (xorShifted << ((-rotation) & 31));
        }

        /// <summary>Returns a pseudo-random double in [0.0, 1.0).</summary>
        public double NextDouble()
        {
            // 24 bits of precision is plenty here and keeps this exactly reproducible.
            return (NextUInt32() >> 8) / (double)(1 << 24);
        }

        /// <summary>Returns a pseudo-random integer in [minInclusive, maxExclusive).</summary>
        public int NextInt(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
            {
                throw new ArgumentOutOfRangeException(nameof(maxExclusive), "maxExclusive must be greater than minInclusive.");
            }

            var range = (uint)(maxExclusive - minInclusive);
            return minInclusive + (int)(NextUInt32() % range);
        }

        /// <summary>
        /// A deterministic per-pixel hash used by generation passes (e.g. greebles, wear) that
        /// need a pseudo-random-looking but order-independent value for a given (x, y): unlike
        /// drawing sequentially from a single stream, this gives the same result regardless of
        /// pixel iteration order, which keeps those passes trivially parallelizable/testable in
        /// isolation.
        /// </summary>
        public static double HashToUnit(ulong seed, int x, int y)
        {
            unchecked
            {
                var h = seed ^ 0x9E3779B97F4A7C15UL;
                h ^= (ulong)(uint)x * 0xBF58476D1CE4E5B9UL;
                h ^= (ulong)(uint)y * 0x94D049BB133111EBUL;
                h ^= h >> 33;
                h *= 0xFF51AFD7ED558CCDUL;
                h ^= h >> 33;
                h *= 0xC4CEB9FE1A85EC53UL;
                h ^= h >> 33;
                return (h >> 11) / (double)(1UL << 53);
            }
        }
    }
}
