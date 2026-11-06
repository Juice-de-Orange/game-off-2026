using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GameOff.EditorTools
{
    /// <summary>
    /// Integrates asset-pipeline deliveries after a merge (docs/ASSETS.md, "Integration nach dem Merge"):
    /// texture import (Point, no mipmaps), one URP Lit material per FBX slot, slot remaps, humanoid rigs.
    /// Idempotent: slots that already have a remap and textures that are already configured are skipped.
    /// Material naming: first slot = M_&lt;Name&gt;, further slots = M_&lt;Name&gt;_&lt;Slot&gt;, slot "Glass" = shared M_Station_Glass.
    /// </summary>
    public static class DeliveryIntegrator
    {
        private const string ModelRoot = "Assets/_Project/Art/Models";
        private const string TextureRoot = "Assets/_Project/Art/Textures";
        private const string MaterialRoot = "Assets/_Project/Art/Materials";
        private const string UiRoot = "Assets/_Project/Art/UI";
        private const string SkyTextureFolder = TextureRoot + "/Sky"; // configured by SkyExteriorBuilder
        private const string TemplateMaterialPath = MaterialRoot + "/Diner/M_Diner_Counter.mat";
        private const string GlassMaterialPath = MaterialRoot + "/Station/M_Station_Glass.mat";
        public const string HabMaterialPath = MaterialRoot + "/Exterior/M_Ext_HabModule.mat";
        private static readonly Color EmissionColor = new Color(1.5f, 1.5f, 1.5f, 1.5f);
        private static readonly HashSet<string> LoopingTakes = new HashSet<string> { "Idle", "Walk", "Talk" };

        [MenuItem("GameOff/Integrate Asset Deliveries")]
        public static void RunFromMenu() => Debug.Log(Run());

        /// <summary>Runs the integration and returns a human-readable report.</summary>
        public static string Run()
        {
            var report = new List<string>();
            ConfigureWorldTextures(report);
            ConfigureUiTextures(report);
            var textures = IndexTextures();
            foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { ModelRoot }))
                IntegrateModel(AssetDatabase.GUIDToAssetPath(guid), textures, report);
            AssetDatabase.SaveAssets();
            return report.Count == 0 ? "DeliveryIntegrator: nothing to do." : "DeliveryIntegrator:\n" + string.Join("\n", report);
        }

        private static void ConfigureWorldTextures(List<string> report)
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { TextureRoot }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.StartsWith(SkyTextureFolder + "/")) continue;
                if (!(AssetImporter.GetAtPath(path) is TextureImporter ti)) continue;
                if (ti.filterMode == FilterMode.Point && !ti.mipmapEnabled) continue;
                ti.filterMode = FilterMode.Point;
                ti.mipmapEnabled = false;
                ti.SaveAndReimport();
                report.Add("texture  " + path);
            }
        }

        /// <summary>UI graphics and documents: full resolution, bilinear (docs/STYLE.md).</summary>
        private static void ConfigureUiTextures(List<string> report)
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { UiRoot }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!(AssetImporter.GetAtPath(path) is TextureImporter ti)) continue;
                if (ti.textureType == TextureImporterType.Sprite && ti.filterMode == FilterMode.Bilinear && !ti.mipmapEnabled
                    && ti.npotScale == TextureImporterNPOTScale.None) continue;
                ti.textureType = TextureImporterType.Sprite;
                ti.spriteImportMode = SpriteImportMode.Single;
                ti.filterMode = FilterMode.Bilinear;
                ti.mipmapEnabled = false;
                ti.npotScale = TextureImporterNPOTScale.None;
                ti.alphaIsTransparency = true;
                ti.SaveAndReimport();
                report.Add("ui       " + path);
            }
        }

        private static Dictionary<string, Texture2D> IndexTextures()
        {
            var index = new Dictionary<string, Texture2D>();
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { TextureRoot }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                index[Path.GetFileNameWithoutExtension(path)] = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
            return index;
        }

        private static bool SharesHabMaterial(string file) => file == "SM_Ext_HabModule_Entry" || file.StartsWith("SM_Ext_HabTube_");

        private static void IntegrateModel(string path, Dictionary<string, Texture2D> textures, List<string> report)
        {
            if (!(AssetImporter.GetAtPath(path) is ModelImporter importer)) return;
            string file = Path.GetFileNameWithoutExtension(path);
            string name = StripPrefix(file);
            string area = Path.GetFileName(Path.GetDirectoryName(path));
            bool changed = false;

            var remapped = new HashSet<string>(importer.GetExternalObjectMap()
                .Where(kv => kv.Key.type == typeof(Material) && kv.Value != null)
                .Select(kv => kv.Key.name));
            var slots = SlotNames(path);
            // These use the atlas of the module they belong to instead of their own textures.
            Material shared = SharesHabMaterial(file) ? AssetDatabase.LoadAssetAtPath<Material>(HabMaterialPath) : null;
            if (SharesHabMaterial(file) && shared == null)
            {
                report.Add($"WARNING  {file}: {HabMaterialPath} doesn't exist yet, it comes with SM_Ext_HabModule. Run this again.");
                return;
            }
            // Some textures leave out the size in the model's name: SM_Hall_WallEnd_1x6 uses T_Hall_WallEnd_Albedo.
            string unsized = System.Text.RegularExpressions.Regex.Replace(name, @"_\d+x\d+$", "");
            string textureName = textures.ContainsKey("T_" + name + "_Albedo") ? name : unsized; // albedo and emission always as a pair
            textures.TryGetValue("T_" + textureName + "_Albedo", out var albedo);
            textures.TryGetValue("T_" + textureName + "_Emission", out var emission);

            // Main slot = first non-glass slot; it gets M_<Name>, every other slot M_<Name>_<Slot>.
            string mainSlot = slots.FirstOrDefault(s => s != "Glass");
            var externalMap = importer.GetExternalObjectMap();
            foreach (string slot in slots)
            {
                if (remapped.Contains(slot))
                {
                    // Rework deliveries can add an emission map later: give the existing main material that map too.
                    if (slot == mainSlot && emission != null
                        && externalMap.FirstOrDefault(kv => kv.Key.type == typeof(Material) && kv.Key.name == slot).Value is Material existing
                        && existing.GetTexture("_EmissionMap") != emission)
                    {
                        ApplyEmission(existing, emission);
                        EditorUtility.SetDirty(existing);
                        report.Add($"emission {file}.{slot} -> {existing.name}");
                    }
                    continue;
                }
                if (slot != "Glass" && albedo == null && shared == null)
                {
                    // Leave the slot unmapped so a later run picks it up once the texture has arrived.
                    report.Add($"WARNING  {file}.{slot}: no texture T_{textureName}_Albedo yet, slot left unmapped");
                    continue;
                }
                var mat = slot == "Glass"
                    ? AssetDatabase.LoadAssetAtPath<Material>(GlassMaterialPath)
                    : shared != null ? shared : GetOrCreateMaterial($"{MaterialRoot}/{area}/M_{name}{(slot == mainSlot ? "" : "_" + slot)}.mat", albedo, emission, report);
                if (mat == null) { report.Add($"ERROR    {file}: no material for slot {slot}"); continue; }
                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), slot), mat);
                report.Add($"remap    {file}.{slot} -> {mat.name}");
                changed = true;
            }

            if (file.StartsWith("SK_") && importer.animationType != ModelImporterAnimationType.Human)
            {
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                report.Add($"rig      {file}: Humanoid, avatar from this model");
                changed = true;
            }

            if (changed) importer.SaveAndReimport();

            // Pipeline extra bones (Ext_*) are animated by the clips, not by the humanoid. Auto-mapping sometimes grabs them
            // for optional face bones (e.g. Xirrik: Ext_Mandible_L as Jaw and LeftEye), which makes the avatar invalid.
            if (file.StartsWith("SK_") && importer.animationType == ModelImporterAnimationType.Human)
            {
                var description = importer.humanDescription;
                var human = description.human;
                var kept = human.Where(h => !h.boneName.StartsWith("Ext_")).ToArray();
                if (kept.Length != human.Length)
                {
                    description.human = kept;
                    importer.humanDescription = description;
                    importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                    importer.SaveAndReimport();
                    report.Add($"rig      {file}: unmapped {string.Join(", ", human.Where(h => h.boneName.StartsWith("Ext_")).Select(h => h.humanName + "=" + h.boneName))}");
                }
            }

            // Clip settings need the takes of the (re)imported humanoid model.
            if (file.StartsWith("SK_") && importer.clipAnimations.Length == 0 && importer.defaultClipAnimations.Length > 0)
            {
                var clips = importer.defaultClipAnimations;
                // The pipeline delivers Idle/Walk/Talk as clean loops (M012); any other take stays one-shot.
                foreach (var clip in clips) clip.loopTime = LoopingTakes.Contains(clip.takeName);
                importer.clipAnimations = clips;
                SetFullTransformMask(importer, AssetDatabase.LoadAssetAtPath<GameObject>(path).transform);
                importer.SaveAndReimport();
                report.Add($"clips    {file}: {string.Join(", ", clips.Select(c => c.name))} (loop, Ext_ bones kept)");
            }
            if (file.StartsWith("SK_"))
            {
                var avatar = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Avatar>().FirstOrDefault();
                if (avatar == null || !avatar.isValid || !avatar.isHuman)
                    report.Add($"ERROR    {file}: humanoid avatar invalid");
            }
        }

        /// <summary>
        /// Gives every clip a "Create From This Model" mask with all transforms active, so humanoid clips keep the
        /// transform curves of the non-humanoid Ext_* bones (crest, antennae, jaw, tail, throat sac).
        /// ModelImporterClipAnimation exposes no setter for the transform mask, hence SerializedObject.
        /// </summary>
        private static void SetFullTransformMask(ModelImporter importer, Transform root)
        {
            var paths = new List<string>();
            CollectPaths(root, "", paths);
            var so = new SerializedObject(importer);
            var clips = so.FindProperty("m_ClipAnimations");
            for (int c = 0; c < clips.arraySize; c++)
            {
                var clip = clips.GetArrayElementAtIndex(c);
                clip.FindPropertyRelative("maskType").intValue = (int)ClipAnimationMaskType.CreateFromThisModel;
                clip.FindPropertyRelative("maskSource").objectReferenceValue = null;
                var mask = clip.FindPropertyRelative("transformMask");
                mask.arraySize = paths.Count;
                for (int i = 0; i < paths.Count; i++)
                {
                    var entry = mask.GetArrayElementAtIndex(i);
                    entry.FindPropertyRelative("m_Path").stringValue = paths[i];
                    entry.FindPropertyRelative("m_Weight").floatValue = 1f;
                }
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CollectPaths(Transform t, string path, List<string> paths)
        {
            paths.Add(path);
            foreach (Transform child in t)
                CollectPaths(child, path.Length == 0 ? child.name : path + "/" + child.name, paths);
        }

        /// <summary>Material slot names in renderer order, as imported from the FBX.</summary>
        private static List<string> SlotNames(string path)
        {
            var importer = (ModelImporter)AssetImporter.GetAtPath(path);
            var map = importer.GetExternalObjectMap();
            var names = new List<string>();
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (root == null) return names;
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            foreach (var m in r.sharedMaterials)
            {
                if (m == null) continue;
                // A remapped slot shows the external material's name; translate it back to the slot name.
                string slot = map.FirstOrDefault(kv => kv.Value == m).Key.name ?? m.name;
                if (!names.Contains(slot)) names.Add(slot);
            }
            return names;
        }

        private static Material GetOrCreateMaterial(string path, Texture2D albedo, Texture2D emission, List<string> report)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;
            var template = AssetDatabase.LoadAssetAtPath<Material>(TemplateMaterialPath);
            if (template == null) { report.Add("ERROR    template material missing: " + TemplateMaterialPath); return null; }

            var mat = new Material(template) { name = Path.GetFileNameWithoutExtension(path) };
            mat.SetColor("_BaseColor", Color.white);
            mat.SetTexture("_BaseMap", albedo);
            mat.SetTexture("_MainTex", albedo); // the template's own texture must not leak into the copy
            if (emission != null)
            {
                ApplyEmission(mat, emission);
            }
            else
            {
                mat.SetTexture("_EmissionMap", null);
                mat.SetColor("_EmissionColor", Color.black);
                mat.DisableKeyword("_EMISSION");
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            }
            AssetFolders.EnsureForAsset(path);
            AssetDatabase.CreateAsset(mat, path);
            report.Add("material " + path + (emission != null ? " (+emission)" : ""));
            return mat;
        }

        private static void ApplyEmission(Material mat, Texture2D emission)
        {
            mat.SetTexture("_EmissionMap", emission);
            mat.SetColor("_EmissionColor", EmissionColor);
            mat.EnableKeyword("_EMISSION");
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
        }

        private static string StripPrefix(string file)
        {
            int i = file.IndexOf('_');
            return i > 0 && (file.StartsWith("SM_") || file.StartsWith("SK_")) ? file.Substring(i + 1) : file;
        }
    }
}
