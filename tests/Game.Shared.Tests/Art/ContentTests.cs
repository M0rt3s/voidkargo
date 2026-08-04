using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Game.Shared.Art.Encoding;
using Game.Shared.Art.Genome;
using Game.Shared.Art.Palette;
using Game.Shared.Art.Rendering;

namespace Game.Shared.Tests.Art;

/// <summary>
/// Discovers and validates every genome/palette file under the repo-root <c>content/</c>
/// directory. This is the "regenerating test" AGENTS.md requires for any committed generated
/// art: a genome or palette file with no corresponding coverage here is a documentation gap,
/// not just a missing asset. Also exercises the same render/bake path the Unity "Foundry"
/// editor window and any future CLI baking tool will use, so a content author finds out
/// immediately (via `dotnet test`) if a hand- or LLM-authored file fails validation or
/// crashes the renderer/encoder - without needing Unity installed.
/// </summary>
public class ContentTests
{
    private static readonly string RepoRoot = FindRepoRoot();
    private static readonly string GenomesDirectory = Path.Combine(RepoRoot, "content", "ship-genomes");
    private static readonly string PalettesDirectory = Path.Combine(RepoRoot, "content", "palettes");

    public static IEnumerable<object[]> GenomeFiles() =>
        Directory.GetFiles(GenomesDirectory, "*.json").OrderBy(f => f).Select(f => new object[] { f });

    public static IEnumerable<object[]> PaletteFiles() =>
        Directory.GetFiles(PalettesDirectory, "*.json").OrderBy(f => f).Select(f => new object[] { f });

    [Fact]
    public void ContentDirectories_Exist()
    {
        Assert.True(Directory.Exists(GenomesDirectory), $"Expected {GenomesDirectory} to exist.");
        Assert.True(Directory.Exists(PalettesDirectory), $"Expected {PalettesDirectory} to exist.");
    }

    [Fact]
    public void AtLeastOneGenomeAndPalette_AreCommitted()
    {
        Assert.NotEmpty(Directory.GetFiles(GenomesDirectory, "*.json"));
        Assert.NotEmpty(Directory.GetFiles(PalettesDirectory, "*.json"));
    }

    [Theory]
    [MemberData(nameof(GenomeFiles))]
    public void Genome_ParsesValidatesAndRenders(string path)
    {
        var genome = GenomeJson.Parse(File.ReadAllText(path));

        // The file name (minus extension) must match the genome's own id, so tooling that
        // globs the directory and tooling that looks up a genome by ShipTypeDto.Id agree.
        var expectedId = Path.GetFileNameWithoutExtension(path);
        Assert.Equal(expectedId, genome.Id);

        var validation = GenomeValidator.Validate(genome);
        Assert.True(validation.IsValid, $"{path} failed validation: {string.Join("; ", validation.Errors)}");

        // Rendering must not throw and must be deterministic - the same guarantee
        // ShipRendererTests pins down for hand-written test genomes, now for real content.
        var first = ShipRenderer.RenderFinal(genome);
        var second = ShipRenderer.RenderFinal(genome);
        for (var y = 0; y < first.Height; y++)
        {
            for (var x = 0; x < first.Width; x++)
            {
                Assert.Equal(first.GetIndex(x, y), second.GetIndex(x, y));
                Assert.Equal(first.GetAlpha(x, y), second.GetAlpha(x, y));
            }
        }

        // Must also be encodable - a genome that renders but can't be baked to PNG is still
        // useless to the Foundry tool.
        var png = PngEncoder.Encode(first);
        Assert.NotEmpty(png);
    }

    [Theory]
    [MemberData(nameof(PaletteFiles))]
    public void Palette_ParsesAndValidates(string path)
    {
        var palette = PaletteJson.Parse(File.ReadAllText(path));

        var expectedId = Path.GetFileNameWithoutExtension(path);
        Assert.Equal(expectedId, palette.Id);

        var validation = PaletteValidator.Validate(palette);
        Assert.True(validation.IsValid, $"{path} failed validation: {string.Join("; ", validation.Errors)}");
    }

    [Fact]
    public void AllGenomes_ReferenceAPaletteRoleSetThatIsComplete()
    {
        // Belt-and-braces: GenomeValidator already checks this, but content authors should
        // see the failure here too since this is the file they're actually editing.
        foreach (var path in Directory.GetFiles(GenomesDirectory, "*.json"))
        {
            var genome = GenomeJson.Parse(File.ReadAllText(path));
            foreach (PaletteRole role in Enum.GetValues(typeof(PaletteRole)))
            {
                Assert.True(genome.Zones.ContainsKey(role), $"{path} is missing zone role '{role}'.");
            }
        }
    }

    [Fact]
    public void PaletteLutBaker_BuildsALutContainingEveryCommittedPalette()
    {
        var palettes = Directory.GetFiles(PalettesDirectory, "*.json")
            .OrderBy(f => f)
            .Select(f => PaletteJson.Parse(File.ReadAllText(f)))
            .ToList();

        var png = PaletteLutBaker.BuildPng(palettes);
        Assert.NotEmpty(png);

        var buffer = PaletteLutBaker.BuildRgbaBuffer(palettes);
        Assert.Equal(palettes.Count * Palette.ColorCount * 4, buffer.Length);
    }

    /// <summary>
    /// Walks up from the test assembly's output directory until it finds the solution file,
    /// so content-directory paths resolve correctly regardless of working directory or
    /// build configuration (Debug/Release, local machine vs. CI).
    /// </summary>
    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "voidkargo.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            $"Could not locate repo root (voidkargo.slnx) walking up from {AppContext.BaseDirectory}.");
    }
}
