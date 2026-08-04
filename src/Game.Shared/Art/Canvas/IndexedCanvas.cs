// Explicit usings and a block-scoped namespace are used here (instead of relying on
// ImplicitUsings/file-scoped namespaces) because Unity compiles this file directly as part of
// the com.voidkargo.shared local package, ignoring this project's .csproj SDK settings, and
// Unity's compiler is pinned to C# 9.0 (file-scoped namespaces need C# 10+).
using System;

namespace Game.Shared.Art.Canvas
{
    /// <summary>
    /// A width x height pixel buffer with three parallel byte planes, matching the packed
    /// texture format decided in ADR 0006's implementation: a palette <c>Index</c> (0-15,
    /// resolved to colour by a <c>Palette</c> at render time - see <c>Palette/Palette.cs</c>), a
    /// <c>Glow</c> emissive intensity (0-255), and coverage <c>Alpha</c> (0 = empty pixel, 255 =
    /// opaque). There is deliberately no RGB plane here - colour is never baked into a sprite.
    /// </summary>
    public sealed class IndexedCanvas
    {
        public int Width { get; }
        public int Height { get; }

        private readonly byte[] _index;
        private readonly byte[] _glow;
        private readonly byte[] _alpha;

        public IndexedCanvas(int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), $"Canvas dimensions must be positive, got {width}x{height}.");
            }

            Width = width;
            Height = height;
            _index = new byte[width * height];
            _glow = new byte[width * height];
            _alpha = new byte[width * height];
        }

        public bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;

        private int OffsetOf(int x, int y)
        {
            if (!InBounds(x, y))
            {
                throw new ArgumentOutOfRangeException(nameof(x), $"Pixel ({x}, {y}) is outside the {Width}x{Height} canvas.");
            }

            return y * Width + x;
        }

        public byte GetIndex(int x, int y) => _index[OffsetOf(x, y)];
        public void SetIndex(int x, int y, byte value) => _index[OffsetOf(x, y)] = value;

        public byte GetGlow(int x, int y) => _glow[OffsetOf(x, y)];
        public void SetGlow(int x, int y, byte value) => _glow[OffsetOf(x, y)] = value;

        public byte GetAlpha(int x, int y) => _alpha[OffsetOf(x, y)];
        public void SetAlpha(int x, int y, byte value) => _alpha[OffsetOf(x, y)] = value;

        /// <summary>Convenience setter for a fully opaque, non-glowing pixel of a given palette index.</summary>
        public void Paint(int x, int y, byte paletteIndex)
        {
            SetIndex(x, y, paletteIndex);
            SetAlpha(x, y, 255);
        }

        /// <summary>Clears a pixel back to fully transparent (index/glow reset to 0).</summary>
        public void Erase(int x, int y)
        {
            SetIndex(x, y, 0);
            SetGlow(x, y, 0);
            SetAlpha(x, y, 0);
        }

        /// <summary>
        /// Integer (nearest-neighbour, i.e. block-replicating) upscale by <paramref name="scale"/>
        /// - the final pass that turns the logical working grid into the baked texture size (see
        /// ADR 0006: "integer-upscale x4 to 256x256"). Never uses bilinear/smooth resizing, which
        /// would blur pixel-art edges into "soup".
        /// </summary>
        public IndexedCanvas UpscaleNearestNeighbor(int scale)
        {
            if (scale <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(scale), $"Scale must be positive, got {scale}.");
            }

            var result = new IndexedCanvas(Width * scale, Height * scale);
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    var index = GetIndex(x, y);
                    var glow = GetGlow(x, y);
                    var alpha = GetAlpha(x, y);

                    var destX0 = x * scale;
                    var destY0 = y * scale;
                    for (var dy = 0; dy < scale; dy++)
                    {
                        for (var dx = 0; dx < scale; dx++)
                        {
                            var destX = destX0 + dx;
                            var destY = destY0 + dy;
                            result.SetIndex(destX, destY, index);
                            result.SetGlow(destX, destY, glow);
                            result.SetAlpha(destX, destY, alpha);
                        }
                    }
                }
            }

            return result;
        }
    }
}
