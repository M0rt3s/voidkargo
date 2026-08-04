// Explicit usings and a block-scoped namespace are used here (instead of relying on
// ImplicitUsings/file-scoped namespaces) because Unity compiles this file directly as part of
// the com.voidkargo.shared local package, ignoring this project's .csproj SDK settings, and
// Unity's compiler is pinned to C# 9.0 (file-scoped namespaces need C# 10+).
using System;
using System.Globalization;

namespace Game.Shared.Design
{
    /// <summary>
    /// A plain sRGB colour with 8-bit channels. Deliberately free of any engine or framework type
    /// so the same value can be handed to Unity (<c>new Color32(r, g, b, a)</c>), to a CSS variable,
    /// or to a report generator without <c>Game.Shared</c> taking a dependency on any of them.
    /// </summary>
    public readonly struct ColorRgb : IEquatable<ColorRgb>
    {
        public ColorRgb(byte r, byte g, byte b, byte a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public byte R { get; }

        public byte G { get; }

        public byte B { get; }

        public byte A { get; }

        /// <summary>
        /// Parses <c>#RGB</c>, <c>#RRGGBB</c> or <c>#RRGGBBAA</c> (the leading <c>#</c> is optional).
        /// </summary>
        public static ColorRgb FromHex(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
            {
                throw new ArgumentException("Hex colour must not be empty.", nameof(hex));
            }

            var value = hex.Trim();
            if (value.StartsWith("#", StringComparison.Ordinal))
            {
                value = value.Substring(1);
            }

            if (value.Length == 3)
            {
                // #RGB is shorthand for #RRGGBB.
                value = string.Concat(value[0], value[0], value[1], value[1], value[2], value[2]);
            }

            if (value.Length != 6 && value.Length != 8)
            {
                throw new FormatException($"'{hex}' is not a valid hex colour (expected #RGB, #RRGGBB or #RRGGBBAA).");
            }

            return new ColorRgb(
                ParseByte(value, 0),
                ParseByte(value, 2),
                ParseByte(value, 4),
                value.Length == 8 ? ParseByte(value, 6) : (byte)255);
        }

        /// <summary>Renders as <c>#RRGGBB</c>, or <c>#RRGGBBAA</c> when not fully opaque.</summary>
        public string ToHex() => A == 255
            ? string.Format(CultureInfo.InvariantCulture, "#{0:X2}{1:X2}{2:X2}", R, G, B)
            : string.Format(CultureInfo.InvariantCulture, "#{0:X2}{1:X2}{2:X2}{3:X2}", R, G, B, A);

        /// <summary>Renders as <c>r, g, b</c> — the form Bootstrap's <c>--bs-*-rgb</c> variables expect.</summary>
        public string ToRgbTriplet() => string.Format(CultureInfo.InvariantCulture, "{0}, {1}, {2}", R, G, B);

        public bool Equals(ColorRgb other) => R == other.R && G == other.G && B == other.B && A == other.A;

        public override bool Equals(object? obj) => obj is ColorRgb other && Equals(other);

        public override int GetHashCode() => (R << 24) | (G << 16) | (B << 8) | A;

        public override string ToString() => ToHex();

        public static bool operator ==(ColorRgb left, ColorRgb right) => left.Equals(right);

        public static bool operator !=(ColorRgb left, ColorRgb right) => !left.Equals(right);

        private static byte ParseByte(string source, int offset) =>
            byte.Parse(source.Substring(offset, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
    }
}
