// Explicit usings and a block-scoped namespace are used here (instead of relying on
// ImplicitUsings/file-scoped namespaces) because Unity compiles this file directly as part of
// the com.voidkargo.shared local package, ignoring this project's .csproj SDK settings, and
// Unity's compiler is pinned to C# 9.0 (file-scoped namespaces need C# 10+).
namespace Game.Shared.Design
{
    /// <summary>
    /// Canonical spacing, radii, borders, motion and layout metrics. Pixel values; the web layer
    /// converts to rem where appropriate.
    /// </summary>
    public static class VoidKargoMetrics
    {
        /// <summary>Spacing scale, 4px base. Nothing in the UI sits off this grid.</summary>
        public static class Space
        {
            public const double S1 = 4;
            public const double S2 = 8;
            public const double S3 = 12;
            public const double S4 = 16;
            public const double S5 = 24;
            public const double S6 = 32;
            public const double S7 = 48;
            public const double S8 = 64;
        }

        /// <summary>
        /// Radii. Deliberately near-zero: the language is machined and cut, not soft. 4px is the
        /// hard ceiling and is reserved for pill-shaped badges.
        /// </summary>
        public static class Radius
        {
            public const double None = 0;
            public const double Sharp = 2;
            public const double Soft = 4;
        }

        /// <summary>Hairline border width. One pixel, always.</summary>
        public const double BorderWidth = 1;

        /// <summary>Motion durations in milliseconds. Nothing animates longer than Slow.</summary>
        public static class Duration
        {
            public const int Instant = 90;
            public const int Fast = 140;
            public const int Slow = 200;
        }

        /// <summary>Fixed layout metrics shared by website chrome and in-game HUD.</summary>
        public static class Layout
        {
            public const double TopBarHeight = 56;
            public const double SideBarWidth = 260;
            public const double ContentMaxWidth = 1320;
        }
    }
}
