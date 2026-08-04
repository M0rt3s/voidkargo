// Explicit usings and a block-scoped namespace are used here (instead of relying on
// ImplicitUsings/file-scoped namespaces) because Unity compiles this file directly as part of
// the com.voidkargo.shared local package, ignoring this project's .csproj SDK settings, and
// Unity's compiler is pinned to C# 9.0 (file-scoped namespaces need C# 10+).
namespace Game.Shared.Art.Encoding
{
    /// <summary>
    /// Adler-32, as required by the zlib stream trailer wrapping PNG <c>IDAT</c> data - hand
    /// implemented for the same reason as <see cref="Crc32"/> (~15 lines, avoids an imaging
    /// library dependency; see ADR 0006).
    /// </summary>
    internal static class Adler32
    {
        private const uint ModAdler = 65521;

        public static uint Compute(byte[] data)
        {
            uint a = 1, b = 0;
            foreach (var value in data)
            {
                a = (a + value) % ModAdler;
                b = (b + a) % ModAdler;
            }

            return (b << 16) | a;
        }
    }
}
