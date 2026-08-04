using System.IO.Compression;

namespace Game.Shared.Tests.Art.TestSupport;

/// <summary>
/// A tiny, test-only PNG decoder that understands exactly the subset of PNG that
/// <c>Game.Shared.Art.Encoding.PngEncoder</c> produces (8-bit RGBA, single IDAT chunk, filter
/// type "None" on every scanline). This intentionally is *not* a general-purpose PNG reader -
/// it exists purely so round-trip tests can assert the encoder's output decodes back to the
/// exact pixel planes that went in, without pulling an imaging library into the main
/// <c>Game.Shared</c> project (see ADR 0006 and PngEncoder's remarks on cross-runtime determinism).
/// </summary>
public static class MinimalPngDecoder
{
    public static (int Width, int Height, byte[] IndexPlane, byte[] GlowPlane, byte[] AlphaPlane) Decode(byte[] png)
    {
        var offset = 8; // skip the fixed 8-byte PNG signature
        int width = 0, height = 0, bitDepth = 0, colorType = 0;
        var idat = new List<byte>();

        while (offset < png.Length)
        {
            var length = ReadUInt32BigEndian(png, offset);
            var type = System.Text.Encoding.ASCII.GetString(png, offset + 4, 4);
            var dataStart = offset + 8;

            if (type == "IHDR")
            {
                width = (int)ReadUInt32BigEndian(png, dataStart);
                height = (int)ReadUInt32BigEndian(png, dataStart + 4);
                bitDepth = png[dataStart + 8];
                colorType = png[dataStart + 9];
            }
            else if (type == "IDAT")
            {
                idat.AddRange(png.Skip(dataStart).Take((int)length));
            }
            else if (type == "IEND")
            {
                break;
            }

            offset = dataStart + (int)length + 4; // + 4 for the trailing CRC
        }

        if (bitDepth != 8 || colorType != 6)
        {
            throw new NotSupportedException($"MinimalPngDecoder only supports 8-bit RGBA PNGs, got bitDepth={bitDepth}, colorType={colorType}.");
        }

        var zlibBytes = idat.ToArray();
        var deflateBody = zlibBytes.Skip(2).Take(zlibBytes.Length - 2 - 4).ToArray(); // strip 2-byte zlib header + 4-byte Adler32 trailer

        byte[] raw;
        using (var compressedStream = new MemoryStream(deflateBody))
        using (var deflate = new DeflateStream(compressedStream, CompressionMode.Decompress))
        using (var rawStream = new MemoryStream())
        {
            deflate.CopyTo(rawStream);
            raw = rawStream.ToArray();
        }

        var indexPlane = new byte[width * height];
        var glowPlane = new byte[width * height];
        var alphaPlane = new byte[width * height];

        var stride = width * 4;
        var rawOffset = 0;
        for (var y = 0; y < height; y++)
        {
            var filterType = raw[rawOffset];
            if (filterType != 0)
            {
                throw new NotSupportedException($"MinimalPngDecoder only supports filter type 0 (None), got {filterType} on row {y}.");
            }

            rawOffset++;
            for (var x = 0; x < width; x++)
            {
                var pixelIndex = y * width + x;
                indexPlane[pixelIndex] = raw[rawOffset++];
                glowPlane[pixelIndex] = raw[rawOffset++];
                rawOffset++; // reserved/blue channel, unused
                alphaPlane[pixelIndex] = raw[rawOffset++];
            }
        }

        return (width, height, indexPlane, glowPlane, alphaPlane);
    }

    private static uint ReadUInt32BigEndian(byte[] buffer, int offset) =>
        ((uint)buffer[offset] << 24) | ((uint)buffer[offset + 1] << 16) | ((uint)buffer[offset + 2] << 8) | buffer[offset + 3];
}
