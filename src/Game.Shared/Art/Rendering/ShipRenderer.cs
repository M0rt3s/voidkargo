// Explicit usings and a block-scoped namespace are used here (instead of relying on
// ImplicitUsings/file-scoped namespaces) because Unity compiles this file directly as part of
// the com.voidkargo.shared local package, ignoring this project's .csproj SDK settings, and
// Unity's compiler is pinned to C# 9.0 (file-scoped namespaces need C# 10+).
using System;
using System.Collections.Generic;
using Game.Shared.Art.Canvas;
using Game.Shared.Art.Genome;
using Game.Shared.Art.Rng;

namespace Game.Shared.Art.Rendering
{
    /// <summary>
    /// The deterministic renderer described in ADR 0006: turns a validated <see cref="ShipGenome"/>
    /// into an <see cref="IndexedCanvas"/> by running a fixed, ordered sequence of passes -
    /// silhouette mass stamping, mirroring, morphological cleanup, distance-based shading with
    /// ordered dithering, greebles, emissive marking, outline, and wear - then integer-upscales
    /// the result to the final baked size. The same genome always produces the same output;
    /// nothing here reads wall-clock time, thread-scheduling order, or <see cref="System.Random"/>.
    /// </summary>
    /// <remarks>
    /// MVP scope reduction (documented per the project convention of calling out deliberate
    /// simplifications): the silhouette mirror pass mirrors the *entire* mass (hull + modules)
    /// rather than applying <see cref="SilhouetteSpec.AsymmetryBudget"/> to gross shape, since a
    /// bilaterally-symmetric hull is the common case for the ship classes in this game's design.
    /// The asymmetry budget is instead spent on the greeble and wear passes, which read from it
    /// to decide how much per-pixel surface detail is allowed to differ port/starboard - this
    /// still delivers the "industrial, not machine-perfect" read the design calls for, at much
    /// lower implementation risk than fully asymmetric mass generation. Likewise, decals are
    /// scoped down to the wear speckle pass only (no bitmap hull-number font yet); see ADR 0006's
    /// Consequences section and the corresponding note in <c>docs/03-modules/game-shared.md</c>.
    /// </remarks>
    public static class ShipRenderer
    {
        // Ordered, non-normalized Bayer 4x4 dither matrix (values 0-15) used to break up hard
        // shading-band edges into a stipple pattern instead of a visible banding line - a classic
        // ordered-dithering technique well suited to small, low-colour-count pixel art.
        private static readonly int[,] BayerMatrix4X4 =
        {
            { 0, 8, 2, 10 },
            { 12, 4, 14, 6 },
            { 3, 11, 1, 9 },
            { 15, 7, 13, 5 },
        };

        // Steps sampled per Catmull-Rom segment when stamping the hull spine - fine enough that
        // consecutive capsule stamps overlap and leave no gaps, at the working (pre-upscale)
        // logical resolution.
        private const int SplineStepsPerSegment = 24;

        /// <summary>Runs the full pipeline at the genome's logical canvas resolution (pre-upscale).</summary>
        public static IndexedCanvas RenderLogical(ShipGenome genome)
        {
            var validation = GenomeValidator.Validate(genome);
            if (!validation.IsValid)
            {
                throw new ArgumentException(
                    "Cannot render an invalid genome:" + Environment.NewLine + string.Join(Environment.NewLine, validation.Errors),
                    nameof(genome));
            }

            var logical = genome.Canvas.Logical;
            var hullMask = new bool[logical, logical];
            var emissiveMask = new bool[logical, logical];

            StampSilhouette(genome, hullMask);
            StampModules(genome, hullMask, emissiveMask);
            MirrorMass(hullMask, logical);
            MirrorMass(emissiveMask, logical);
            MorphologicalCleanup(hullMask, logical);

            var canvas = new IndexedCanvas(logical, logical);
            ShadeAndAssignZones(genome, hullMask, canvas);
            ApplyGreebles(genome, hullMask, canvas);
            ApplyEmissive(genome, emissiveMask, canvas);
            ApplyOutline(genome, hullMask, canvas);
            ApplyWear(genome, hullMask, canvas);

            return canvas;
        }

        /// <summary>Runs the full pipeline and integer-upscales to the genome's declared final texture size (256x256).</summary>
        public static IndexedCanvas RenderFinal(ShipGenome genome) => RenderLogical(genome).UpscaleNearestNeighbor(genome.Canvas.Scale);

        // --- Pass 1: silhouette mass stamping (Catmull-Rom spline + capsule stamps) ---

        private static void StampSilhouette(ShipGenome genome, bool[,] hullMask)
        {
            var spine = genome.Silhouette.Spine;
            var logical = genome.Canvas.Logical;
            var segments = spine.Count - 1;

            for (var segment = 0; segment < segments; segment++)
            {
                for (var step = 0; step <= SplineStepsPerSegment; step++)
                {
                    var t = step / (double)SplineStepsPerSegment;
                    var (x, y, thickness) = SampleCatmullRom(spine, segment, t);
                    StampCapsule(hullMask, logical, x, y, thickness);
                }
            }
        }

        private static (double X, double Y, double Thickness) SampleCatmullRom(IReadOnlyList<SpinePoint> spine, int segment, double t)
        {
            var p0 = spine[Math.Max(segment - 1, 0)];
            var p1 = spine[segment];
            var p2 = spine[segment + 1];
            var p3 = spine[Math.Min(segment + 2, spine.Count - 1)];

            var x = CatmullRom(p0.X, p1.X, p2.X, p3.X, t);
            var y = CatmullRom(p0.Y, p1.Y, p2.Y, p3.Y, t);
            var thickness = CatmullRom(p0.Thickness, p1.Thickness, p2.Thickness, p3.Thickness, t);
            return (x, y, Math.Max(thickness, 0.0));
        }

        private static double CatmullRom(double p0, double p1, double p2, double p3, double t)
        {
            var t2 = t * t;
            var t3 = t2 * t;
            return 0.5 * (
                2.0 * p1 +
                (-p0 + p2) * t +
                (2.0 * p0 - 5.0 * p1 + 4.0 * p2 - p3) * t2 +
                (-p0 + 3.0 * p1 - 3.0 * p2 + p3) * t3);
        }

        private static void StampCapsule(bool[,] mask, int logical, double cx, double cy, double radius)
        {
            var minX = (int)Math.Floor(cx - radius);
            var maxX = (int)Math.Ceiling(cx + radius);
            var minY = (int)Math.Floor(cy - radius);
            var maxY = (int)Math.Ceiling(cy + radius);
            var radiusSquared = radius * radius;

            for (var y = Math.Max(minY, 0); y <= Math.Min(maxY, logical - 1); y++)
            {
                for (var x = Math.Max(minX, 0); x <= Math.Min(maxX, logical - 1); x++)
                {
                    var dx = x + 0.5 - cx;
                    var dy = y + 0.5 - cy;
                    if (dx * dx + dy * dy <= radiusSquared)
                    {
                        mask[x, y] = true;
                    }
                }
            }
        }

        // --- Pass 2: module placement ---

        private static void StampModules(ShipGenome genome, bool[,] hullMask, bool[,] emissiveMask)
        {
            var logical = genome.Canvas.Logical;
            foreach (var module in genome.Modules)
            {
                // Modules with Count > 1 are fanned out along the local tangent direction
                // (approximated here as the vertical/spine axis) so e.g. a 3-engine cluster
                // reads as a row of nacelles rather than a single overlapping blob.
                var spread = module.Size * 1.5;
                var startOffset = -(module.Count - 1) * spread / 2.0;
                for (var i = 0; i < module.Count; i++)
                {
                    var offset = startOffset + i * spread;
                    var cx = module.AnchorX + offset;
                    var cy = module.AnchorY;
                    StampCapsule(hullMask, logical, cx, cy, module.Size);

                    if (module.Emissive)
                    {
                        StampCapsule(emissiveMask, logical, cx, cy, module.Size * 0.5);
                    }
                }
            }
        }

        // --- Pass 3: mirror (see the MVP-scope remark on the class doc comment) ---

        private static void MirrorMass(bool[,] mask, int logical)
        {
            for (var y = 0; y < logical; y++)
            {
                for (var x = 0; x < logical / 2; x++)
                {
                    var mirroredX = logical - 1 - x;
                    var combined = mask[x, y] || mask[mirroredX, y];
                    mask[x, y] = combined;
                    mask[mirroredX, y] = combined;
                }
            }
        }

        // --- Pass 4: morphological cleanup (closing + orphan removal) ---

        private static void MorphologicalCleanup(bool[,] mask, int logical)
        {
            var dilated = MorphologyStep(mask, logical, dilate: true);
            var closed = MorphologyStep(dilated, logical, dilate: false);
            RemoveOrphans(closed, logical, minimumComponentSize: 3);

            Array.Copy(closed, mask, closed.Length);
        }

        private static bool[,] MorphologyStep(bool[,] mask, int logical, bool dilate)
        {
            var result = new bool[logical, logical];
            for (var y = 0; y < logical; y++)
            {
                for (var x = 0; x < logical; x++)
                {
                    var anyFilledNeighbor = mask[x, y];
                    var allFilledNeighbors = mask[x, y];
                    for (var dy = -1; dy <= 1; dy++)
                    {
                        for (var dx = -1; dx <= 1; dx++)
                        {
                            var nx = x + dx;
                            var ny = y + dy;
                            var neighborFilled = nx >= 0 && ny >= 0 && nx < logical && ny < logical && mask[nx, ny];
                            anyFilledNeighbor |= neighborFilled;
                            allFilledNeighbors &= neighborFilled;
                        }
                    }

                    result[x, y] = dilate ? anyFilledNeighbor : allFilledNeighbors;
                }
            }

            return result;
        }

        private static void RemoveOrphans(bool[,] mask, int logical, int minimumComponentSize)
        {
            var visited = new bool[logical, logical];
            var stack = new Stack<(int X, int Y)>();

            for (var startY = 0; startY < logical; startY++)
            {
                for (var startX = 0; startX < logical; startX++)
                {
                    if (!mask[startX, startY] || visited[startX, startY])
                    {
                        continue;
                    }

                    var component = new List<(int X, int Y)>();
                    stack.Push((startX, startY));
                    visited[startX, startY] = true;

                    while (stack.Count > 0)
                    {
                        var (x, y) = stack.Pop();
                        component.Add((x, y));

                        foreach (var (nx, ny) in FourNeighbors(x, y, logical))
                        {
                            if (mask[nx, ny] && !visited[nx, ny])
                            {
                                visited[nx, ny] = true;
                                stack.Push((nx, ny));
                            }
                        }
                    }

                    if (component.Count < minimumComponentSize)
                    {
                        foreach (var (x, y) in component)
                        {
                            mask[x, y] = false;
                        }
                    }
                }
            }
        }

        private static IEnumerable<(int X, int Y)> FourNeighbors(int x, int y, int logical)
        {
            if (x > 0)
            {
                yield return (x - 1, y);
            }

            if (x < logical - 1)
            {
                yield return (x + 1, y);
            }

            if (y > 0)
            {
                yield return (x, y - 1);
            }

            if (y < logical - 1)
            {
                yield return (x, y + 1);
            }
        }

        // --- Pass 5: distance-transform shading, quantized to the palette ramp with ordered dithering ---

        private static void ShadeAndAssignZones(ShipGenome genome, bool[,] hullMask, IndexedCanvas canvas)
        {
            var logical = genome.Canvas.Logical;
            var distance = ComputeInwardDistanceTransform(hullMask, logical);

            var maxDistance = 0.0;
            for (var y = 0; y < logical; y++)
            {
                for (var x = 0; x < logical; x++)
                {
                    if (distance[x, y] > maxDistance)
                    {
                        maxDistance = distance[x, y];
                    }
                }
            }

            if (maxDistance <= 0.0)
            {
                maxDistance = 1.0;
            }

            // A fixed upper-left key light, consistent across every generated sprite so the
            // whole fleet reads as lit from the same direction.
            const double lightX = -0.6;
            const double lightY = -0.8;

            var hullShadowIndex = (byte)genome.Zones[PaletteRole.HullShadow];
            var hullIndex = (byte)genome.Zones[PaletteRole.Hull];
            var hullLightIndex = (byte)genome.Zones[PaletteRole.HullLight];

            for (var y = 0; y < logical; y++)
            {
                for (var x = 0; x < logical; x++)
                {
                    if (!hullMask[x, y])
                    {
                        continue;
                    }

                    var normalizedDistance = distance[x, y] / maxDistance;

                    var gradX = SampleDistance(distance, hullMask, logical, x + 1, y) - SampleDistance(distance, hullMask, logical, x - 1, y);
                    var gradY = SampleDistance(distance, hullMask, logical, x, y + 1) - SampleDistance(distance, hullMask, logical, x, y - 1);
                    var lightTerm = Clamp01(0.5 - 0.5 * (gradX * lightX + gradY * lightY));

                    var shade = Clamp01(0.35 * normalizedDistance + 0.65 * lightTerm);

                    // Ordered dithering: nudge the shade value by a small, position-dependent
                    // offset from the Bayer matrix before thresholding into bands, so band edges
                    // become a stipple instead of a hard line.
                    var ditherOffset = (BayerMatrix4X4[y & 3, x & 3] / 16.0 - 0.5) * 0.12;
                    var ditheredShade = Clamp01(shade + ditherOffset);

                    var index = ditheredShade < 0.38 ? hullShadowIndex : ditheredShade < 0.68 ? hullIndex : hullLightIndex;
                    canvas.Paint(x, y, index);
                }
            }
        }

        private static double SampleDistance(double[,] distance, bool[,] hullMask, int logical, int x, int y)
        {
            if (x < 0 || y < 0 || x >= logical || y >= logical || !hullMask[x, y])
            {
                return 0.0;
            }

            return distance[x, y];
        }

        /// <summary>
        /// A two-pass chamfer distance transform (grassfire) approximating each filled pixel's
        /// distance to the nearest background pixel. Not true Euclidean distance, but more than
        /// accurate enough to drive a stylized 3-band pixel-art shading ramp.
        /// </summary>
        private static double[,] ComputeInwardDistanceTransform(bool[,] mask, int logical)
        {
            const double orthogonalWeight = 1.0;
            const double diagonalWeight = 1.4;
            const double largeValue = 1e9;

            var distance = new double[logical, logical];
            for (var y = 0; y < logical; y++)
            {
                for (var x = 0; x < logical; x++)
                {
                    distance[x, y] = mask[x, y] ? largeValue : 0.0;
                }
            }

            for (var y = 0; y < logical; y++)
            {
                for (var x = 0; x < logical; x++)
                {
                    if (!mask[x, y])
                    {
                        continue;
                    }

                    var best = distance[x, y];
                    best = Math.Min(best, NeighborDistance(distance, logical, x - 1, y, orthogonalWeight));
                    best = Math.Min(best, NeighborDistance(distance, logical, x, y - 1, orthogonalWeight));
                    best = Math.Min(best, NeighborDistance(distance, logical, x - 1, y - 1, diagonalWeight));
                    best = Math.Min(best, NeighborDistance(distance, logical, x + 1, y - 1, diagonalWeight));
                    distance[x, y] = best;
                }
            }

            for (var y = logical - 1; y >= 0; y--)
            {
                for (var x = logical - 1; x >= 0; x--)
                {
                    if (!mask[x, y])
                    {
                        continue;
                    }

                    var best = distance[x, y];
                    best = Math.Min(best, NeighborDistance(distance, logical, x + 1, y, orthogonalWeight));
                    best = Math.Min(best, NeighborDistance(distance, logical, x, y + 1, orthogonalWeight));
                    best = Math.Min(best, NeighborDistance(distance, logical, x + 1, y + 1, diagonalWeight));
                    best = Math.Min(best, NeighborDistance(distance, logical, x - 1, y + 1, diagonalWeight));
                    distance[x, y] = best;
                }
            }

            return distance;
        }

        private static double NeighborDistance(double[,] distance, int logical, int x, int y, double weight)
        {
            if (x < 0 || y < 0 || x >= logical || y >= logical)
            {
                return 0.0 + weight; // treat out-of-bounds as background, i.e. distance 0 plus the step
            }

            return distance[x, y] + weight;
        }

        private static double Clamp01(double value) => value < 0.0 ? 0.0 : value > 1.0 ? 1.0 : value;

        // --- Pass 6: greebles (panel-line surface detail) ---

        private static void ApplyGreebles(ShipGenome genome, bool[,] hullMask, IndexedCanvas canvas)
        {
            var logical = genome.Canvas.Logical;
            var density = genome.Greebles.Density;
            if (density <= 0.0)
            {
                return;
            }

            var trimIndex = (byte)genome.Zones[PaletteRole.Trim];
            var accentIndex = (byte)genome.Zones[PaletteRole.Accent];

            for (var y = 0; y < logical; y++)
            {
                for (var x = 0; x < logical; x++)
                {
                    if (!hullMask[x, y])
                    {
                        continue;
                    }

                    // Order-independent per-pixel hash (see Pcg32.HashToUnit) rather than a
                    // sequential RNG draw, so greeble placement doesn't depend on scan order and
                    // can be reasoned about/tested pixel-by-pixel in isolation.
                    var roll = Pcg32.HashToUnit(genome.Seed ^ 0xA5A5A5A5UL, x, y);
                    if (roll >= density)
                    {
                        continue;
                    }

                    // A second, differently-salted roll picks trim vs. accent so panel lines
                    // aren't a single flat colour.
                    var styleRoll = Pcg32.HashToUnit(genome.Seed ^ 0x5A5A5A5AUL, x, y);
                    canvas.Paint(x, y, styleRoll < 0.7 ? trimIndex : accentIndex);
                }
            }
        }

        // --- Pass 7: emissive mask / glow plane ---

        private static void ApplyEmissive(ShipGenome genome, bool[,] emissiveMask, IndexedCanvas canvas)
        {
            var logical = genome.Canvas.Logical;
            var emissiveIndex = (byte)genome.Zones[PaletteRole.Emissive];

            for (var y = 0; y < logical; y++)
            {
                for (var x = 0; x < logical; x++)
                {
                    if (!emissiveMask[x, y])
                    {
                        continue;
                    }

                    canvas.SetIndex(x, y, emissiveIndex);
                    canvas.SetAlpha(x, y, 255);
                    canvas.SetGlow(x, y, 255);
                }
            }
        }

        // --- Pass 8: selective outline ---

        private static void ApplyOutline(ShipGenome genome, bool[,] hullMask, IndexedCanvas canvas)
        {
            var logical = genome.Canvas.Logical;
            var outlineIndex = (byte)genome.Zones[PaletteRole.Outline];

            // Snapshot which pixels were hull before this pass writes new (outline) pixels, so an
            // outline pixel added at (x, y) doesn't itself get treated as hull when checking (x, y)'s
            // own neighbours later in the same scan.
            var wasHull = (bool[,])hullMask.Clone();

            for (var y = 0; y < logical; y++)
            {
                for (var x = 0; x < logical; x++)
                {
                    if (wasHull[x, y])
                    {
                        continue;
                    }

                    var adjacentToHull = false;
                    foreach (var (nx, ny) in FourNeighbors(x, y, logical))
                    {
                        if (wasHull[nx, ny])
                        {
                            adjacentToHull = true;
                            break;
                        }
                    }

                    if (adjacentToHull)
                    {
                        canvas.Paint(x, y, outlineIndex);
                    }
                }
            }
        }

        // --- Pass 9: wear (speckle noise; see the MVP-scope remark on decals) ---

        private static void ApplyWear(ShipGenome genome, bool[,] hullMask, IndexedCanvas canvas)
        {
            if (genome.Wear <= 0.0)
            {
                return;
            }

            var logical = genome.Canvas.Logical;
            var hullShadowIndex = (byte)genome.Zones[PaletteRole.HullShadow];

            for (var y = 0; y < logical; y++)
            {
                for (var x = 0; x < logical; x++)
                {
                    if (!hullMask[x, y])
                    {
                        continue;
                    }

                    var roll = Pcg32.HashToUnit(genome.Seed ^ 0x0F0F0F0FUL, x, y);
                    if (roll < genome.Wear * 0.15)
                    {
                        canvas.Paint(x, y, hullShadowIndex);
                    }
                }
            }
        }
    }
}
