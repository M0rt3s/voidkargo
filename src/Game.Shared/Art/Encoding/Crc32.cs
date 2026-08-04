// Explicit usings and a block-scoped namespace are used here (instead of relying on
// ImplicitUsings/file-scoped namespaces) because Unity compiles this file directly as part of
// the com.voidkargo.shared local package, ignoring this project's .csproj SDK settings, and
// Unity's compiler is pinned to C# 9.0 (file-scoped namespaces need C# 10+).
namespace Game.Shared.Art.Encoding
{
    /// <summary>
    /// CRC-32 (poly 0xEDB88320, the standard "CRC-32/ISO-HDLC" variant PNG chunks use) - hand
    /// implemented because it's ~30 lines and PNG chunk checksums are the only place it's
    /// needed; not worth a dependency (see ADR 0006, "no System.Drawing/ImageSharp dependency").
    /// </summary>
    internal static class Crc32
    {
        private static readonly uint[] Table = BuildTable();

        private static uint[] BuildTable()
        {
            var table = new uint[256];
            for (uint i = 0; i < 256; i++)
            {
                var c = i;
                for (var k = 0; k < 8; k++)
                {
                    c = (c & 1) != 0 ? 0xEDB88320 ^ (c >> 1) : c >> 1;
                }

                table[i] = c;
            }

            return table;
        }

        public static uint Compute(byte[] data, int offset, int count)
        {
            var crc = 0xFFFFFFFFU;
            for (var i = offset; i < offset + count; i++)
            {
                crc = Table[(crc ^ data[i]) & 0xFF] ^ (crc >> 8);
            }

            return crc ^ 0xFFFFFFFFU;
        }

        public static uint Compute(byte[] data) => Compute(data, 0, data.Length);
    }
}
