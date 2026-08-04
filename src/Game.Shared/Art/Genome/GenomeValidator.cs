// Explicit usings and a block-scoped namespace are used here (instead of relying on
// ImplicitUsings/file-scoped namespaces) because Unity compiles this file directly as part of
// the com.voidkargo.shared local package, ignoring this project's .csproj SDK settings, and
// Unity's compiler is pinned to C# 9.0 (file-scoped namespaces need C# 10+).
using System;
using System.Collections.Generic;
using Game.Shared.Art.Rendering;
using Game.Shared.Dtos;

namespace Game.Shared.Art.Genome
{
    /// <summary>
    /// The style linter described in ADR 0006: rejects a genome *before* it reaches the
    /// renderer if it violates class-silhouette conventions, zone-map completeness, or basic
    /// range constraints. This catches structurally broken content (an LLM agent hallucinating
    /// an out-of-range value); it deliberately does not and cannot judge whether a valid genome
    /// looks *good* - that remains a human/aesthetic review.
    /// </summary>
    public static class GenomeValidator
    {
        // Average spine thickness, expressed as a fraction of Canvas.Logical, expected per class
        // - this is what makes a LightHauler read as "fast/thin" and a HeavyHauler as "slow/bulky"
        // at a glance regardless of the logical grid resolution chosen for a given design.
        private static readonly IReadOnlyDictionary<ShipClass, (double Min, double Max)> ThicknessFractionByClass =
            new Dictionary<ShipClass, (double, double)>
            {
                [ShipClass.LightHauler] = (0.02, 0.12),
                [ShipClass.MediumHauler] = (0.06, 0.20),
                [ShipClass.HeavyHauler] = (0.12, 0.32),
            };

        // The earliest epoch each module kind is allowed to appear on any hull - an MVP proxy
        // for "gated by research tree" (see the game-design plan): weapons/habitats are later-epoch
        // silhouette features, engines/cargo/sensors are available from the start.
        private static readonly IReadOnlyDictionary<ModuleKind, int> MinimumEpochByModuleKind = new Dictionary<ModuleKind, int>
        {
            [ModuleKind.Engine] = 0,
            [ModuleKind.Cargo] = 0,
            [ModuleKind.Sensor] = 0,
            [ModuleKind.Antenna] = 1,
            [ModuleKind.Radiator] = 1,
            [ModuleKind.Weapon] = 2,
            [ModuleKind.Habitat] = 3,
        };

        private static readonly PaletteRole[] RequiredZones =
        {
            PaletteRole.Hull,
            PaletteRole.HullShadow,
            PaletteRole.HullLight,
            PaletteRole.Trim,
            PaletteRole.Accent,
            PaletteRole.Glass,
            PaletteRole.Emissive,
            PaletteRole.Outline,
        };

        public static ValidationResult Validate(ShipGenome genome)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(genome.Id))
            {
                errors.Add("Genome id must not be empty.");
            }

            ValidateCanvas(genome.Canvas, errors);
            ValidateSilhouette(genome, errors);
            ValidateModules(genome, errors);
            ValidateZones(genome.Zones, errors);

            if (genome.Wear < 0.0 || genome.Wear > 1.0)
            {
                errors.Add($"Wear must be in [0, 1], was {genome.Wear}.");
            }

            if (genome.Greebles.Density < 0.0 || genome.Greebles.Density > 1.0)
            {
                errors.Add($"Greeble density must be in [0, 1], was {genome.Greebles.Density}.");
            }

            return errors.Count == 0 ? ValidationResult.Success : new ValidationResult(errors);
        }

        private static void ValidateCanvas(CanvasSpec canvas, List<string> errors)
        {
            if (canvas.Logical <= 0 || canvas.Scale <= 0)
            {
                errors.Add($"Canvas logical size and scale must both be positive, was Logical={canvas.Logical}, Scale={canvas.Scale}.");
                return;
            }

            var finalSize = canvas.Logical * canvas.Scale;
            if (finalSize != 256)
            {
                errors.Add($"Canvas Logical * Scale must equal 256, was {canvas.Logical} * {canvas.Scale} = {finalSize}.");
            }
        }

        private static void ValidateSilhouette(ShipGenome genome, List<string> errors)
        {
            var silhouette = genome.Silhouette;
            if (silhouette.Spine.Count < 2)
            {
                errors.Add($"Silhouette spine must have at least 2 control points, had {silhouette.Spine.Count}.");
                return;
            }

            if (silhouette.AsymmetryBudget < 0.0)
            {
                errors.Add($"Silhouette asymmetry budget must not be negative, was {silhouette.AsymmetryBudget}.");
            }

            if (!ThicknessFractionByClass.TryGetValue(genome.Class, out var range))
            {
                errors.Add($"No thickness range configured for ship class '{genome.Class}'.");
                return;
            }

            var logical = genome.Canvas.Logical;
            if (logical <= 0)
            {
                return; // already reported by ValidateCanvas
            }

            double thicknessSum = 0.0;
            foreach (var point in silhouette.Spine)
            {
                if (point.Thickness < 0.0)
                {
                    errors.Add($"Spine thickness must not be negative, was {point.Thickness}.");
                }

                thicknessSum += point.Thickness;
            }

            var averageFraction = thicknessSum / silhouette.Spine.Count / logical;
            if (averageFraction < range.Min || averageFraction > range.Max)
            {
                errors.Add(
                    $"Average spine thickness fraction {averageFraction:F3} is out of the expected range " +
                    $"[{range.Min:F3}, {range.Max:F3}] for ship class '{genome.Class}'.");
            }
        }

        private static void ValidateModules(ShipGenome genome, List<string> errors)
        {
            foreach (var module in genome.Modules)
            {
                if (module.Count <= 0)
                {
                    errors.Add($"Module '{module.Kind}' count must be positive, was {module.Count}.");
                }

                if (module.Size <= 0.0)
                {
                    errors.Add($"Module '{module.Kind}' size must be positive, was {module.Size}.");
                }

                if (!MinimumEpochByModuleKind.TryGetValue(module.Kind, out var minimumEpoch))
                {
                    errors.Add($"No minimum-epoch rule configured for module kind '{module.Kind}'.");
                    continue;
                }

                if (genome.Epoch < minimumEpoch)
                {
                    errors.Add($"Module '{module.Kind}' requires epoch >= {minimumEpoch}, but genome epoch is {genome.Epoch}.");
                }
            }
        }

        private static void ValidateZones(IReadOnlyDictionary<PaletteRole, int> zones, List<string> errors)
        {
            foreach (var role in RequiredZones)
            {
                if (!zones.TryGetValue(role, out var index))
                {
                    errors.Add($"Zone map is missing required role '{role}'.");
                    continue;
                }

                if (index < 0 || index > 15)
                {
                    errors.Add($"Zone '{role}' palette index must be in [0, 15], was {index}.");
                }
            }
        }
    }
}
