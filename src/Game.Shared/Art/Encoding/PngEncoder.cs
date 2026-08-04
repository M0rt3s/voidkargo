// Explicit usings and a block-scoped namespace are used here (instead of relying on
// ImplicitUsings/file-scoped namespaces) because Unity compiles this file directly as part of
// the com.voidkargo.shared local package, ignoring this project's .csproj SDK settings, and
// Unity's compiler is pinned to C# 9.0 (file-scoped namespaces need C# 10+).
using System;
using System.IO;
using System.IO.Compression;
using Game.Shared.Art.Canvas;

namespace Game.Shared.Art.Encoding
{
    /// <summary>
    /// A minimal PNG encoder for an <see cref="IndexedCanvas"/>, with no
    /// <c>System.Drawing</c>/ImageSharp dependency (see ADR 0006) so the exact same code runs
    /// under Unity's C# 9 toolchain and the `dotnet` SDK. Writes a single IDAT chunk, filter
    /// type "None" per scanline (simplicity over compression ratio - acceptable since these are
    /// small, mostly-transparent sprites, not photographic images), colour type 6
    /// (truecolour + alpha, 8 bits/channel): R = palette index (0-15), G = glow/emissive
    /// intensity, B = reserved/0, A = coverage. Deflate compression is delegated to the inbox
    /// <see cref="DeflateStream"/> (raw DEFLATE, no zlib/gzip wrapper), which this encoder then
    /// wraps in a hand-written zlib header/trailer as PNG's IDAT format requires.
    /// </summary>
    /// <remarks>
    /// Note on determinism: the *decoded pixel content* of a baked PNG is guaranteed
    /// byte-for-byte identical for a given genome (that's what the renderer and its golden-hash
    /// tests guarantee - see ADR 0006). The *compressed* PNG file bytes could in principle differ
    /// between two different DeflateStream implementations (e.g. CoreCLR vs. Mono) even though
    /// both decode back to the same pixels, because DEFLATE doesn't mandate a single canonical
    /// encoding. Tests in this repo therefore verify determinism by decoding and comparing pixel
    /// planes, not by comparing raw PNG file bytes across environments.
    /// </remarks>
    public static class PngEncoder
    {
        private static readonly byte[] Signature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        public static byte[] Encode(IndexedCanvas canvas)
        {
            using var output = new MemoryStream();
            output.Write(Signature, 0, Signature.Length);
            WriteChunk(output, "IHDR", BuildIhdr(canvas.Width, canvas.Height));
            WriteChunk(output, "IDAT", BuildIdat(canvas));
            WriteChunk(output, "IEND", Array.Empty<byte>());
            return output.ToArray();
        }

        private static byte[] BuildIhdr(int width, int height)
        {
            var data = new byte[13];
            WriteUInt32BigEndian(data, 0, (uint)width);
            WriteUInt32BigEndian(data, 4, (uint)height);
            data[8] = 8; // bit depth
            data[9] = 6; // colour type: truecolour + alpha
            data[10] = 0; // compression method (only value defined by the PNG spec)
            data[11] = 0; // filter method (only value defined by the PNG spec)
            data[12] = 0; // interlace method: none
            return data;
        }

        private static byte[] BuildIdat(IndexedCanvas canvas)
        {
            var raw = BuildRawScanlines(canvas);

            byte[] deflated;
            using (var compressed = new MemoryStream())
            {
                using (var deflate = new DeflateStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
                {
                    deflate.Write(raw, 0, raw.Length);
                }

                deflated = compressed.ToArray();
            }

            using var zlib = new MemoryStream();
            zlib.WriteByte(0x78); // CMF: deflate, 32K window
            zlib.WriteByte(0x9C); // FLG: default compression level, checksum bits valid for this CMF
            zlib.Write(deflated, 0, deflated.Length);
            WriteUInt32BigEndian(zlib, Adler32.Compute(raw));
            return zlib.ToArray();
        }

        private static byte[] BuildRawScanlines(IndexedCanvas canvas)
        {
            const int bytesPerPixel = 4;
            var stride = canvas.Width * bytesPerPixel;
            var raw = new byte[(stride + 1) * canvas.Height];

            var offset = 0;
            for (var y = 0; y < canvas.Height; y++)
            {
                raw[offset++] = 0; // scanline filter type: None
                for (var x = 0; x < canvas.Width; x++)
                {
                    raw[offset++] = canvas.GetIndex(x, y);
                    raw[offset++] = canvas.GetGlow(x, y);
                    raw[offset++] = 0; // reserved
                    raw[offset++] = canvas.GetAlpha(x, y);
                }
            }

            return raw;
        }

        private static void WriteChunk(Stream output, string type, byte[] data)
        {
            WriteUInt32BigEndian(output, (uint)data.Length);

            var typeBytes = new byte[4];
            for (var i = 0; i < 4; i++)
            {
                typeBytes[i] = (byte)type[i];
            }

            output.Write(typeBytes, 0, 4);
            output.Write(data, 0, data.Length);

            var crcInput = new byte[4 + data.Length];
            Buffer.BlockCopy(typeBytes, 0, crcInput, 0, 4);
            Buffer.BlockCopy(data, 0, crcInput, 4, data.Length);
            WriteUInt32BigEndian(output, Crc32.Compute(crcInput));
        }

        private static void WriteUInt32BigEndian(byte[] buffer, int offset, uint value)
        {
            buffer[offset] = (byte)(value >> 24);
            buffer[offset + 1] = (byte)(value >> 16);
            buffer[offset + 2] = (byte)(value >> 8);
            buffer[offset + 3] = (byte)value;
        }

        private static void WriteUInt32BigEndian(Stream stream, uint value)
        {
            stream.WriteByte((byte)(value >> 24));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)value);
        }
    }
}
