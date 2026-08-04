// Explicit usings and a block-scoped namespace are used here (instead of relying on
// ImplicitUsings/file-scoped namespaces) because Unity compiles this file directly as part of
// the com.voidkargo.shared local package, ignoring this project's .csproj SDK settings, and
// Unity's compiler is pinned to C# 9.0 (file-scoped namespaces need C# 10+).
using System;
using System.Collections.Generic;

namespace Game.Shared.Art.Palette
{
    /// <summary>An 8-bit-per-channel opaque colour. Alpha lives separately on the canvas (see <c>Canvas/IndexedCanvas.cs</c>), not here.</summary>
    public readonly struct RgbColor
    {
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }

        public RgbColor(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        /// <summary>Relative luminance per the WCAG 2.x definition (sRGB, linearized), used for contrast checks.</summary>
        public double RelativeLuminance()
        {
            var r = Linearize(R / 255.0);
            var g = Linearize(G / 255.0);
            var b = Linearize(B / 255.0);
            return 0.2126 * r + 0.7152 * g + 0.0722 * b;
        }

        private static double Linearize(double channel) =>
            channel <= 0.03928 ? channel / 12.92 : Math.Pow((channel + 0.055) / 1.055, 2.4);
    }

    /// <summary>
    /// A 16-colour indexed palette row. The renderer never bakes final colours into a sprite -
    /// it bakes a palette *index* per pixel, and a Palette resolves those indices to actual
    /// colours at render (shader) time (see ADR 0006). A faction skin or a purchasable cosmetic
    /// is just a different Palette applied to the same indexed texture.
    /// </summary>
    public sealed class Palette
    {
        public const int ColorCount = 16;

        private readonly RgbColor[] _colors;

        public string Id { get; }

        public Palette(string id, IReadOnlyList<RgbColor> colors)
        {
            if (colors.Count != ColorCount)
            {
                throw new ArgumentException($"A palette must have exactly {ColorCount} colours, got {colors.Count}.", nameof(colors));
            }

            Id = id;
            _colors = new RgbColor[ColorCount];
            for (var i = 0; i < ColorCount; i++)
            {
                _colors[i] = colors[i];
            }
        }

        public RgbColor this[int index] => _colors[index];
    }
}
