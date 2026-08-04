// Explicit usings and a block-scoped namespace are used here (instead of relying on
// ImplicitUsings/file-scoped namespaces) because Unity compiles this file directly as part of
// the com.voidkargo.shared local package, ignoring this project's .csproj SDK settings, and
// Unity's compiler is pinned to C# 9.0 (file-scoped namespaces need C# 10+).
namespace Game.Shared.Design
{
    /// <summary>
    /// The canonical "Void &amp; Ember" colour palette — the single source of truth for both the
    /// Blazor website (<c>wwwroot/css/tokens.css</c>) and the Unity client. If a value changes here
    /// it must change in <c>tokens.css</c> in the same commit; see
    /// <c>docs/05-design/design-tokens.md</c>.
    /// </summary>
    /// <remarks>
    /// Ramps are named low-to-high the way ink is: a higher number on <c>Void</c> means darker,
    /// a higher number on <c>Bone</c>/<c>Steel</c>/accents means lighter. Semantic aliases at the
    /// bottom are what UI code should reference — reach for a raw ramp value only when defining a
    /// new semantic token.
    /// </remarks>
    public static class VoidKargoPalette
    {
        // ---- Void: backgrounds. Never pure black — the void has depth, not absence. ----------
        public static readonly ColorRgb Void900 = ColorRgb.FromHex("#06080A");
        public static readonly ColorRgb Void800 = ColorRgb.FromHex("#0A0D11");
        public static readonly ColorRgb Void700 = ColorRgb.FromHex("#0F141A");
        public static readonly ColorRgb Void600 = ColorRgb.FromHex("#151B23");
        public static readonly ColorRgb Void500 = ColorRgb.FromHex("#1D242D");

        // ---- Steel: hairlines, dividers, de-emphasised text. -------------------------------
        public static readonly ColorRgb Steel400 = ColorRgb.FromHex("#232C36");
        public static readonly ColorRgb Steel300 = ColorRgb.FromHex("#33404E");
        public static readonly ColorRgb Steel200 = ColorRgb.FromHex("#6C7A8A");
        public static readonly ColorRgb Steel100 = ColorRgb.FromHex("#9BA8B6");

        // ---- Bone: type. Off-white, never #FFF — pure white glares on a dark field. ---------
        public static readonly ColorRgb Bone100 = ColorRgb.FromHex("#DDE3E9");
        public static readonly ColorRgb Bone000 = ColorRgb.FromHex("#F2F5F7");

        // ---- Ember: the one accent. Instrument backlight / cargo-hazard ochre. --------------
        public static readonly ColorRgb Ember600 = ColorRgb.FromHex("#8A4F0C");
        public static readonly ColorRgb Ember500 = ColorRgb.FromHex("#E08A1E");
        public static readonly ColorRgb Ember400 = ColorRgb.FromHex("#F0A73C");
        public static readonly ColorRgb Ember300 = ColorRgb.FromHex("#FFC66B");

        // ---- Signals. Used only to carry meaning, never for decoration. ---------------------

        /// <summary>Cold steel-blue. Informational, telemetry, links inside dense data.</summary>
        public static readonly ColorRgb Frost500 = ColorRgb.FromHex("#58A6C9");

        /// <summary>Sober green. Operational / nominal / success.</summary>
        public static readonly ColorRgb Moss500 = ColorRgb.FromHex("#3F8F63");

        /// <summary>Dull brass. Degraded / warning — distinct from the ember accent.</summary>
        public static readonly ColorRgb Sulfur500 = ColorRgb.FromHex("#C9A227");

        /// <summary>Rushnyk red, borrowed from Slavic embroidery. Failure / destructive only.</summary>
        public static readonly ColorRgb Rushnyk500 = ColorRgb.FromHex("#C8352A");

        // ---- Semantic aliases: what UI code should actually use. ---------------------------
        public static ColorRgb Background => Void900;
        public static ColorRgb Surface => Void700;
        public static ColorRgb SurfaceRaised => Void600;
        public static ColorRgb SurfaceInset => Void800;
        public static ColorRgb Border => Steel400;
        public static ColorRgb BorderStrong => Steel300;
        public static ColorRgb Text => Bone100;
        public static ColorRgb TextStrong => Bone000;
        public static ColorRgb TextMuted => Steel100;
        public static ColorRgb TextDim => Steel200;
        public static ColorRgb Accent => Ember500;
        public static ColorRgb AccentHover => Ember400;
        public static ColorRgb AccentPressed => Ember600;
        public static ColorRgb Focus => Ember300;
        public static ColorRgb Info => Frost500;
        public static ColorRgb Success => Moss500;
        public static ColorRgb Warning => Sulfur500;
        public static ColorRgb Danger => Rushnyk500;
    }
}
