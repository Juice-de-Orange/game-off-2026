using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameOff.Day;
using GameOff.Travelers;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GameOff.EditorTools
{
    /// <summary>
    /// P108: Data/Travelers/TravelerLooks.asset from the delivered textures and hair, and one passport photo per combination rendered from
    /// the real figure into Art/Textures/Documents/Photos. Menu: GameOff/Build Traveler Photos (opens an empty scene; run before Build Story Characters).
    /// </summary>
    public static class TravelerPhotoBuilder
    {
        private const string AssetPath = "Assets/_Project/Data/Travelers/TravelerLooks.asset";
        private const string PhotoFolder = "Assets/_Project/Art/Textures/Documents/Photos/";
        private const string TextureFolder = "Assets/_Project/Art/Textures/Characters/";
        private const string HairFolder = "Assets/_Project/Art/Models/Characters/";
        private const string PoolPath = "Assets/_Project/Data/Days/DayPool.asset";
        private static readonly string[] HairNames = { "SM_NPC_Hair_Short", "SM_NPC_Hair_Long", "SM_NPC_Hair_Bun", "SM_NPC_Hat_Cap", "SM_NPC_Hat_Beanie" };
        /// <summary>Measured on the rendered photos, same order as HairNames.</summary>
        private static readonly float[] HairHeights = { 0.03f, -0.08f, 0.05f, 0.17f, 0.13f };
        private const int PhotoWidth = 128, PhotoHeight = 160;

        [MenuItem("GameOff/Build Traveler Photos")]
        public static void BuildFromMenu() => Debug.Log(Build());

        public static string Build()
        {
            var looks = AssetDatabase.LoadAssetAtPath<TravelerLooks>(AssetPath);
            if (looks == null)
            {
                looks = ScriptableObject.CreateInstance<TravelerLooks>();
                AssetFolders.EnsureForAsset(AssetPath);
                AssetDatabase.CreateAsset(looks, AssetPath);
            }
            var so = new SerializedObject(looks);
            var speciesList = so.FindProperty("species");
            speciesList.arraySize = TravelerPrefabBuilder.Species.Length;
            for (int i = 0; i < TravelerPrefabBuilder.Species.Length; i++)
            {
                string name = TravelerPrefabBuilder.Species[i];
                var entry = speciesList.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("species").stringValue = name;
                var skins = entry.FindPropertyRelative("skins");
                var found = new List<Texture2D> { Load<Texture2D>($"{TextureFolder}T_NPC_{name}_Albedo.png") };
                for (int v = 1; v <= 5; v++)
                {
                    var t = Load<Texture2D>($"{TextureFolder}T_NPC_{name}_Albedo_V{v}.png");
                    if (t != null) found.Add(t);
                }
                skins.arraySize = found.Count;
                for (int s = 0; s < found.Count; s++) skins.GetArrayElementAtIndex(s).objectReferenceValue = found[s];
            }
            var hairList = so.FindProperty("hairs");
            var hairs = HairNames.Select(h => Load<GameObject>($"{HairFolder}{h}.fbx")).Where(g => g != null).ToArray();
            hairList.arraySize = hairs.Length;
            for (int i = 0; i < hairs.Length; i++) hairList.GetArrayElementAtIndex(i).objectReferenceValue = hairs[i];
            var heights = so.FindProperty("hairHeights");
            heights.arraySize = HairHeights.Length;
            for (int i = 0; i < HairHeights.Length; i++) heights.GetArrayElementAtIndex(i).floatValue = HairHeights[i];
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var lightGo = new GameObject("Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.transform.rotation = Quaternion.Euler(25f, -30f, 0f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.55f, 0.55f, 0.58f);
            var camGo = new GameObject("Camera");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.32f, 0.36f, 0.42f);
            cam.fieldOfView = 24f;
            cam.nearClipPlane = 0.05f;
            cam.aspect = PhotoWidth / (float)PhotoHeight;
            var target = new RenderTexture(PhotoWidth, PhotoHeight, 24);
            cam.targetTexture = target;
            Directory.CreateDirectory(PhotoFolder);

            var rendered = new List<string>();
            foreach (var species in looks.Species)
            {
                var prefab = Load<GameObject>($"{TravelerPrefabBuilder.PrefabFolder}PF_NPC_{species.species}.prefab");
                if (prefab == null) continue;
                bool human = species.species == "Human";
                var figure = Object.Instantiate(prefab);
                figure.transform.position = Vector3.zero;
                figure.transform.rotation = Quaternion.Euler(0f, 180f, 0f); // faces the camera at -Z
                for (int skin = 0; skin < species.skins.Length; skin++)
                    for (int hair = 0; hair <= (human ? looks.HairCount : 0); hair++)
                    {
                        var look = new TravelerLooks.Look(skin, hair);
                        looks.Apply(figure, species.species, look);
                        var bounds = new Bounds(figure.transform.position + Vector3.up, Vector3.zero);
                        foreach (var r in figure.GetComponentsInChildren<Renderer>()) bounds.Encapsulate(r.bounds);
                        float top = bounds.max.y;
                        float headHeight = bounds.size.y * 0.34f; // head and shoulders
                        var centre = new Vector3(bounds.center.x, top - headHeight * 0.5f, 0f);
                        float distance = headHeight * 0.55f / Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
                        cam.transform.SetPositionAndRotation(centre + Vector3.back * distance, Quaternion.identity);
                        cam.Render();
                        RenderTexture.active = target;
                        var image = new Texture2D(PhotoWidth, PhotoHeight, TextureFormat.RGB24, false);
                        image.ReadPixels(new Rect(0, 0, PhotoWidth, PhotoHeight), 0, 0);
                        image.Apply();
                        string path = PhotoFolder + TravelerLooks.PhotoName(species.species, look) + ".png";
                        File.WriteAllBytes(path, image.EncodeToPNG());
                        Object.DestroyImmediate(image);
                        rendered.Add(path);
                    }
                Object.DestroyImmediate(figure);
            }
            RenderTexture.active = null;
            AssetDatabase.Refresh();
            var textures = new List<Texture2D>();
            foreach (string path in rendered)
            {
                if (AssetImporter.GetAtPath(path) is TextureImporter ti && (ti.filterMode != FilterMode.Point || ti.mipmapEnabled))
                {
                    ti.filterMode = FilterMode.Point;
                    ti.mipmapEnabled = false;
                    ti.SaveAndReimport();
                }
                textures.Add(Load<Texture2D>(path));
            }
            so.Update();
            var photos = so.FindProperty("photos");
            photos.arraySize = textures.Count;
            for (int i = 0; i < textures.Count; i++) photos.GetArrayElementAtIndex(i).objectReferenceValue = textures[i];
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(looks);
            var pool = Load<DayPool>(PoolPath);
            if (pool != null)
            {
                var pso = new SerializedObject(pool);
                pso.FindProperty("looks").objectReferenceValue = looks;
                pso.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(pool);
            }
            AssetDatabase.SaveAssets();
            return $"[TravelerPhotoBuilder] {rendered.Count} photos";
        }

        private static T Load<T>(string path) where T : Object => AssetDatabase.LoadAssetAtPath<T>(path);
    }
}
