// Explicit usings and a block-scoped namespace are used here (instead of relying on
// ImplicitUsings/file-scoped namespaces) because Unity compiles this file directly as part of
// the com.voidkargo.shared local package, ignoring this project's .csproj SDK settings, and
// Unity's compiler is pinned to C# 9.0 (file-scoped namespaces need C# 10+).
using System.Collections.Generic;
using Game.Shared.Dtos;

namespace Game.Shared.Art.Genome
{
    /// <summary>
    /// Logical canvas geometry for a generated sprite. The renderer always works at
    /// <see cref="Logical"/> pixels and integer-upscales by <see cref="Scale"/> to reach the
    /// final baked texture size - authoring directly at the final resolution would produce
    /// smooth/anti-aliased "soup" rather than readable pixel art (see ADR 0006). The product
    /// <c>Logical * Scale</c> must equal 256 (the final texture size settled on for ships and
    /// stations); <see cref="Logical"/> itself can vary per design (e.g. 64x4, 32x8, 128x2).
    /// </summary>
    public sealed record CanvasSpec(int Logical, int Scale);

    /// <summary>
    /// One control point of a ship's spine, in logical canvas space, with a thickness (radius,
    /// in logical pixels) at that point. The renderer interpolates a Catmull-Rom curve through
    /// consecutive points and stamps capsule shapes along it to build the hull mass.
    /// </summary>
    public sealed record SpinePoint(double X, double Y, double Thickness);

    /// <summary>
    /// Describes the ship's overall body/mass shape: a spine (ordered list of
    /// <see cref="SpinePoint"/>s from bow to stern) plus how strictly the left/right halves
    /// mirror each other.
    /// </summary>
    public sealed record SilhouetteSpec(
        IReadOnlyList<SpinePoint> Spine,
        /// <summary>
        /// 0 = perfectly mirrored port/starboard; greater than 0 allows the asymmetry pass to
        /// nudge greebles/modules off-axis by up to this many logical pixels, so hulls read as
        /// "industrial" rather than machine-perfect. See ADR 0006's mirror pass.
        /// </summary>
        double AsymmetryBudget);

    /// <summary>The functional kind of an attached module; drives both placement rules and shading/emissive treatment.</summary>
    public enum ModuleKind
    {
        Engine,
        Cargo,
        Sensor,
        Radiator,
        Weapon,
        Habitat,
        Antenna,
    }

    /// <summary>
    /// One attached module (engine nacelle, cargo pod, sensor dish, ...), anchored to a point on
    /// the silhouette spine in logical pixels.
    /// </summary>
    public sealed record ModuleSpec(
        ModuleKind Kind,
        double AnchorX,
        double AnchorY,
        int Count,
        double Size,
        bool Emissive);

    /// <summary>Surface-detail ("greeble") density/style controlling panel-line generation.</summary>
    public sealed record GreebleSpec(
        /// <summary>0 (bare) to 1 (maximally detailed) target fraction of hull area carrying panel-line detail.</summary>
        double Density,
        /// <summary>A style tag (e.g. "slavic-industrial", "smooth-military") the renderer uses to bias line orientation/spacing.</summary>
        string Style);

    /// <summary>
    /// The palette *role* a zone of the hull is painted with - never a literal colour. The
    /// renderer resolves each role to a palette index at bake time via
    /// <see cref="ShipGenome.Zones"/>, and swapping a faction/cosmetic skin is just swapping
    /// which <c>Palette</c> those roles resolve against (see ADR 0006, palette-indexed textures).
    /// </summary>
    public enum PaletteRole
    {
        Hull,
        HullShadow,
        HullLight,
        Trim,
        Accent,
        Glass,
        Emissive,
        Outline,
    }

    /// <summary>
    /// A schema-validated, LLM-authored description of one ship's or station's art, from which
    /// the deterministic renderer (<c>Rendering/ShipRenderer</c>) always produces byte-identical
    /// pixels for a given <see cref="Seed"/>. LLM agents author and edit genomes; they never
    /// emit pixels directly (see ADR 0006).
    /// </summary>
    public sealed record ShipGenome(
        /// <summary>Matches <see cref="ShipTypeDto.Id"/> - genome and game-balance data share one identity.</summary>
        string Id,
        ShipClass Class,
        string FactionId,
        int Epoch,
        ulong Seed,
        CanvasSpec Canvas,
        SilhouetteSpec Silhouette,
        IReadOnlyList<ModuleSpec> Modules,
        GreebleSpec Greebles,
        /// <summary>Every <see cref="PaletteRole"/> must be present - see <c>GenomeValidator</c>.</summary>
        IReadOnlyDictionary<PaletteRole, int> Zones,
        /// <summary>0 (pristine) to 1 (heavily worn); drives the speckle-noise wear pass. See ADR 0006's MVP scope note on decals.</summary>
        double Wear);
}
