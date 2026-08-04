// Explicit usings and a block-scoped namespace are used here (instead of relying on
// ImplicitUsings/file-scoped namespaces) because Unity compiles this file directly as part of
// the com.voidkargo.shared local package, ignoring this project's .csproj SDK settings, and
// Unity's compiler is pinned to C# 9.0 (file-scoped namespaces need C# 10+).
using System;
using System.Collections.Generic;
using Game.Shared.Art.Json;
using Game.Shared.Dtos;

namespace Game.Shared.Art.Genome
{
    /// <summary>
    /// (De)serializes <see cref="ShipGenome"/> to/from the dependency-free <see cref="JsonValue"/>
    /// model (see <c>Json/JsonValue.cs</c>) so genomes can be authored by an LLM agent as plain
    /// JSON files and loaded identically by <c>Game.Backend</c> tooling or the Unity editor.
    /// </summary>
    public static class GenomeJson
    {
        public static ShipGenome Parse(string json) => FromJson(JsonValue.Parse(json));

        public static string ToJsonString(ShipGenome genome) => ToJson(genome).ToJsonString();

        public static ShipGenome FromJson(JsonValue root)
        {
            var canvasJson = root.Get("canvas");
            var canvas = new CanvasSpec(canvasJson.Get("logical").AsInt(), canvasJson.Get("scale").AsInt());

            var silhouetteJson = root.Get("silhouette");
            var spine = new List<SpinePoint>();
            foreach (var pointJson in silhouetteJson.Get("spine").AsArray())
            {
                spine.Add(new SpinePoint(pointJson.Get("x").AsDouble(), pointJson.Get("y").AsDouble(), pointJson.Get("thickness").AsDouble()));
            }

            var silhouette = new SilhouetteSpec(spine, silhouetteJson.Get("asymmetryBudget").AsDouble());

            var modules = new List<ModuleSpec>();
            foreach (var moduleJson in root.Get("modules").AsArray())
            {
                modules.Add(new ModuleSpec(
                    ParseModuleKind(moduleJson.Get("kind").AsString()),
                    moduleJson.Get("anchorX").AsDouble(),
                    moduleJson.Get("anchorY").AsDouble(),
                    moduleJson.Get("count").AsInt(),
                    moduleJson.Get("size").AsDouble(),
                    moduleJson.Get("emissive").AsBool()));
            }

            var greeblesJson = root.Get("greebles");
            var greebles = new GreebleSpec(greeblesJson.Get("density").AsDouble(), greeblesJson.Get("style").AsString());

            var zones = new Dictionary<PaletteRole, int>();
            foreach (var kvp in root.Get("zones").AsObject())
            {
                zones[ParsePaletteRole(kvp.Key)] = kvp.Value.AsInt();
            }

            return new ShipGenome(
                root.Get("id").AsString(),
                ParseShipClass(root.Get("class").AsString()),
                root.Get("factionId").AsString(),
                root.Get("epoch").AsInt(),
                root.Get("seed").AsUInt64(),
                canvas,
                silhouette,
                modules,
                greebles,
                zones,
                root.Get("wear").AsDouble());
        }

        public static JsonValue ToJson(ShipGenome genome)
        {
            var spineJson = JsonValue.Array();
            foreach (var point in genome.Silhouette.Spine)
            {
                spineJson.Add(JsonValue.Object()
                    .Set("x", JsonValue.Of(point.X))
                    .Set("y", JsonValue.Of(point.Y))
                    .Set("thickness", JsonValue.Of(point.Thickness)));
            }

            var silhouetteJson = JsonValue.Object()
                .Set("spine", spineJson)
                .Set("asymmetryBudget", JsonValue.Of(genome.Silhouette.AsymmetryBudget));

            var modulesJson = JsonValue.Array();
            foreach (var module in genome.Modules)
            {
                modulesJson.Add(JsonValue.Object()
                    .Set("kind", JsonValue.Of(module.Kind.ToString()))
                    .Set("anchorX", JsonValue.Of(module.AnchorX))
                    .Set("anchorY", JsonValue.Of(module.AnchorY))
                    .Set("count", JsonValue.Of(module.Count))
                    .Set("size", JsonValue.Of(module.Size))
                    .Set("emissive", JsonValue.Of(module.Emissive)));
            }

            var greeblesJson = JsonValue.Object()
                .Set("density", JsonValue.Of(genome.Greebles.Density))
                .Set("style", JsonValue.Of(genome.Greebles.Style));

            var zonesJson = JsonValue.Object();
            foreach (var kvp in genome.Zones)
            {
                zonesJson.Set(ZoneKeyName(kvp.Key), JsonValue.Of(kvp.Value));
            }

            return JsonValue.Object()
                .Set("id", JsonValue.Of(genome.Id))
                .Set("class", JsonValue.Of(genome.Class.ToString()))
                .Set("factionId", JsonValue.Of(genome.FactionId))
                .Set("epoch", JsonValue.Of(genome.Epoch))
                .Set("seed", JsonValue.Of(genome.Seed))
                .Set("canvas", JsonValue.Object()
                    .Set("logical", JsonValue.Of(genome.Canvas.Logical))
                    .Set("scale", JsonValue.Of(genome.Canvas.Scale)))
                .Set("silhouette", silhouetteJson)
                .Set("modules", modulesJson)
                .Set("greebles", greeblesJson)
                .Set("zones", zonesJson)
                .Set("wear", JsonValue.Of(genome.Wear));
        }

        private static ShipClass ParseShipClass(string value) => value switch
        {
            "LightHauler" => ShipClass.LightHauler,
            "MediumHauler" => ShipClass.MediumHauler,
            "HeavyHauler" => ShipClass.HeavyHauler,
            _ => throw new FormatException($"Unknown ship class '{value}'."),
        };

        private static ModuleKind ParseModuleKind(string value) => value switch
        {
            "Engine" => ModuleKind.Engine,
            "Cargo" => ModuleKind.Cargo,
            "Sensor" => ModuleKind.Sensor,
            "Radiator" => ModuleKind.Radiator,
            "Weapon" => ModuleKind.Weapon,
            "Habitat" => ModuleKind.Habitat,
            "Antenna" => ModuleKind.Antenna,
            _ => throw new FormatException($"Unknown module kind '{value}'."),
        };

        // Zone keys use snake_case in genome JSON (matching ADR 0006's "hull/hull_shadow/.../outline"
        // wording) while the C# enum uses PascalCase - this is the single place that bridges them.
        private static readonly IReadOnlyDictionary<string, PaletteRole> ZoneKeysByName = new Dictionary<string, PaletteRole>
        {
            ["hull"] = PaletteRole.Hull,
            ["hull_shadow"] = PaletteRole.HullShadow,
            ["hull_light"] = PaletteRole.HullLight,
            ["trim"] = PaletteRole.Trim,
            ["accent"] = PaletteRole.Accent,
            ["glass"] = PaletteRole.Glass,
            ["emissive"] = PaletteRole.Emissive,
            ["outline"] = PaletteRole.Outline,
        };

        private static PaletteRole ParsePaletteRole(string value) =>
            ZoneKeysByName.TryGetValue(value, out var role) ? role : throw new FormatException($"Unknown palette zone key '{value}'.");

        private static string ZoneKeyName(PaletteRole role)
        {
            foreach (var kvp in ZoneKeysByName)
            {
                if (kvp.Value == role)
                {
                    return kvp.Key;
                }
            }

            throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown palette role.");
        }
    }
}
