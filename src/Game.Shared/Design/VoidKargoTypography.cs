// Explicit usings and a block-scoped namespace are used here (instead of relying on
// ImplicitUsings/file-scoped namespaces) because Unity compiles this file directly as part of
// the com.voidkargo.shared local package, ignoring this project's .csproj SDK settings, and
// Unity's compiler is pinned to C# 9.0 (file-scoped namespaces need C# 10+).
namespace Game.Shared.Design
{
    /// <summary>
    /// Canonical typography tokens, mirrored by <c>wwwroot/css/tokens.css</c> and by the Unity
    /// client. See <c>docs/05-design/design-tokens.md</c>. Spacing/radii/motion metrics live in
    /// <see cref="VoidKargoMetrics"/>.
    /// </summary>
    public static class VoidKargoTypography
    {
        /// <summary>Display face. Geometric, Cyrillic-native — headings and the wordmark only.</summary>
        public const string FontDisplay = "Unbounded";

        /// <summary>Interface face. Everything that is read as prose or a control label.</summary>
        public const string FontUi = "Inter";

        /// <summary>
        /// Monospace face. Every number, ID, coordinate, timer and code — tabular figures are what
        /// makes the UI read as instrumentation rather than as a brochure.
        /// </summary>
        public const string FontMono = "JetBrains Mono";

        /// <summary>
        /// Type scale, in pixels, ratio ~1.25 (major third). Web divides by 16 to get rem.
        /// Steps below 400 are UI chrome; 400 is body; 500 and up are display sizes.
        /// </summary>
        public static class Size
        {
            public const double Step100 = 11;
            public const double Step200 = 12;
            public const double Step300 = 14;
            public const double Step400 = 16;
            public const double Step500 = 20;
            public const double Step600 = 25;
            public const double Step700 = 31;
            public const double Step800 = 39;
            public const double Step900 = 49;
        }

        /// <summary>Letter-spacing in em.</summary>
        public static class Tracking
        {
            /// <summary>Display headings: tight, so large type reads as one solid mass.</summary>
            public const double Display = -0.02;

            public const double Normal = 0;

            /// <summary>Small uppercase labels ("eyebrows"), the signature HUD texture.</summary>
            public const double Label = 0.14;
        }
    }
}
