// Nullable reference annotations are enabled explicitly (rather than relying on project-level
// settings) because Unity's own generated .csproj for this assembly does not set <Nullable>
// the way Game.Shared's does - without this pragma the '?' annotations below would still
// compile, but only as a warning-suppressed no-op rather than an intentional nullable context.
#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Game.Shared.Art.Canvas;
using Game.Shared.Art.Encoding;
using Game.Shared.Art.Genome;
using Game.Shared.Art.Palette;
using Game.Shared.Art.Rendering;
using UnityEditor;
using UnityEngine;

namespace VoidKargo.Editor.Foundry
{
    /// <summary>
    /// "Foundry": the Unity Editor window for the genome-driven pixel-art pipeline (ADR 0006).
    /// Loads ship genomes and palettes from the repo-root <c>content/</c> directory (shared with
    /// <c>Game.Backend</c> tooling and <c>Game.Shared.Tests</c>'s <c>ContentTests</c> - see
    /// <c>docs/03-modules/game-shared.md</c>), previews a genome rendered against any loaded
    /// palette, and bakes the result to <c>Assets/Art/Generated/...</c> PNGs with the correct
    /// (uncompressed, point-filtered, non-sRGB) import settings for an indexed data texture.
    ///
    /// This file was authored without a local Unity install available to compile-check it (see
    /// AGENTS.md's Unity verification note); open the project in Unity 6000.5.6f1 and check the
    /// Editor Console after pulling this change.
    /// </summary>
    public sealed class FoundryWindow : EditorWindow
    {
        private const string RepoRootMarkerFile = "voidkargo.slnx";
        private const string ShipGenomesRelativePath = "content/ship-genomes";
        private const string PalettesRelativePath = "content/palettes";
        private const string GeneratedShipsAssetDir = "Assets/Art/Generated/Ships";
        private const string GeneratedPaletteLutAssetPath = "Assets/Art/Generated/PaletteLut.png";

        private sealed class GenomeEntry
        {
            public string FilePath = string.Empty;
            public string FileName = string.Empty;
            public ShipGenome? Genome;
            public ValidationResult Validation = ValidationResult.Success;
            public bool ShowErrors;
        }

        private sealed class PaletteEntry
        {
            public string FilePath = string.Empty;
            public string FileName = string.Empty;
            public Palette? Palette;
            public ValidationResult Validation = ValidationResult.Success;
            public bool ShowErrors;
        }

        private readonly List<GenomeEntry> _genomes = new List<GenomeEntry>();
        private readonly List<PaletteEntry> _palettes = new List<PaletteEntry>();

        private int _selectedGenomeIndex = -1;
        private int _selectedPaletteIndex = -1;
        private Texture2D? _previewTexture;
        private Vector2 _genomeListScroll;
        private Vector2 _paletteListScroll;
        private string? _repoRoot;
        private string _statusMessage = string.Empty;

        [MenuItem("VoidKargo/Foundry")]
        public static void Open()
        {
            var window = GetWindow<FoundryWindow>();
            window.titleContent = new GUIContent("Foundry");
            window.minSize = new Vector2(720, 440);
            window.Show();
        }

        private void OnEnable() => RefreshContent();

        private void OnDisable()
        {
            if (_previewTexture != null)
            {
                DestroyImmediate(_previewTexture);
                _previewTexture = null;
            }
        }

        private void RefreshContent()
        {
            _repoRoot = FindRepoRoot();
            _genomes.Clear();
            _palettes.Clear();
            _statusMessage = string.Empty;

            if (_repoRoot == null)
            {
                _statusMessage = $"Could not locate repo root (looking for '{RepoRootMarkerFile}') above {Application.dataPath}.";
                return;
            }

            LoadGenomes(Path.Combine(_repoRoot, ToNativePath(ShipGenomesRelativePath)));
            LoadPalettes(Path.Combine(_repoRoot, ToNativePath(PalettesRelativePath)));

            _selectedGenomeIndex = _genomes.Count > 0 ? 0 : -1;
            _selectedPaletteIndex = _palettes.Count > 0 ? 0 : -1;

            UpdatePreview();
        }

        private void LoadGenomes(string directory)
        {
            if (!Directory.Exists(directory))
            {
                return;
            }

            foreach (var file in Directory.GetFiles(directory, "*.json").OrderBy(f => f))
            {
                var entry = new GenomeEntry { FilePath = file, FileName = Path.GetFileNameWithoutExtension(file) };
                try
                {
                    entry.Genome = GenomeJson.Parse(File.ReadAllText(file));
                    entry.Validation = GenomeValidator.Validate(entry.Genome);
                }
                catch (Exception ex)
                {
                    entry.Validation = ValidationResult.Failure($"Failed to parse: {ex.Message}");
                }

                _genomes.Add(entry);
            }
        }

        private void LoadPalettes(string directory)
        {
            if (!Directory.Exists(directory))
            {
                return;
            }

            foreach (var file in Directory.GetFiles(directory, "*.json").OrderBy(f => f))
            {
                var entry = new PaletteEntry { FilePath = file, FileName = Path.GetFileNameWithoutExtension(file) };
                try
                {
                    entry.Palette = PaletteJson.Parse(File.ReadAllText(file));
                    entry.Validation = PaletteValidator.Validate(entry.Palette);
                }
                catch (Exception ex)
                {
                    entry.Validation = ValidationResult.Failure($"Failed to parse: {ex.Message}");
                }

                _palettes.Add(entry);
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            DrawSidebar();
            DrawPreviewAndBakePane();
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(_statusMessage))
            {
                EditorGUILayout.HelpBox(_statusMessage, MessageType.Info);
            }
        }

        private void DrawSidebar()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(300));

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Ship genomes", EditorStyles.boldLabel);
            if (GUILayout.Button("Refresh", GUILayout.Width(70)))
            {
                RefreshContent();
            }
            EditorGUILayout.EndHorizontal();

            _genomeListScroll = EditorGUILayout.BeginScrollView(_genomeListScroll, GUILayout.Height(220));
            for (var i = 0; i < _genomes.Count; i++)
            {
                DrawGenomeRow(i);
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Palettes", EditorStyles.boldLabel);

            _paletteListScroll = EditorGUILayout.BeginScrollView(_paletteListScroll, GUILayout.Height(160));
            for (var i = 0; i < _palettes.Count; i++)
            {
                DrawPaletteRow(i);
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        private void DrawGenomeRow(int index)
        {
            var entry = _genomes[index];
            EditorGUILayout.BeginHorizontal();

            var isSelected = index == _selectedGenomeIndex;
            var label = (entry.Validation.IsValid ? "OK  " : "FAIL ") + entry.FileName;
            var content = new GUIContent(label, entry.FilePath);
            var newSelected = GUILayout.Toggle(isSelected, content, EditorStyles.radioButton);
            if (newSelected && !isSelected)
            {
                _selectedGenomeIndex = index;
                UpdatePreview();
            }

            if (!entry.Validation.IsValid && GUILayout.Button("?", GUILayout.Width(22)))
            {
                entry.ShowErrors = !entry.ShowErrors;
            }

            EditorGUILayout.EndHorizontal();

            if (entry.ShowErrors && !entry.Validation.IsValid)
            {
                foreach (var error in entry.Validation.Errors)
                {
                    EditorGUILayout.HelpBox(error, MessageType.Error);
                }
            }
        }

        private void DrawPaletteRow(int index)
        {
            var entry = _palettes[index];
            EditorGUILayout.BeginHorizontal();

            var isSelected = index == _selectedPaletteIndex;
            var label = (entry.Validation.IsValid ? "OK  " : "FAIL ") + entry.FileName;
            var content = new GUIContent(label, entry.FilePath);
            var newSelected = GUILayout.Toggle(isSelected, content, EditorStyles.radioButton);
            if (newSelected && !isSelected)
            {
                _selectedPaletteIndex = index;
                UpdatePreview();
            }

            if (!entry.Validation.IsValid && GUILayout.Button("?", GUILayout.Width(22)))
            {
                entry.ShowErrors = !entry.ShowErrors;
            }

            EditorGUILayout.EndHorizontal();

            if (entry.ShowErrors && !entry.Validation.IsValid)
            {
                foreach (var error in entry.Validation.Errors)
                {
                    EditorGUILayout.HelpBox(error, MessageType.Error);
                }
            }
        }

        private void DrawPreviewAndBakePane()
        {
            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

            var rect = GUILayoutUtility.GetRect(256, 256, GUILayout.ExpandWidth(false));
            if (_previewTexture != null)
            {
                // Point-filtered draw so the 256x256 baked preview stays crisp pixel art, not
                // blurred by the Editor GUI's default texture scaling.
                var previousFilterMode = _previewTexture.filterMode;
                _previewTexture.filterMode = FilterMode.Point;
                GUI.DrawTexture(rect, _previewTexture, ScaleMode.ScaleToFit, true);
                _previewTexture.filterMode = previousFilterMode;
            }
            else
            {
                EditorGUI.HelpBox(rect, "Select a valid genome and palette to preview.", MessageType.None);
            }

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(!CanBakeSelectedGenome()))
            {
                if (GUILayout.Button("Bake Selected Ship", GUILayout.Height(28)))
                {
                    BakeShip(_genomes[_selectedGenomeIndex]);
                }
            }

            using (new EditorGUI.DisabledScope(_genomes.Count == 0))
            {
                if (GUILayout.Button("Bake All Valid Ships"))
                {
                    BakeAllValidShips();
                }
            }

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(_palettes.Count == 0))
            {
                if (GUILayout.Button("Bake Palette LUT (all valid palettes)", GUILayout.Height(28)))
                {
                    BakePaletteLut();
                }
            }

            EditorGUILayout.EndVertical();
        }

        private bool CanBakeSelectedGenome() =>
            _selectedGenomeIndex >= 0
            && _selectedGenomeIndex < _genomes.Count
            && _genomes[_selectedGenomeIndex].Genome != null
            && _genomes[_selectedGenomeIndex].Validation.IsValid;

        private void UpdatePreview()
        {
            if (_previewTexture != null)
            {
                DestroyImmediate(_previewTexture);
                _previewTexture = null;
            }

            if (_selectedGenomeIndex < 0 || _selectedGenomeIndex >= _genomes.Count)
            {
                return;
            }

            var genome = _genomes[_selectedGenomeIndex].Genome;
            if (genome == null || !_genomes[_selectedGenomeIndex].Validation.IsValid)
            {
                return;
            }

            if (_selectedPaletteIndex < 0 || _selectedPaletteIndex >= _palettes.Count)
            {
                return;
            }

            var palette = _palettes[_selectedPaletteIndex].Palette;
            if (palette == null)
            {
                return;
            }

            try
            {
                var canvas = ShipRenderer.RenderFinal(genome);
                _previewTexture = BuildPreviewTexture(canvas, palette);
            }
            catch (Exception ex)
            {
                _statusMessage = $"Render failed for '{genome.Id}': {ex.Message}";
            }
        }

        /// <summary>
        /// Resolves the baked index/glow/alpha planes against a palette into a flat-shaded
        /// RGBA preview. This is deliberately simpler than the eventual URP palette-swap shader
        /// (no bloom/rim/dithered emissive) - it exists so a content author can sanity-check
        /// silhouette, zone placement, and palette choice without leaving the Editor.
        /// </summary>
        private static Texture2D BuildPreviewTexture(IndexedCanvas canvas, Palette palette)
        {
            var texture = new Texture2D(canvas.Width, canvas.Height, TextureFormat.RGBA32, false, true)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };

            var pixels = new Color32[canvas.Width * canvas.Height];
            for (var y = 0; y < canvas.Height; y++)
            {
                for (var x = 0; x < canvas.Width; x++)
                {
                    var index = canvas.GetIndex(x, y);
                    var alpha = canvas.GetAlpha(x, y);
                    var glow = canvas.GetGlow(x, y);
                    var rgb = palette[index];

                    // Nudge emissive pixels toward white in the flat preview so glow placement
                    // is visible at a glance; the real bloom/additive look is a shader concern.
                    var r = (byte)Mathf.Min(255, rgb.R + glow / 3);
                    var g = (byte)Mathf.Min(255, rgb.G + glow / 3);
                    var b = (byte)Mathf.Min(255, rgb.B + glow / 3);

                    // Texture2D row 0 is the bottom row; the canvas's row 0 is the top, so flip.
                    var destY = canvas.Height - 1 - y;
                    pixels[destY * canvas.Width + x] = new Color32(r, g, b, alpha);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            return texture;
        }

        private void BakeAllValidShips()
        {
            var bakedCount = 0;
            foreach (var entry in _genomes)
            {
                if (entry.Genome != null && entry.Validation.IsValid)
                {
                    BakeShip(entry);
                    bakedCount++;
                }
            }

            _statusMessage = $"Baked {bakedCount} ship texture(s) to {GeneratedShipsAssetDir}.";
        }

        private void BakeShip(GenomeEntry entry)
        {
            if (entry.Genome == null || !entry.Validation.IsValid)
            {
                _statusMessage = $"Refusing to bake '{entry.FileName}': genome failed validation.";
                return;
            }

            byte[] png;
            try
            {
                var canvas = ShipRenderer.RenderFinal(entry.Genome);
                png = PngEncoder.Encode(canvas);
            }
            catch (Exception ex)
            {
                _statusMessage = $"Render/encode failed for '{entry.Genome.Id}': {ex.Message}";
                return;
            }

            var projectRoot = Directory.GetParent(Application.dataPath)!.FullName;
            var absoluteDir = Path.Combine(projectRoot, ToNativePath(GeneratedShipsAssetDir));
            Directory.CreateDirectory(absoluteDir);

            var assetPath = $"{GeneratedShipsAssetDir}/{entry.Genome.Id}.png";
            var absolutePath = Path.Combine(absoluteDir, $"{entry.Genome.Id}.png");
            File.WriteAllBytes(absolutePath, png);

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            ConfigureDataTextureImporter(assetPath);
            _statusMessage = $"Baked {assetPath}.";
        }

        private void BakePaletteLut()
        {
            var validPalettes = _palettes
                .Where(p => p.Palette != null && p.Validation.IsValid)
                .Select(p => p.Palette!)
                .ToList();

            if (validPalettes.Count == 0)
            {
                _statusMessage = "No valid palettes to bake into the LUT.";
                return;
            }

            var png = PaletteLutBaker.BuildPng(validPalettes);

            var projectRoot = Directory.GetParent(Application.dataPath)!.FullName;
            var absoluteDir = Path.Combine(projectRoot, "Assets", "Art", "Generated");
            Directory.CreateDirectory(absoluteDir);

            var absolutePath = Path.Combine(absoluteDir, "PaletteLut.png");
            File.WriteAllBytes(absolutePath, png);

            AssetDatabase.ImportAsset(GeneratedPaletteLutAssetPath, ImportAssetOptions.ForceUpdate);
            ConfigureDataTextureImporter(GeneratedPaletteLutAssetPath);

            _statusMessage = $"Baked {GeneratedPaletteLutAssetPath} ({validPalettes.Count} palette row(s), one row per faction/cosmetic skin).";
        }

        /// <summary>
        /// These sprites/LUTs encode data (palette index / glow / alpha), not colour to display
        /// directly - so they must not be gamma-corrected, compressed (which would corrupt exact
        /// index values), mip-mapped (which blends indices across mip levels), or filtered
        /// (which blends indices across pixels). See ADR 0006.
        /// </summary>
        private static void ConfigureDataTextureImporter(string assetPath)
        {
            if (!(AssetImporter.GetAtPath(assetPath) is TextureImporter importer))
            {
                return;
            }

            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = false;
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapEnabled = false;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        private static string? FindRepoRoot()
        {
            var directory = new DirectoryInfo(Application.dataPath);
            while (directory != null)
            {
                if (File.Exists(Path.Combine(directory.FullName, RepoRootMarkerFile)))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            return null;
        }

        private static string ToNativePath(string forwardSlashPath) => forwardSlashPath.Replace('/', Path.DirectorySeparatorChar);
    }
}
