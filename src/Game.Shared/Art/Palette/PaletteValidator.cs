// Explicit usings and a block-scoped namespace are used here (instead of relying on
// ImplicitUsings/file-scoped namespaces) because Unity compiles this file directly as part of
// the com.voidkargo.shared local package, ignoring this project's .csproj SDK settings, and
// Unity's compiler is pinned to C# 9.0 (file-scoped namespaces need C# 10+).
using System;
using System.Collections.Generic;
using Game.Shared.Art.Rendering;

namespace Game.Shared.Art.Palette
{
    /// <summary>
    /// Validates a <see cref="Palette"/> against the accessibility rules called for in ADR 0006:
    /// enough contrast against the game's dark backdrop to stay readable, and enough pairwise
    /// separability under simulated colour-blindness that swapping a faction/cosmetic palette
    /// can never make a hull silhouette unreadable for a colour-blind player. The colour-blindness
    /// simulation here is a deliberately simplified linear approximation (not a clinically exact
    /// model) - it's a pragmatic guard against "these two zone colours become identical", not a
    /// substitute for real accessibility testing with colour-blind players.
    /// </summary>
    public static class PaletteValidator
    {
        // A fixed dark space-background luminance to check contrast against, matching the game's
        // stated "dark but nice" art direction - this is deliberately darker than pure black to
        // approximate a starfield/nebula backdrop rather than a solid #000000 canvas.
        private static readonly RgbColor BackgroundColor = new RgbColor(8, 10, 14);

        // Minimum WCAG-style contrast ratio a palette colour must have against the background to
        // count as "used for readable detail" (roughly WCAG AA's large-text threshold of 3:1 -
        // chosen instead of the 4.5:1 text threshold because these are hull details, not text).
        private const double MinimumContrastRatio = 3.0;

        // Minimum Euclidean distance (in a 0..1 per-channel RGB cube) two *simulated*
        // colour-blind colours must keep between them - anything smaller is treated as
        // "collapsed" (i.e. indistinguishable) for that vision type. Tunable, not derived from a
        // formal standard.
        private const double MinimumSimulatedSeparation = 0.12;

        public static ValidationResult Validate(Palette palette)
        {
            var errors = new List<string>();

            ValidateContrast(palette, errors);
            ValidateColorBlindSeparability(palette, ColorBlindnessType.Protanopia, errors);
            ValidateColorBlindSeparability(palette, ColorBlindnessType.Deuteranopia, errors);
            ValidateColorBlindSeparability(palette, ColorBlindnessType.Tritanopia, errors);

            return errors.Count == 0 ? ValidationResult.Success : new ValidationResult(errors);
        }

        private static void ValidateContrast(Palette palette, List<string> errors)
        {
            var backgroundLuminance = BackgroundColor.RelativeLuminance();
            for (var i = 0; i < Palette.ColorCount; i++)
            {
                var color = palette[i];
                var luminance = color.RelativeLuminance();
                var ratio = (Math.Max(luminance, backgroundLuminance) + 0.05) / (Math.Min(luminance, backgroundLuminance) + 0.05);
                if (ratio < MinimumContrastRatio)
                {
                    errors.Add($"Palette '{palette.Id}' colour index {i} has contrast ratio {ratio:F2}:1 against the background, below the minimum {MinimumContrastRatio:F1}:1.");
                }
            }
        }

        private static void ValidateColorBlindSeparability(Palette palette, ColorBlindnessType type, List<string> errors)
        {
            var simulated = new (double R, double G, double B)[Palette.ColorCount];
            for (var i = 0; i < Palette.ColorCount; i++)
            {
                simulated[i] = SimulateColorBlindness(palette[i], type);
            }

            for (var i = 0; i < Palette.ColorCount; i++)
            {
                for (var j = i + 1; j < Palette.ColorCount; j++)
                {
                    var distance = EuclideanDistance(simulated[i], simulated[j]);
                    if (distance < MinimumSimulatedSeparation)
                    {
                        errors.Add(
                            $"Palette '{palette.Id}' colour indices {i} and {j} become nearly indistinguishable " +
                            $"under simulated {type} (distance {distance:F3}, minimum {MinimumSimulatedSeparation:F2}).");
                    }
                }
            }
        }

        private static double EuclideanDistance((double R, double G, double B) a, (double R, double G, double B) b)
        {
            var dr = a.R - b.R;
            var dg = a.G - b.G;
            var db = a.B - b.B;
            return Math.Sqrt(dr * dr + dg * dg + db * db);
        }

        private enum ColorBlindnessType
        {
            Protanopia,
            Deuteranopia,
            Tritanopia,
        }

        // Simplified linear RGB approximations of dichromacy (informed by the general shape of
        // published simulation matrices, e.g. Machado/Oliveira/Fairchild-style approaches) - see
        // the type-level remark above about these being pragmatic, not clinically exact.
        private static (double R, double G, double B) SimulateColorBlindness(RgbColor color, ColorBlindnessType type)
        {
            var r = color.R / 255.0;
            var g = color.G / 255.0;
            var b = color.B / 255.0;

            switch (type)
            {
                case ColorBlindnessType.Protanopia:
                    // Red-cone loss: red channel is reconstructed from green/blue.
                    return (0.567 * r + 0.433 * g + 0.0 * b, 0.558 * r + 0.442 * g + 0.0 * b, 0.0 * r + 0.242 * g + 0.758 * b);
                case ColorBlindnessType.Deuteranopia:
                    // Green-cone loss: green channel is reconstructed from red/blue.
                    return (0.625 * r + 0.375 * g + 0.0 * b, 0.7 * r + 0.3 * g + 0.0 * b, 0.0 * r + 0.3 * g + 0.7 * b);
                case ColorBlindnessType.Tritanopia:
                    // Blue-cone loss: blue channel is reconstructed from red/green.
                    return (0.95 * r + 0.05 * g + 0.0 * b, 0.0 * r + 0.433 * g + 0.567 * b, 0.0 * r + 0.475 * g + 0.525 * b);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown colour-blindness type.");
            }
        }
    }
}
