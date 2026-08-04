// Explicit usings and a block-scoped namespace are used here (instead of relying on
// ImplicitUsings/file-scoped namespaces) because Unity compiles this file directly as part of
// the com.voidkargo.shared local package, ignoring this project's .csproj SDK settings, and
// Unity's compiler is pinned to C# 9.0 (file-scoped namespaces need C# 10+).
using System.Collections.Generic;
using Game.Shared.Art.Encoding;

namespace Game.Shared.Art.Palette
{
    /// <summary>
    /// Lays out a set of <see cref="Palette"/>s into a single combined lookup-table image: one
    /// row per palette, 16 columns (one per palette index). A URP shader samples this LUT at
    /// <c>(paletteIndex / 16, paletteRow)</c> instead of an individual per-skin texture - this is
    /// what makes "repaint parts of it"/faction skins/cosmetics a data change (a new LUT row)
    /// rather than a new texture asset (see ADR 0006). Baking this into an actual texture asset
    /// is done by Unity Editor tooling ("Foundry"); this class only produces the raw pixel
    /// bytes/PNG so the layout logic has no Unity dependency and is unit-testable here.
    /// </summary>
    public static class PaletteLutBaker
    {
        /// <summary>Builds a row-major RGBA8 buffer, width = <see cref="Palette.ColorCount"/>, height = <paramref name="palettes"/>.Count.</summary>
        public static byte[] BuildRgbaBuffer(IReadOnlyList<Palette> palettes)
        {
            var width = Palette.ColorCount;
            var height = palettes.Count;
            var buffer = new byte[width * height * 4];

            for (var row = 0; row < height; row++)
            {
                var palette = palettes[row];
                for (var column = 0; column < width; column++)
                {
                    var color = palette[column];
                    var offset = (row * width + column) * 4;
                    buffer[offset] = color.R;
                    buffer[offset + 1] = color.G;
                    buffer[offset + 2] = color.B;
                    buffer[offset + 3] = 255;
                }
            }

            return buffer;
        }

        /// <summary>Encodes the combined LUT as a PNG (width 16, height = palette count) via <see cref="PngEncoder.EncodeRgba"/>.</summary>
        public static byte[] BuildPng(IReadOnlyList<Palette> palettes) =>
            PngEncoder.EncodeRgba(Palette.ColorCount, palettes.Count, BuildRgbaBuffer(palettes));
    }
}
