// Explicit usings and a block-scoped namespace are used here (instead of relying on
// ImplicitUsings/file-scoped namespaces) because Unity compiles this file directly as part of
// the com.voidkargo.shared local package, ignoring this project's .csproj SDK settings, and
// Unity's compiler is pinned to C# 9.0 (file-scoped namespaces need C# 10+).
using System.Collections.Generic;
using Game.Shared.Art.Json;

namespace Game.Shared.Art.Palette
{
    /// <summary>
    /// (De)serializes a <see cref="Palette"/> to/from the dependency-free <see cref="JsonValue"/>
    /// model, mirroring <c>Genome/GenomeJson.cs</c>, so faction/cosmetic palettes can be authored
    /// as plain JSON files alongside genomes (see ADR 0006).
    /// </summary>
    public static class PaletteJson
    {
        public static Palette Parse(string json) => FromJson(JsonValue.Parse(json));

        public static string ToJsonString(Palette palette) => ToJson(palette).ToJsonString();

        public static Palette FromJson(JsonValue root)
        {
            var id = root.Get("id").AsString();
            var colorsJson = root.Get("colors").AsArray();
            var colors = new List<RgbColor>(colorsJson.Count);
            foreach (var colorJson in colorsJson)
            {
                colors.Add(new RgbColor(
                    (byte)colorJson.Get("r").AsInt(),
                    (byte)colorJson.Get("g").AsInt(),
                    (byte)colorJson.Get("b").AsInt()));
            }

            return new Palette(id, colors);
        }

        public static JsonValue ToJson(Palette palette)
        {
            var colorsJson = JsonValue.Array();
            for (var i = 0; i < Palette.ColorCount; i++)
            {
                var color = palette[i];
                colorsJson.Add(JsonValue.Object()
                    .Set("r", JsonValue.Of(color.R))
                    .Set("g", JsonValue.Of(color.G))
                    .Set("b", JsonValue.Of(color.B)));
            }

            return JsonValue.Object()
                .Set("id", JsonValue.Of(palette.Id))
                .Set("colors", colorsJson);
        }
    }
}
